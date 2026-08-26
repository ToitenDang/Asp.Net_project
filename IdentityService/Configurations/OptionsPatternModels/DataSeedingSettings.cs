namespace IdentityService.Configurations.OptionsPatternModels
{
    public class DataSeedingSettings
    {
        public AdminAccount AdminAccount { get; set; } = new AdminAccount();
        public RoleSettings RoleSettings { get; set; } = new RoleSettings();
    }

    public class AdminAccount
    {
        public string Name { get; set; } = "Admin";
        public string UserName { get; set; } = "admin";
        public string Password { get; set; } = "Admin@123";
        public string Email { get; set; } = "admin@system.com";
    }

    public class RoleSettings
    {
        public string RoleAdminName { get; set; } = "ADMIN";
        public string RoleAdminCode { get; set; } = "ADMIN";
        public string RoleUserName { get; set; } = "USER";
        public string RoleUserCode { get; set; } = "USER";
    }
}