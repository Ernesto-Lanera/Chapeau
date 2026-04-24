using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Chapeau.Models;

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

        public List<Employee> GetEmployees()
        {
            var employees = new List<Employee>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT EmployeeID, Name, Username, PasswordHash, Role, IsActive FROM Employees";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var employee = new Employee
                            {
                                EmployeeID = (int)reader["EmployeeID"],
                                Name = (string)reader["Name"],
                                Username = (string)reader["Username"],
                                PasswordHash = (string)reader["PasswordHash"],
                                Role = (string)reader["Role"],
                                IsActive = (bool)reader["IsActive"]
                            };
                            employees.Add(employee);
                        }
                    }
                }
            }

            return employees;
        }

        public void AddEmployee(Employee employee)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "INSERT INTO Employees (Name, Username, PasswordHash, Role, IsActive) VALUES (@Name, @Username, @PasswordHash, @Role, @IsActive)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", employee.Name);
                    command.Parameters.AddWithValue("@Username", employee.Username);
                    command.Parameters.AddWithValue("@PasswordHash", employee.PasswordHash);
                    command.Parameters.AddWithValue("@Role", employee.Role);
                    command.Parameters.AddWithValue("@IsActive", employee.IsActive);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateEmployee(Employee employee)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "UPDATE Employees SET Name = @Name, Username = @Username, PasswordHash = @PasswordHash, Role = @Role, IsActive = @IsActive WHERE EmployeeID = @EmployeeID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", employee.Name);
                    command.Parameters.AddWithValue("@Username", employee.Username);
                    command.Parameters.AddWithValue("@PasswordHash", employee.PasswordHash);
                    command.Parameters.AddWithValue("@Role", employee.Role);
                    command.Parameters.AddWithValue("@IsActive", employee.IsActive);
                    command.Parameters.AddWithValue("@EmployeeID", employee.EmployeeID);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void SetEmployeeActive(int id, bool active)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "UPDATE Employees SET IsActive = @IsActive WHERE EmployeeID = @EmployeeID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IsActive", active);
                    command.Parameters.AddWithValue("@EmployeeID", id);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
