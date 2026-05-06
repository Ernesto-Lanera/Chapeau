using System;
using System.Collections.Generic;
using System.Data;
using Chapeau.Constants;
using Chapeau.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Chapeau.Repositories
{
    using System.Data;

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
                using SqlCommand command = new (query, connection);
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
                            EmployeeID = reader.GetInt32(employeeIdOrdinal),
                            Name = reader.GetString(nameOrdinal),
                            Username = reader.GetString(usernameOrdinal),
                            PasswordHash = reader.GetString(passwordHashOrdinal),
                string query = "INSERT INTO Employee (Name, PasswordHash, RoleID, IsActive) VALUES (@Name, @PasswordHash, @RoleID, @IsActive)";
                using SqlCommand command = new(query, connection);

                command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = (object)employee.Name ?? DBNull.Value;
                command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = (object)employee.PasswordHash ?? DBNull.Value;
                command.Parameters.Add("@RoleID", SqlDbType.Int).Value = employee.RoleID;
                command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = employee.IsActive;

            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving employees");
                throw new InvalidOperationException(ErrorMessages.RetrieveEmployeesError, ex);
                string query = "INSERT INTO Employees (Name, Username, PasswordHash, Role, IsActive) VALUES (@Name, @Username, @PasswordHash, @Role, @IsActive)";
                using SqlCommand command = new(query, connection);
                throw new InvalidOperationException("This employee name already exists.");
                command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = (object)employee.Name ?? DBNull.Value;
                command.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = (object)employee.Username ?? DBNull.Value;
                command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = (object)employee.PasswordHash ?? DBNull.Value;
                command.Parameters.Add("@Role", SqlDbType.NVarChar, 50).Value = employee.Role.ToString();
                command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = employee.IsActive;

            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = "INSERT INTO Employees (Name, Username, PasswordHash, Role, IsActive) VALUES (@Name, @Username, @PasswordHash, @Role, @IsActive)";
                using SqlCommand command = new(query, connection);
                throw new InvalidOperationException("This username is already taken. Please try another one.");
                command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = (object)employee.Name ?? DBNull.Value;
                command.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = (object)employee.Username ?? DBNull.Value;
                command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = (object)employee.PasswordHash ?? DBNull.Value;
                command.Parameters.Add("@Role", SqlDbType.NVarChar, 50).Value = employee.Role.ToString();
                command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = employee.IsActive;

                command.ExecuteNonQuery();
                string query = "UPDATE Employee SET Name = @Name, PasswordHash = @PasswordHash, RoleID = @RoleID, IsActive = @IsActive WHERE EmployeeID = @EmployeeID";
                using SqlCommand command = new(query, connection);

                command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = (object)employee.Name ?? DBNull.Value;
                command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = (object)employee.PasswordHash ?? DBNull.Value;
                command.Parameters.Add("@RoleID", SqlDbType.Int).Value = employee.RoleID;
                command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = employee.IsActive;
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error adding employee");
                throw new InvalidOperationException(ErrorMessages.AddEmployeeError, ex);
            }
            catch (Exception ex)
                string query = "UPDATE Employees SET Name = @Name, Username = @Username, PasswordHash = @PasswordHash, Role = @Role, IsActive = @IsActive WHERE EmployeeID = @EmployeeID";
                using SqlCommand command = new(query, connection);

                command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = (object)employee.Name ?? DBNull.Value;
                command.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = (object)employee.Username ?? DBNull.Value;
                command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = (object)employee.PasswordHash ?? DBNull.Value;
                command.Parameters.Add("@Role", SqlDbType.NVarChar, 50).Value = employee.Role.ToString();
                command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = employee.IsActive;
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();
                throw new InvalidOperationException("This employee name is already taken by another employee.");
                string query = "UPDATE Employees SET Name = @Name, Username = @Username, PasswordHash = @PasswordHash, Role = @Role, IsActive = @IsActive WHERE EmployeeID = @EmployeeID";
                using SqlCommand command = new(query, connection);

                command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = (object)employee.Name ?? DBNull.Value;
                command.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = (object)employee.Username ?? DBNull.Value;
                command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = (object)employee.PasswordHash ?? DBNull.Value;
                command.Parameters.Add("@Role", SqlDbType.NVarChar, 50).Value = employee.Role.ToString();
                command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = employee.IsActive;
                command.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = employee.EmployeeID;

                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 0)
                    throw new KeyNotFoundException(ErrorMessages.EmployeeNotFound);
                throw new InvalidOperationException("This username is already taken by another employee.");
                _logger.LogInformation("Employee updated: {EmployeeId}", employee.EmployeeID);
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                _logger.LogWarning("Duplicate username: {Username}", employee.Username);
                throw new InvalidOperationException(ErrorMessages.UsernameAlreadyTaken, ex);
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("This username is already taken by another employee.");
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the employee's active status: {ex.Message}", ex);
            }
        {
            string roleText = reader.IsDBNull(roleOrdinal) ? "" : reader.GetString(roleOrdinal).Trim();
            EmployeeRole role = Enum.TryParse<EmployeeRole>(roleText, true, out var parsedRole) 
                ? parsedRole 
                : EmployeeRole.Waiter;

            return new Employee
            {
                EmployeeID = reader.GetInt32(employeeIdOrdinal),
                Name = reader.GetString(nameOrdinal),
                Username = reader.GetString(usernameOrdinal),
                PasswordHash = reader.GetString(passwordHashOrdinal),
                Role = role,
                IsActive = reader.GetBoolean(isActiveOrdinal)
            };
        }

        private void AddEmployeeParameters(SqlCommand command, Employee employee)
        {
            command.Parameters.Add("@Name", SqlDbType.NVarChar, DatabaseConstraints.EmployeeNameMaxLength)
                .Value = (object?)employee.Name ?? DBNull.Value;
            command.Parameters.Add("@Username", SqlDbType.NVarChar, DatabaseConstraints.EmployeeUsernameMaxLength)
                .Value = (object?)employee.Username ?? DBNull.Value;
            command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, DatabaseConstraints.EmployeePasswordHashMaxLength)
                .Value = (object?)employee.PasswordHash ?? DBNull.Value;
            command.Parameters.Add("@Role", SqlDbType.NVarChar, DatabaseConstraints.EmployeeRoleMaxLength)
                .Value = employee.Role.ToString();
            command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = employee.IsActive;
        }
    }
}