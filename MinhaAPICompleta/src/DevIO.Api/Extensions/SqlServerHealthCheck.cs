using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DevIO.Api.Extensions
{
    public class SqlServerHealthCheck : IHealthCheck
    {
        private readonly string _connection;

        public SqlServerHealthCheck(string connection)
        {
            _connection = connection;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var con = new SqlConnection(_connection))
                {
                    await con.OpenAsync(cancellationToken);

                    var cmd = con.CreateCommand();
                    cmd.CommandText = "SELECT COUNT(id) FROM produtos";

                    return Convert.ToInt32( await cmd.ExecuteScalarAsync(cancellationToken)) > 0 ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
                }
            }
            catch (Exception)
            {
                return HealthCheckResult.Unhealthy();
            }
        }
    }
}
