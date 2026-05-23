using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Application.Abstractions.Models;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Extensions;

public static class PaginatedQueryExtensions
{
    public static IQueryable<T> ApplySearch<T>(
        this IQueryable<T> query,
        string? searchTerm,
        IReadOnlyDictionary<string, Expression<Func<T, string>>> searchMap)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || searchMap.Count == 0)
        {
            return query;
        }

        ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
        string escaped = searchTerm
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
        string pattern = $"%{escaped.ToUpperInvariant()}%";

        MethodInfo likeMethod = GetLikeMethod();
        MethodInfo toUpperMethod = typeof(string).GetMethod(nameof(string.ToUpper), Type.EmptyTypes)!;
        ConstantExpression efFunctions = Expression.Constant(EF.Functions);

        Expression? combined = null;

        foreach (Expression<Func<T, string>> fieldSelector in searchMap.Values)
        {
            Expression fieldAccess = BuildAccessor(fieldSelector, parameter);
            Expression upperFieldAccess = Expression.Call(fieldAccess, toUpperMethod);
            Expression likeCall = Expression.Call(
                likeMethod,
                efFunctions,
                upperFieldAccess,
                Expression.Constant(pattern));

            combined = combined is null ? likeCall : Expression.OrElse(combined, likeCall);
        }

        if (combined is null)
        {
            return query;
        }

        var lambda = Expression.Lambda<Func<T, bool>>(combined, parameter);
        return query.Where(lambda);
    }

    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> query,
        IReadOnlyList<SortParam> sorts,
        IReadOnlyDictionary<string, Expression<Func<T, object>>> sortMap,
        Expression<Func<T, object>>? defaultSort = null,
        SortDirection defaultDirection = SortDirection.Desc)
    {
        bool isFirst = true;

        if (sorts is { Count: > 0 })
        {
            foreach (SortParam sort in sorts)
            {
                if (!sortMap.TryGetValue(sort.Column, out Expression<Func<T, object>>? selector))
                {
                    continue;
                }

                query = ApplyOrder(query, selector, sort.Direction, isFirst);
                isFirst = false;
            }
        }

        if (isFirst && defaultSort is not null)
        {
            query = ApplyOrder(query, defaultSort, defaultDirection, isFirst);
        }

        return query;
    }

    public static IQueryable<T> ApplyFilters<T>(
        this IQueryable<T> query,
        IReadOnlyList<FilterParam> filters,
        IReadOnlyDictionary<string, Expression<Func<T, object>>> filterMap)
    {
        if (filters is not { Count: > 0 })
        {
            return query;
        }

        ParameterExpression parameter = Expression.Parameter(typeof(T), "x");

        foreach (FilterParam filter in filters)
        {
            if (!filterMap.TryGetValue(filter.Column, out Expression<Func<T, object>>? selector))
            {
                continue;
            }

            Expression body = BuildAccessor(selector, parameter);
            Type memberType = GetMemberType(selector);

            Expression? condition = BuildFilterCondition(body, memberType, filter);

            if (condition is null)
            {
                continue;
            }

            var lambda = Expression.Lambda<Func<T, bool>>(condition, parameter);
            query = query.Where(lambda);
        }

        return query;
    }

    public static async Task<PaginatedList<T>> ToPaginatedListAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        int totalCount = await query.CountAsync(cancellationToken);

        List<T> items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public static async Task<KanbanList<TColumn, TItem>> ToKanbanListAsync<T, TColumn, TItem>(
        this IQueryable<T> query,
        Func<T, TColumn> columnKeySelector,
        Func<T, TItem> itemSelector,
        Func<TColumn, string> columnLabelSelector,
        Func<TColumn, int> columnOrderSelector,
        CancellationToken cancellationToken)
    {
        List<T> data = await query.ToListAsync(cancellationToken);

        var columns = data
            .GroupBy(columnKeySelector)
            .Select(group => new KanbanColumn<TColumn, TItem>
            {
                Key = group.Key,
                Label = columnLabelSelector(group.Key),
                Order = columnOrderSelector(group.Key),
                Items = group.Select(itemSelector).ToList()
            })
            .OrderBy(c => c.Order)
            .ToList();

        return new KanbanList<TColumn, TItem>
        {
            Columns = columns
        };
    }

    public static async Task<TreeList<T>> ToTreeListAsync<T>(
        this IQueryable<T> query,
        CancellationToken cancellationToken) where T : ITreeNode<T>
    {
        List<T> items = await query.ToListAsync(cancellationToken);

        var lookup = items.ToDictionary(i => i.Id);
        List<T> roots = [];

        foreach (T item in items)
        {
            if (item.ParentId is { } parentId && lookup.TryGetValue(parentId, out T? parent) && parent is not null)
            {
                parent.Children.Add(item);
            }
            else
            {
                roots.Add(item);
            }
        }

        return new TreeList<T>
        {
            Roots = roots
        };
    }

    private static IQueryable<T> ApplyOrder<T>(
        IQueryable<T> query,
        Expression<Func<T, object>> selector,
        SortDirection direction,
        bool isFirst)
    {
        string methodName = (isFirst, direction) switch
        {
            (true, SortDirection.Asc) => nameof(Queryable.OrderBy),
            (true, SortDirection.Desc) => nameof(Queryable.OrderByDescending),
            (false, SortDirection.Asc) => nameof(Queryable.ThenBy),
            (false, SortDirection.Desc) => nameof(Queryable.ThenByDescending),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Invalid sort direction")
        };

        Type keyType = GetMemberType(selector);
        LambdaExpression keySelector = StripConvert(selector);

        MethodInfo method = typeof(Queryable)
            .GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), keyType);

        return (IQueryable<T>)method.Invoke(null, [query, keySelector])!;
    }

    private static Expression? BuildFilterCondition(
        Expression memberBody,
        Type memberType,
        FilterParam filter)
    {
        if (filter.Operator is FilterOperator.IsNull)
        {
            return Expression.Equal(memberBody, Expression.Constant(null, memberType));
        }

        if (filter.Operator is FilterOperator.IsNotNull)
        {
            return Expression.NotEqual(memberBody, Expression.Constant(null, memberType));
        }

        if (filter.Operator is FilterOperator.Contains or FilterOperator.StartsWith or FilterOperator.EndsWith)
        {
            if (filter.Value is null)
            {
                return null;
            }

            if (memberType != typeof(string))
            {
                return null;
            }

            string methodName = filter.Operator switch
            {
                FilterOperator.Contains => nameof(string.Contains),
                FilterOperator.StartsWith => nameof(string.StartsWith),
                FilterOperator.EndsWith => nameof(string.EndsWith),
                _ => throw new ArgumentOutOfRangeException(nameof(filter), filter.Operator, "Unexpected string operator")
            };

            MethodInfo method = typeof(string).GetMethod(
                methodName,
                [typeof(string)])!;

            return Expression.Call(
                memberBody,
                method,
                Expression.Constant(filter.Value, typeof(string)));
        }

        if (filter.Operator is FilterOperator.Between)
        {
            object? valueFrom = ParseFilterValue(filter.Value, memberType);
            object? valueTo = ParseFilterValue(filter.ValueTo, memberType);

            if (valueFrom is null || valueTo is null)
            {
                return null;
            }

            Expression constantFrom = Expression.Constant(valueFrom, memberType);
            Expression constantTo = Expression.Constant(valueTo, memberType);

            return Expression.AndAlso(
                Expression.GreaterThanOrEqual(memberBody, constantFrom),
                Expression.LessThanOrEqual(memberBody, constantTo));
        }

        object? parsedValue = ParseFilterValue(filter.Value, memberType);

        if (parsedValue is null && filter.Operator != FilterOperator.Equals)
        {
            return null;
        }

        if (parsedValue is null && filter.Operator == FilterOperator.Equals)
        {
            return Expression.Equal(memberBody, Expression.Constant(null, memberType));
        }

        Expression constant = Expression.Constant(parsedValue, memberType);

        return filter.Operator switch
        {
            FilterOperator.Equals => Expression.Equal(memberBody, constant),
            FilterOperator.NotEquals => Expression.NotEqual(memberBody, constant),
            FilterOperator.GreaterThan => Expression.GreaterThan(memberBody, constant),
            FilterOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(memberBody, constant),
            FilterOperator.LessThan => Expression.LessThan(memberBody, constant),
            FilterOperator.LessThanOrEqual => Expression.LessThanOrEqual(memberBody, constant),
            _ => null
        };
    }

    private static Expression BuildAccessor(LambdaExpression selector, ParameterExpression newParameter)
    {
        Expression body = selector.Body;

        if (body is UnaryExpression { NodeType: ExpressionType.Convert } unary)
        {
            body = unary.Operand;
        }

        return new ParameterReplacer(selector.Parameters[0], newParameter).Visit(body);
    }

    private static LambdaExpression StripConvert<T>(Expression<Func<T, object>> selector)
    {
        if (selector.Body is UnaryExpression { NodeType: ExpressionType.Convert } unary)
        {
            return Expression.Lambda(unary.Operand, selector.Parameters);
        }

        return selector;
    }

    private static Type GetMemberType<T>(Expression<Func<T, object>> selector)
    {
        Expression body = selector.Body;

        if (body is UnaryExpression { NodeType: ExpressionType.Convert } unary)
        {
            return unary.Operand.Type;
        }

        return body.Type;
    }

    private static object? ParseFilterValue(string? value, Type targetType)
    {
        if (value is null)
        {
            return null;
        }

        Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlyingType.IsEnum)
        {
            return Enum.Parse(underlyingType, value, ignoreCase: true);
        }

        if (underlyingType == typeof(string))
        {
            return value;
        }

        if (underlyingType == typeof(Guid))
        {
            return Guid.Parse(value);
        }

        if (underlyingType == typeof(int))
        {
            return int.Parse(value, CultureInfo.InvariantCulture);
        }

        if (underlyingType == typeof(long))
        {
            return long.Parse(value, CultureInfo.InvariantCulture);
        }

        if (underlyingType == typeof(decimal))
        {
            return decimal.Parse(value, CultureInfo.InvariantCulture);
        }

        if (underlyingType == typeof(double))
        {
            return double.Parse(value, CultureInfo.InvariantCulture);
        }

        if (underlyingType == typeof(float))
        {
            return float.Parse(value, CultureInfo.InvariantCulture);
        }

        if (underlyingType == typeof(DateTime))
        {
            return DateTime.Parse(value, CultureInfo.InvariantCulture);
        }

        if (underlyingType == typeof(DateTimeOffset))
        {
            return DateTimeOffset.Parse(value, CultureInfo.InvariantCulture);
        }

        if (underlyingType == typeof(DateOnly))
        {
            return DateOnly.Parse(value, CultureInfo.InvariantCulture);
        }

        if (underlyingType == typeof(TimeOnly))
        {
            return TimeOnly.Parse(value, CultureInfo.InvariantCulture);
        }

        if (underlyingType == typeof(bool))
        {
            return bool.Parse(value);
        }

        return Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
    }

    private static MethodInfo GetLikeMethod()
    {
        return typeof(DbFunctionsExtensions)
            .GetMethods()
            .First(m => m.Name == nameof(DbFunctionsExtensions.Like)
                        && m.GetParameters().Length == 3);
    }

    private sealed class ParameterReplacer(
        ParameterExpression oldParameter,
        ParameterExpression newParameter) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == oldParameter ? newParameter : base.VisitParameter(node);
        }
    }
}
