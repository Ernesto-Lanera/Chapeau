namespace Chapeau.ViewModels
{
    public class RolePermissionsIndexViewModel
    {
        public List<RolePermissionRowViewModel> Roles { get; set; } = new List<RolePermissionRowViewModel>();
    }

    public class RolePermissionRowViewModel
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new List<string>();
    }

    public class RolePermissionsEditViewModel
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public List<string> AllPermissions { get; set; } = new List<string>();
        public List<string> CurrentPermissions { get; set; } = new List<string>();
    }

    public class RolePermissionsSaveResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public List<string> SavedPermissions { get; set; } = new List<string>();
    }
}
