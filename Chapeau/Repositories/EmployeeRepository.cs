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
    public class EmployeeRepository(IConfiguration configuration, ILogger<EmployeeRepository> logger)
    {
        private readonly string _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
            ?? throw new InvalidOperationException(ErrorMessages.ConnectionStringMissing);
        
        private readonly ILogger<EmployeeRepository> _logger = logger;

        public List<Employee> GetEmployees()
        {
            var employees = new List<Employee>();

            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = "SELECT EmployeeID, Name, Username, PasswordHash, Role, IsActive FROM Employees";
                using SqlCommand command = new(query, connection);
                using SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                    employees = MapEmployeesFromReader(reader);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error retrieving employees");
                throw new InvalidOperationException(ErrorMessages.RetrieveEmployeesError, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving employees");
                throw new InvalidOperationException(ErrorMessages.RetrieveEmployeesError, ex);
            }

            return employees;
        }

        public void AddEmployee(Employee employee)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = @"INSERT INTO Employees (Name, Username, PasswordHash, Role, IsActive) 
                               VALUES (@Name, @Username, @PasswordHash, @Role, @IsActive)";
                
                using SqlCommand command = new(query, connection);
                AddEmployeeParameters(command, employee);
                command.ExecuteNonQuery();

                _logger.LogInformation("Employee added: {EmployeeName}", employee.Name);
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                _logger.LogWarning("Duplicate username: {Username}", employee.Username);
                throw new InvalidOperationException(ErrorMessages.UsernameTaken, ex);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error adding employee");
                throw new InvalidOperationException(ErrorMessages.AddEmployeeError, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error adding employee");
                throw new InvalidOperationException(ErrorMessages.AddEmployeeError, ex);
            }
        }

        public void UpdateEmployee(Employee employee)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = @"UPDATE Employees SET Name = @Name, Username = @Username, 
                               PasswordHash = @PasswordHash, Role = @Role, IsActive = @IsActive 
                               WHERE EmployeeID = @EmployeeID";
                
                using SqlCommand command = new(query, connection);
                AddEmployeeParameters(command, employee);
                command.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = employee.EmployeeID;

                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 0)
                    throw new KeyNotFoundException(ErrorMessages.EmployeeNotFound);

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
                _logger.LogError(ex, "SQL error updating employee");
                throw new InvalidOperationException(ErrorMessages.UpdateEmployeeError, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating employee");
                throw new InvalidOperationException(ErrorMessages.UpdateEmployeeError, ex);
            }
        }

        public void SetEmployeeActive(int id, bool active)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = "UPDATE Employees SET IsActive = @IsActive WHERE EmployeeID = @EmployeeID";
                using SqlCommand command = new(query, connection);
                command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = active;
                command.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = id;

                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 0)
                    throw new KeyNotFoundException(ErrorMessages.EmployeeNotFound);

                _logger.LogInformation("Employee {EmployeeId} active status changed to {Active}", id, active);
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating employee status");
                throw new InvalidOperationException(ErrorMessages.UpdateEmployeeActiveError, ex);
            }
        }

        // ===== HELPER METHODS =====

        private List<Employee> MapEmployeesFromReader(SqlDataReader reader)
        {
            var employees = new List<Employee>();
            int employeeIdOrdinal = reader.GetOrdinal("EmployeeID");
            int nameOrdinal = reader.GetOrdinal("Name");
            int usernameOrdinal = reader.GetOrdinal("Username");
            int passwordHashOrdinal = reader.GetOrdinal("PasswordHash");
            int roleOrdinal = reader.GetOrdinal("Role");
            int isActiveOrdinal = reader.GetOrdinal("IsActive");

            while (reader.Read())
                employees.Add(MapEmployeeRow(reader, employeeIdOrdinal, nameOrdinal, usernameOrdinal, 
                    passwordHashOrdinal, roleOrdinal, isActiveOrdinal));

            return employees;
        }

        private Employee MapEmployeeRow(SqlDataReader reader, int employeeIdOrdinal, int nameOrdinal,
            int usernameOrdinal, int passwordHashOrdinal, int roleOrdinal, int isActiveOrdinal)
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