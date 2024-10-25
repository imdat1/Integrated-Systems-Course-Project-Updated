using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Service.Interface;

namespace Service.Implementation
{
    public class CheckDBConnection : ICheckDBConnection
    {
        public async Task<bool> CheckDatabaseCredentialsAsync(string host, string database, string username, string password)
        {
            try
            {
                using (var connection = new NpgsqlConnection($"Host={host};Database={database};Username={username};Password={password}"))
                {
                    await connection.OpenAsync();
                    // If we reach here, the credentials are valid
                    return true;
                }
            }
            catch (NpgsqlException)
            {
                // Catch PostgreSQL specific exceptions (e.g. authentication failure)
                return false;
            }
            catch (Exception)
            {
                // Catch all other exceptions (e.g. network issues)
                return false;
            }
        }
    }
}
