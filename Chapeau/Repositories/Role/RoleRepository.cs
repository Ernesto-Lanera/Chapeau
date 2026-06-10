using System.Data;
using Chapeau.Constants;
using Chapeau.Constants.Login;
using Chapeau.Models;
using Microsoft.Data.SqlClient;

namespace Chapeau.Repositories
{
    public class RoleRepository(IConfiguration configuration) : IRoleRepository
    {
        private readonly string _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
            ?? throw new InvalidOperationException(ErrorMessages.ConnectionStringMissing);

        private static readonly IReadOnlyDictionary<string, string> KnownPermissions =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [PermissionConstants.TakeOrders] = "Bestellingen opnemen.",
                [PermissionConstants.PrepareFood] = "Keukenbestellingen verwerken.",
                [PermissionConstants.PrepareDrinks] = "Barbestellingen verwerken.",
                [PermissionConstants.ManageEmployees] = "Medewerkers beheren.",
                [PermissionConstants.ManageMenuItems] = "Menu-items beheren.",
                [PermissionConstants.ManageStock] = "Voorraad beheren.",
                [PermissionConstants.ViewFinance] = "Financieel overzicht bekijken.",
                [PermissionConstants.ManageRoles] = "Rollen en permissies beheren."
            };

        public List<EmployeeRole> GetRoles()
        {
            const string query = "SELECT RoleID, RoleName FROM Roles ORDER BY RoleName;";
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(query, connection);
            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();
            var roles = new List<EmployeeRole>();
            while (reader.Read())
            {
                roles.Add(new EmployeeRole
                {
                    RoleID = reader.GetInt32(reader.GetOrdinal("RoleID")),
                    RoleName = reader.GetString(reader.GetOrdinal("RoleName"))
                });
            }

            return roles;
        }

        public List<string> GetRolePermissions(int roleId)
        {
            const string query = """
                SELECT p.PermissionName
                FROM RolePermissions AS rp
                INNER JOIN Permissions AS p ON p.PermissionID = rp.PermissionID
                WHERE rp.RoleID = @RoleID
                ORDER BY p.PermissionName;
                """;

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(query, connection);
            command.Parameters.Add("@RoleID", SqlDbType.Int).Value = roleId;
            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();
            var permissions = new List<string>();
            while (reader.Read())
            {
                permissions.Add(NormalizePermissionName(reader.GetString(0)));
            }

            return permissions.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(permission => permission).ToList();
        }

        public List<string> GetAllPermissionNames()
        {
            const string query = "SELECT PermissionName FROM Permissions ORDER BY PermissionName;";
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(query, connection);
            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();
            var permissions = new List<string>();
            while (reader.Read())
            {
                permissions.Add(NormalizePermissionName(reader.GetString(0)));
            }

            return permissions
                .Union(KnownPermissions.Keys, StringComparer.OrdinalIgnoreCase)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(permission => permission)
                .ToList();
        }

        public void SetRolePermissions(int roleId, List<string> permissionNames)
        {
            using SqlConnection connection = new(_connectionString);
            connection.Open();
            using SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                DeleteRolePermissions(roleId, connection, transaction);
                foreach (string permissionName in NormalizePermissionNames(permissionNames))
                {
                    EnsurePermissionExists(permissionName, connection, transaction);
                    InsertRolePermission(roleId, permissionName, connection, transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private static void DeleteRolePermissions(int roleId, SqlConnection connection, SqlTransaction transaction)
        {
            const string query = "DELETE FROM RolePermissions WHERE RoleID = @RoleID;";
            using SqlCommand command = new(query, connection, transaction);
            command.Parameters.Add("@RoleID", SqlDbType.Int).Value = roleId;
            command.ExecuteNonQuery();
        }

        private static void EnsurePermissionExists(string permissionName, SqlConnection connection, SqlTransaction transaction)
        {
            const string query = """
                IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionName = @PermissionName)
                BEGIN
                    INSERT INTO Permissions (PermissionName, Description)
                    VALUES (@PermissionName, @Description);
                END;
                """;

            using SqlCommand command = new(query, connection, transaction);
            command.Parameters.Add("@PermissionName", SqlDbType.NVarChar, 100).Value = permissionName;
            command.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value = KnownPermissions[permissionName];
            command.ExecuteNonQuery();
        }

        private static void InsertRolePermission(int roleId, string permissionName, SqlConnection connection, SqlTransaction transaction)
        {
            const string query = """
                INSERT INTO RolePermissions (RoleID, PermissionID)
                SELECT @RoleID, PermissionID
                FROM Permissions
                WHERE PermissionName = @PermissionName;
                """;

            using SqlCommand command = new(query, connection, transaction);
            command.Parameters.Add("@RoleID", SqlDbType.Int).Value = roleId;
            command.Parameters.Add("@PermissionName", SqlDbType.NVarChar, 100).Value = permissionName;
            command.ExecuteNonQuery();
        }

        private static IEnumerable<string> NormalizePermissionNames(IEnumerable<string> permissionNames) =>
            permissionNames
                .Select(NormalizePermissionName)
                .Where(permissionName => KnownPermissions.ContainsKey(permissionName))
                .Distinct(StringComparer.OrdinalIgnoreCase);

        private static string NormalizePermissionName(string permissionName) =>
            string.Equals(permissionName, PermissionConstants.LegacyViewReports, StringComparison.OrdinalIgnoreCase)
                ? PermissionConstants.ViewFinance
                : permissionName;
    }
}
