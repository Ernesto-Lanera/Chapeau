using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Chapeau.Models;

namespace Chapeau.Repositories
{
    using System.Data;

    public class EmployeeRepository
    {
        private readonly string _connectionString;

        public EmployeeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
                                ?? throw new Exception("Database connection string is missing.");
        }

        public List<Employee> GetEmployees()
        {
            var employees = new List<Employee>();

            try
            {
                using SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();

                string query = "SELECT EmployeeID, Name, Username, PasswordHash, Role, IsActive FROM Employees";
                using SqlCommand command = new SqlCommand(query, connection);
                using SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    int roleOrdinal = reader.GetOrdinal("Role");
                    int employeeIdOrdinal = reader.GetOrdinal("EmployeeID");
                    int nameOrdinal = reader.GetOrdinal("Name");
                    int usernameOrdinal = reader.GetOrdinal("Username");
                    int passwordHashOrdinal = reader.GetOrdinal("PasswordHash");
                    int isActiveOrdinal = reader.GetOrdinal("IsActive");

                    while (reader.Read())
                    {
                        string roleText = !reader.IsDBNull(roleOrdinal) 
                            ? reader.GetString(roleOrdinal).Trim() 
                            : "";

                        EmployeeRole role = Enum.TryParse<EmployeeRole>(
                            roleText,
                            true,
                            out EmployeeRole parsedRole
                        ) ? parsedRole : EmployeeRole.Waiter;

                        var employee = new Employee
                        {
                            EmployeeID = reader.GetInt32(employeeIdOrdinal),
                            Name = reader.GetString(nameOrdinal),
                            Username = reader.GetString(usernameOrdinal),
                            PasswordHash = reader.GetString(passwordHashOrdinal),
                            Role = role,
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

        public void AddEmployee(Employee employee)
        {
            try
            {
                using SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();

                string query = "INSERT INTO Employees (Name, Username, PasswordHash, Role, IsActive) VALUES (@Name, @Username, @PasswordHash, @Role, @IsActive)";
                using SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = (object)employee.Name ?? DBNull.Value;
                command.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = (object)employee.Username ?? DBNull.Value;
                command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = (object)employee.PasswordHash ?? DBNull.Value;
                command.Parameters.Add("@Role", SqlDbType.NVarChar, 50).Value = employee.Role.ToString();
                command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = employee.IsActive;

                command.ExecuteNonQuery();
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                throw new InvalidOperationException("This username is already taken. Please try another one.");
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
                using SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();

                string query = "UPDATE Employees SET Name = @Name, Username = @Username, PasswordHash = @PasswordHash, Role = @Role, IsActive = @IsActive WHERE EmployeeID = @EmployeeID";
                using SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = (object)employee.Name ?? DBNull.Value;
                command.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = (object)employee.Username ?? DBNull.Value;
                command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = (object)employee.PasswordHash ?? DBNull.Value;
                command.Parameters.Add("@Role", SqlDbType.NVarChar, 50).Value = employee.Role.ToString();
                command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = employee.IsActive;
                command.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = employee.EmployeeID;

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new Exception("Update failed: Employee not found.");
                }
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                throw new InvalidOperationException("This username is already taken by another employee.");
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
                using SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();

                string query = "UPDATE Employees SET IsActive = @IsActive WHERE EmployeeID = @EmployeeID";
                using SqlCommand command = new SqlCommand(query, connection);

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