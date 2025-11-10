namespace Chat.Routes;

public static class ApiRoutes
{
    public const string Base = "api";
    public static class Friends
    {
        public const string GetAll = $"all";
        public const string Delete = $"{{friendId}}";
    }

    public static class FriendRequests
    {
        public const string GetAll = "list";
        public const string Send = "send";
        public const string Accept = $"{{id}}/accept";
        public const string Delete = $"{{id}}";
    }

    public static class Messages
    {
        public const string All = "all";
    }

    public static class Users
    {
        public const string GetUserbyUserName = "{username}";
    }

}