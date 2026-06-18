using System.Data;
using Chapeau.Constants;
using Chapeau.Models;
using Microsoft.Data.SqlClient;

namespace Chapeau.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private const string SelectEmployeeColumns = @"
            SELECT e.EmployeeID, e.Name, e.PasswordHash, e.RoleID, e.IsActive, r.RoleName
            FROM Employee AS e
            INNER JOIN Roles AS r ON r.RoleID = e.RoleID";

        private readonly string _connectionString;
        private readonly ILogger<EmployeeRepository> _logger;

        public EmployeeRepository(IConfiguration configuration, ILogger<EmployeeRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
                ?? throw new InvalidOperationException(ErrorMessages.ConnectionStringMissing);
            _logger = logger;
        }

        public List<Employee> GetEmployees()
        {
            string query = SelectEmployeeColumns + " ORDER BY e.Name;";
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand(query, connection);
            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();
            List<Employee> employees = new List<Employee>();
            while (reader.Read())
            {
                employees.Add(MapEmployee(reader));
            }

            return employees;
        }

        public Employee? GetEmployeeById(int employeeId)
        {
            string query = SelectEmployeeColumns + " WHERE e.EmployeeID = @EmployeeID;";
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = employeeId;
            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapEmployee(reader);
            }

            return null;
        }

        public Employee? GetEmployeeByName(string name)
        {
            string query = SelectEmployeeColumns + " WHERE e.Name = @Name;";
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name;
            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapEmployee(reader);
            }

            return null;
        }

        public async Task<Employee?> GetEmployeeByNameAsync(string name)
        {
            string query = SelectEmployeeColumns + " WHERE e.Name = @Name;";
            await using SqlConnection connection = new SqlConnection(_connectionString);
            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name;
            await connection.OpenAsync();

            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapEmployee(reader);
            }

            return null;
        }

        public bool NameExists(string name, int? excludedEmployeeId = null)
        {
            string query = @"
                SELECT COUNT(1) FROM Employee
                WHERE LOWER(Name) = LOWER(@Name)
                  AND (@ExcludedEmployeeID IS NULL OR EmployeeID <> @ExcludedEmployeeID);";

            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name;
            command.Parameters.Add("@ExcludedEmployeeID", SqlDbType.Int).Value =
                excludedEmployeeId.HasValue ? excludedEmployeeId.Value : DBNull.Value;
            connection.Open();

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        public void AddEmployee(Employee employee)
        {
            string query = @"
                INSERT INTO Employee (Name, PasswordHash, RoleID, IsActive)
                OUTPUT INSERTED.EmployeeID
                VALUES (@Name, @PasswordHash, @RoleID, @IsActive);";

            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand(query, connection);
            AddEditableParameters(command, employee);
            connection.Open();

            employee.EmployeeID = Convert.ToInt32(command.ExecuteScalar());
            _logger.LogInformation("Medewerker toegevoegd: {EmployeeId}.", employee.EmployeeID);
        }

        public void UpdateEmployee(Employee employee)
        {
            string query = @"
                UPDATE Employee
                SET Name = @Name, PasswordHash = @PasswordHash, RoleID = @RoleID, IsActive = @IsActive
                WHERE EmployeeID = @EmployeeID;";

            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = employee.EmployeeID;
            AddEditableParameters(command, employee);
            connection.Open();

            EnsureUpdated(command.ExecuteNonQuery(), employee.EmployeeID);
        }

        public void SetEmployeeActive(int employeeId, bool active)
        {
            string query = "UPDATE Employee SET IsActive = @IsActive WHERE EmployeeID = @EmployeeID;";
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = employeeId;
            command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = active;
            connection.Open();

            EnsureUpdated(command.ExecuteNonQuery(), employeeId);
        }

        public bool TestConnection()
        {
            try
            {
                using SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                return true;
            }
            catch (SqlException exception)
            {
                _logger.LogWarning(exception, "Databaseverbinding testen is mislukt.");
                return false;
            }
        }

        private static void AddEditableParameters(SqlCommand command, Employee employee)
        {
            command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = employee.Name;
            command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = employee.PasswordHash;
            command.Parameters.Add("@RoleID", SqlDbType.Int).Value = employee.RoleID;
            command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = employee.IsActive;
        }

        private void EnsureUpdated(int rowsAffected, int employeeId)
        {
            if (rowsAffected > 0)
            {
                return;
            }

            _logger.LogWarning("Medewerker niet gevonden: {EmployeeId}.", employeeId);
            throw new InvalidOperationException(ErrorMessages.EmployeeNotFound);
        }

        private static Employee MapEmployee(SqlDataReader reader)
        {
            int roleId = reader.GetInt32(reader.GetOrdinal("RoleID"));
            return new Employee
            {
                EmployeeID = reader.GetInt32(reader.GetOrdinal("EmployeeID")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                RoleID = roleId,
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                Role = new EmployeeRole
                {
                    RoleID = roleId,
                    RoleName = reader.GetString(reader.GetOrdinal("RoleName"))
                }
            };
        }
    }
}
