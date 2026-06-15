using System.Data;
using Chapeau.Constants;
using Chapeau.Models;
using Microsoft.Data.SqlClient;

namespace Chapeau.Repositories
{
    /// <summary>
    /// Repository for employee CRUD operations using ADO.NET with SQL Server.
    /// </summary>
    public class EmployeeRepository(IConfiguration configuration, ILogger<EmployeeRepository> logger) : IEmployeeRepository
    {
        private const string SelectEmployeeColumns = """
            SELECT e.EmployeeID, e.Name, e.PasswordHash, e.RoleID, e.IsActive, r.RoleName
            FROM Employee AS e
            INNER JOIN Roles AS r ON r.RoleID = e.RoleID
            """;

        private readonly string _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
            ?? throw new InvalidOperationException(ErrorMessages.ConnectionStringMissing);
        private readonly ILogger<EmployeeRepository> _logger = logger;

        /// <summary>Gets all employees ordered by name.</summary>
        public List<Employee> GetEmployees()
        {
            string query = SelectEmployeeColumns + " ORDER BY e.Name;";
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(query, connection);
            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();
            var employees = new List<Employee>();
            while (reader.Read()) employees.Add(MapEmployee(reader));
            return employees;
        }

        /// <summary>Gets a single employee by ID, or null if not found.</summary>
        public Employee? GetEmployeeById(int employeeId)
        {
            string query = SelectEmployeeColumns + " WHERE e.EmployeeID = @EmployeeID;";
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(query, connection);
            command.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = employeeId;
            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();
            return reader.Read() ? MapEmployee(reader) : null;
        }

        /// <summary>Gets a single employee by name, or null if not found.</summary>
        public Employee? GetEmployeeByName(string name)
        {
            string query = SelectEmployeeColumns + " WHERE e.Name = @Name;";
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(query, connection);
            command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name.Trim();
            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();
            return reader.Read() ? MapEmployee(reader) : null;
        }

        /// <summary>Async version of GetEmployeeByName used during login.</summary>
        public async Task<Employee?> GetEmployeeByNameAsync(string name)
        {
            string query = SelectEmployeeColumns + " WHERE e.Name = @Name;";
            await using SqlConnection connection = new(_connectionString);
            await using SqlCommand command = new(query, connection);
            command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name.Trim();
            await connection.OpenAsync();

            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapEmployee(reader) : null;
        }

        /// <summary>Checks if an employee name already exists in the database, case-insensitive.</summary>
        public bool NameExists(string name, int? excludedEmployeeId = null)
        {
            const string query = """
                SELECT COUNT(1) FROM Employee
                WHERE LOWER(Name) = LOWER(@Name)
                  AND (@ExcludedEmployeeID IS NULL OR EmployeeID <> @ExcludedEmployeeID);
                """;

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(query, connection);
            command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name.Trim();
            command.Parameters.Add("@ExcludedEmployeeID", SqlDbType.Int).Value =
                excludedEmployeeId.HasValue ? excludedEmployeeId.Value : DBNull.Value;
            connection.Open();

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        /// <summary>Adds a new employee and sets the EmployeeID from the database-generated value.</summary>
        public void AddEmployee(Employee employee)
        {
            const string query = """
                INSERT INTO Employee (Name, PasswordHash, RoleID, IsActive)
                OUTPUT INSERTED.EmployeeID
                VALUES (@Name, @PasswordHash, @RoleID, @IsActive);
                """;

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(query, connection);
            AddEditableParameters(command, employee);
            connection.Open();

            employee.EmployeeID = Convert.ToInt32(command.ExecuteScalar());
            _logger.LogInformation("Medewerker toegevoegd: {EmployeeId}.", employee.EmployeeID);
        }

        /// <summary>Updates an existing employee's details.</summary>
        public void UpdateEmployee(Employee employee)
        {
            const string query = """
                UPDATE Employee
                SET Name = @Name, PasswordHash = @PasswordHash, RoleID = @RoleID, IsActive = @IsActive
                WHERE EmployeeID = @EmployeeID;
                """;

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(query, connection);
            command.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = employee.EmployeeID;
            AddEditableParameters(command, employee);
            connection.Open();

            EnsureUpdated(command.ExecuteNonQuery(), employee.EmployeeID);
        }

        /// <summary>Sets the active/inactive status of an employee.</summary>
        public void SetEmployeeActive(int employeeId, bool active)
        {
            const string query = "UPDATE Employee SET IsActive = @IsActive WHERE EmployeeID = @EmployeeID;";
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(query, connection);
            command.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = employeeId;
            command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = active;
            connection.Open();

            EnsureUpdated(command.ExecuteNonQuery(), employeeId);
        }

        /// <summary>Tests whether the database connection is available.</summary>
        public bool TestConnection()
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();
                return true;
            }
            catch (SqlException exception)
            {
                _logger.LogWarning(exception, "Databaseverbinding testen is mislukt.");
                return false;
            }
        }

        /// <summary>Adds the common editable parameters (Name, PasswordHash, RoleID, IsActive) to a command.</summary>
        private static void AddEditableParameters(SqlCommand command, Employee employee)
        {
            command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = employee.Name.Trim();
            command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = employee.PasswordHash;
            command.Parameters.Add("@RoleID", SqlDbType.Int).Value = employee.RoleID;
            command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = employee.IsActive;
        }

        /// <summary>Throws if no rows were affected by an update/delete operation.</summary>
        private void EnsureUpdated(int rowsAffected, int employeeId)
        {
            if (rowsAffected > 0) return;
            _logger.LogWarning("Medewerker niet gevonden: {EmployeeId}.", employeeId);
            throw new InvalidOperationException(ErrorMessages.EmployeeNotFound);
        }

        /// <summary>Maps a SqlDataReader row to an Employee object.</summary>
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
