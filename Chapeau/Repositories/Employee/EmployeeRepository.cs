using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Chapeau.Models;
using System.Data;

namespace Chapeau.Repositories
{
    public class EmployeeRepository(IConfiguration configuration)
    {
        private readonly string _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
                                ?? throw new Exception("Database connection string is missing.");

        public List<Employee> GetEmployees()
        {
            var employees = new List<Employee>();

            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = "SELECT EmployeeID, Name, PasswordHash, RoleID, IsActive FROM Employee";
                using SqlCommand command = new(query, connection);
                using SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    int employeeIdOrdinal = reader.GetOrdinal("EmployeeID");
                    int nameOrdinal = reader.GetOrdinal("Name");
                    int passwordHashOrdinal = reader.GetOrdinal("PasswordHash");
                    int roleIdOrdinal = reader.GetOrdinal("RoleID");
                    int isActiveOrdinal = reader.GetOrdinal("IsActive");

                    while (reader.Read())
                    {
                        var employee = new Employee
                        {
                            EmployeeID = reader.GetInt32(employeeIdOrdinal),
                            Name = reader.GetString(nameOrdinal),
                            PasswordHash = reader.GetString(passwordHashOrdinal),
                            RoleID = reader.GetInt32(roleIdOrdinal),
                            IsActive = reader.GetBoolean(isActiveOrdinal)
                        };

                        employees.Add(employee);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving employees: {ex.Message}", ex);
            }

            return employees;
        }

        public async Task<Employee?> GetEmployeeByNameAsync(string name)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                await connection.OpenAsync();

                string query = "SELECT EmployeeID, Name, PasswordHash, RoleID, IsActive FROM Employee WHERE Name = @Name AND IsActive = 1";
                using SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@Name", name);

                using SqlDataReader reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new Employee
                    {
                        EmployeeID = reader.GetInt32(reader.GetOrdinal("EmployeeID")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                        RoleID = reader.GetInt32(reader.GetOrdinal("RoleID")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving employee: {ex.Message}", ex);
            }

            return null;
        }

        public void AddEmployee(Employee employee)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = "INSERT INTO Employee (Name, PasswordHash, RoleID, IsActive) VALUES (@Name, @PasswordHash, @RoleID, @IsActive)";
                using SqlCommand command = new(query, connection);

                command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = (object)employee.Name ?? DBNull.Value;
                command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = (object)employee.PasswordHash ?? DBNull.Value;
                command.Parameters.Add("@RoleID", SqlDbType.Int).Value = employee.RoleID;
                command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = employee.IsActive;

                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while adding the employee: {ex.Message}", ex);
            }
        }

        public void UpdateEmployee(Employee employee)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = "UPDATE Employee SET Name = @Name, PasswordHash = @PasswordHash, RoleID = @RoleID, IsActive = @IsActive WHERE EmployeeID = @EmployeeID";
                using SqlCommand command = new(query, connection);

                command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = (object)employee.Name ?? DBNull.Value;
                command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = (object)employee.PasswordHash ?? DBNull.Value;
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

                string query = "UPDATE Employee SET IsActive = @IsActive WHERE EmployeeID = @EmployeeID";
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
                throw new Exception($"An error occurred while updating the employee's active status: {ex.Message}", ex);
            }
        }
    }
}