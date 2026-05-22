namespace Domain.SubscriptionFeatures;

public static class SubscriptionLimit
{
    public const string MaxUsers = "limit:max_users";
    public const string MaxTodos = "limit:max_todos";
    public const string StorageMb = "limit:storage_mb";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        MaxUsers, MaxTodos, StorageMb
    };

    public const int Unlimited = -1;
}
