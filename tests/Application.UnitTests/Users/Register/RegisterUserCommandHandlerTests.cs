using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.UnitTests.TestHelpers;
using Application.Users.Register;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using SharedKernel;

namespace Application.UnitTests.Users.Register;

public sealed class RegisterUserCommandHandlerTests
{
    private readonly IApplicationDbContext _context = Substitute.For<IApplicationDbContext>();
    private readonly UserManager<User> _userManager;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        IUserStore<User> userStore = Substitute.For<IUserStore<User>>();
        _userManager = Substitute.For<UserManager<User>>(userStore, null, null, null, null, null, null, null, null);
        _handler = new RegisterUserCommandHandler(_context, _userManager);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmailNotUnique()
    {
        var command = new RegisterUserCommand("existing@example.com", "John", "Doe", "StrongP@ss1");
        List<User> existingUsers = [new User { Email = "existing@example.com" }];
        DbSet<User> dbSet = CreateAsyncMockDbSet(existingUsers);
        _context.Users.Returns(dbSet);

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(UserErrors.EmailNotUnique);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCreateAsyncFails()
    {
        var command = new RegisterUserCommand("new@example.com", "John", "Doe", "weak");
        List<User> existingUsers = [];
        DbSet<User> dbSet = CreateAsyncMockDbSet(existingUsers);
        _context.Users.Returns(dbSet);

        IdentityError[] errors = [new IdentityError { Code = "WeakPassword", Description = "Password too weak" }];
        _userManager.CreateAsync(Arg.Any<User>(), command.Password)
            .Returns(IdentityResult.Failed(errors));

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("WeakPassword");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUserCreated()
    {
        var command = new RegisterUserCommand("new@example.com", "John", "Doe", "StrongP@ss1");
        List<User> existingUsers = [];
        DbSet<User> dbSet = CreateAsyncMockDbSet(existingUsers);
        _context.Users.Returns(dbSet);

        _userManager.CreateAsync(Arg.Any<User>(), command.Password)
            .Returns(IdentityResult.Success);
        _context.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);
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
