namespace Game_API.Models
{
    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<User> Users { get; set; } = new List<User>();

        public static class RoleNames
        {
            public const string Admin = "Admin";
            public const string User = "User";
        }
    }
}
