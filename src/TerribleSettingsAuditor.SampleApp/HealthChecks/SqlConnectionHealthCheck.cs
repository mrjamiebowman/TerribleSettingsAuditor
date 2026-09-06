using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TerribleSettingsAuditor.SampleApp.HealthChecks;

public sealed class SqlConnectionHealthCheck : IHealthCheck
{
    private readonly string _connectionString;
    private readonly string _testQuery;
    private readonly TimeSpan _timeout;

    public SqlConnectionHealthCheck(string connectionString, string testQuery = "SELECT 1;", TimeSpan? timeout = null)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string must not be empty.", nameof(connectionString));
        }

        _testQuery = testQuery;
        _timeout = timeout ?? TimeSpan.FromSeconds(3);
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_timeout);

        var start = TimeSpan.FromMilliseconds(Environment.TickCount64);

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cts.Token).ConfigureAwait(false);

            await using var command = connection.CreateCommand();
            command.CommandText = _testQuery;
            command.CommandType = CommandType.Text;
            command.CommandTimeout = (int)_timeout.TotalSeconds;

            await command.ExecuteScalarAsync(cts.Token).ConfigureAwait(false);

            var elapsed = TimeSpan.FromMilliseconds(Environment.TickCount64) - start;

            return HealthCheckResult.Healthy("SQL connection succeeded.", new Dictionary<string, object> { ["elapsed_ms"] = elapsed.TotalMilliseconds });
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy($"SQL connection timed out after {_timeout.TotalSeconds:0.#}s.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("SQL connection failed.", ex);
        }
    }
}