using Application.Abstractions.Data;
using Application.SubscriptionFeatures.GetPricing;
using Application.UnitTests.TestHelpers;
using Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using SharedKernel;

namespace Application.UnitTests.SubscriptionFeatures.GetPricing;

public sealed class GetPricingQueryHandlerTests
{
    private readonly IApplicationDbContext _context = Substitute.For<IApplicationDbContext>();
    private readonly GetPricingQueryHandler _handler;

    public GetPricingQueryHandlerTests()
    {
        _handler = new GetPricingQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldReturnPricingList()
    {
        List<SubscriptionPlan> plans =
        [
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(), Name = "Free", PriceMonthly = 0m, PriceYearly = 0m,
                SortOrder = 0, IsActive = true, CreatedAt = DateTime.UtcNow
            },
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(), Name = "Pro", PriceMonthly = 29.99m, PriceYearly = 299.99m,
                SortOrder = 1, IsActive = true, CreatedAt = DateTime.UtcNow
            },
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(), Name = "Enterprise", PriceMonthly = 99.99m, PriceYearly = 999.99m,
                SortOrder = 2, IsActive = true, CreatedAt = DateTime.UtcNow
            }
        ];

        DbSet<SubscriptionPlan> dbSet = CreateAsyncMockDbSet(plans);
        _context.SubscriptionPlans.Returns(dbSet);

        Result<List<PricingResponse>> result = await _handler.Handle(new GetPricingQuery(), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(5);
        result.Value.ShouldContain(p => p.Plan == "Pro");
        result.Value.ShouldContain(p => p.Plan == "Enterprise");
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectFreePlanAmount()
    {
        List<SubscriptionPlan> plans =
        [
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(), Name = "Free", PriceMonthly = 0m, PriceYearly = 0m,
                SortOrder = 0, IsActive = true, CreatedAt = DateTime.UtcNow
            }
        ];

        DbSet<SubscriptionPlan> dbSet = CreateAsyncMockDbSet(plans);
        _context.SubscriptionPlans.Returns(dbSet);

        Result<List<PricingResponse>> result = await _handler.Handle(new GetPricingQuery(), CancellationToken.None);

        PricingResponse? freePlan = result.Value.FirstOrDefault(p => p.Plan == "Free");
        freePlan.ShouldNotBeNull();
        freePlan.Amount.ShouldBe(0m);
    }

    private static DbSet<TEntity> CreateAsyncMockDbSet<TEntity>(List<TEntity> data) where TEntity : class
    {
        TestAsyncEnumerable<TEntity> asyncEnumerable = new(data);
        IQueryable<TEntity> queryable = asyncEnumerable;
        DbSet<TEntity> dbSet = Substitute.For<DbSet<TEntity>, IQueryable<TEntity>, IAsyncEnumerable<TEntity>>();

        ((IQueryable<TEntity>)dbSet).Provider.Returns(queryable.Provider);
        ((IQueryable<TEntity>)dbSet).Expression.Returns(queryable.Expression);
        ((IQueryable<TEntity>)dbSet).ElementType.Returns(queryable.ElementType);
        ((IQueryable<TEntity>)dbSet).GetEnumerator().Returns(queryable.GetEnumerator());
        ((IAsyncEnumerable<TEntity>)dbSet).GetAsyncEnumerator(Arg.Any<CancellationToken>())
            .Returns(asyncEnumerable.GetAsyncEnumerator());

        return dbSet;
    }
}
