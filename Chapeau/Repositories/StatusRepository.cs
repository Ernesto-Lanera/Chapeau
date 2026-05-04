using Chapeau.Constants;
using Microsoft.Data.SqlClient;

namespace Chapeau.Repositories
{
    public class StatusRepository(IConfiguration configuration)
    {
        private readonly string _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
            ?? throw new InvalidOperationException(ErrorMessages.ConnectionStringMissing);

        public bool IsApplicationHealthy()
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}