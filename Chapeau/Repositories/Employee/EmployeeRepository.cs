using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using EmployeeModel = Chapeau.Models.Employee;

namespace Chapeau.Repositories
{
    public class EmployeeRepository
    {
        private readonly string _connectionString;

        public EmployeeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
                ?? throw new Exception("Database connection string is missing.");
        }

        public List<EmployeeModel> GetEmployees()
        {
            var employees = new List<EmployeeModel>();

            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = @"
                    SELECT 
                        e.EmployeeID,
                        e.Name,
                        e.PasswordHash,
                        e.RoleID,
                        e.IsActive,
                        r.RoleName
                    FROM Employee e
                    INNER JOIN Roles r ON e.RoleID = r.RoleID
                    ORDER BY e.EmployeeID";

                using SqlCommand command = new(query, connection);
                using SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    employees.Add(MapEmployee(reader));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving employees: {ex.Message}", ex);
            }

            return employees;
        }

        public EmployeeModel? GetEmployeeById(int employeeId)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = @"
                    SELECT 
                        e.EmployeeID,
                        e.Name,
                        e.PasswordHash,
                        e.RoleID,
                        e.IsActive,
                        r.RoleName
                    FROM Employee e
                    INNER JOIN Roles r ON e.RoleID = r.RoleID
                    WHERE e.EmployeeID = @EmployeeID";

                using SqlCommand command = new(query, connection);
                command.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = employeeId;

                using SqlDataReader reader = command.ExecuteReader();

                if (!reader.Read())
                {
                    return null;
                }

                return MapEmployee(reader);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving employee by id: {ex.Message}", ex);
            }
        }

        public EmployeeModel? GetEmployeeByName(string name)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = @"
                    SELECT 
                        e.EmployeeID,
                        e.Name,
                        e.PasswordHash,
                        e.RoleID,
                        e.IsActive,
                        r.RoleName
                    FROM Employee e
                    INNER JOIN Roles r ON e.RoleID = r.RoleID
                    WHERE e.Name = @Name";

                using SqlCommand command = new(query, connection);
                command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name;

                using SqlDataReader reader = command.ExecuteReader();

                if (!reader.Read())
                {
                    return null;
                }

                return MapEmployee(reader);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving employee by name: {ex.Message}", ex);
            }
        }

        public async Task<EmployeeModel?> GetEmployeeByNameAsync(string name)
        {
            try
            {
                await using SqlConnection connection = new(_connectionString);
                await connection.OpenAsync();

                string query = @"
                    SELECT 
                        e.EmployeeID,
                        e.Name,
                        e.PasswordHash,
                        e.RoleID,
                        e.IsActive,
                        r.RoleName
                    FROM Employee e
                    INNER JOIN Roles r ON e.RoleID = r.RoleID
                    WHERE e.Name = @Name";

                await using SqlCommand command = new(query, connection);
                command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name;

                await using SqlDataReader reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return null;
                }

                return MapEmployee(reader);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving employee by name async: {ex.Message}", ex);
            }
        }

        public void AddEmployee(EmployeeModel employee)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = @"
                    INSERT INTO Employee 
                        (Name, PasswordHash, RoleID, IsActive)
                    VALUES 
                        (@Name, @PasswordHash, @RoleID, @IsActive)";

                using SqlCommand command = new(query, connection);

                command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = employee.Name;
                command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = employee.PasswordHash;
                command.Parameters.Add("@RoleID", SqlDbType.Int).Value = employee.RoleID;
                command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = employee.IsActive;

                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while adding the employee: {ex.Message}", ex);
            }
        }

        public void UpdateEmployee(EmployeeModel employee)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = @"
                    UPDATE Employee
                    SET 
                        Name = @Name,
                        PasswordHash = @PasswordHash,
                        RoleID = @RoleID,
                        IsActive = @IsActive
                    WHERE EmployeeID = @EmployeeID";

                using SqlCommand command = new(query, connection);

                command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = employee.Name;
                command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = employee.PasswordHash;
                command.Parameters.Add("@RoleID", SqlDbType.Int).Value = employee.RoleID;
                command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = employee.IsActive;
                command.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = employee.EmployeeID;

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new Exception("Update failed: Employee not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the employee: {ex.Message}", ex);
            }
        }

        public void SetEmployeeActive(int id, bool active)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = @"
                    UPDATE Employee
                    SET IsActive = @IsActive
                    WHERE EmployeeID = @EmployeeID";

                using SqlCommand command = new(query, connection);

                command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = active;
                command.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = id;

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new Exception("Update failed: Employee not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the employee active status: {ex.Message}", ex);
            }
        }

        private static EmployeeModel MapEmployee(SqlDataReader reader)
        {
            return new EmployeeModel
            {
                EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                Name = reader["Name"].ToString() ?? string.Empty,
                PasswordHash = reader["PasswordHash"].ToString() ?? string.Empty,
                RoleID = Convert.ToInt32(reader["RoleID"]),
                RoleName = reader["RoleName"].ToString() ?? string.Empty,
                IsActive = Convert.ToBoolean(reader["IsActive"])
            };
        }
    }
}