using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApiService.Models;
using Microsoft.Data.SqlClient;

namespace ApiService.EventHandlers.Positions;

public class GetPositionsFromDatabase
{
    private readonly string _connectionString;

    public GetPositionsFromDatabase(ServiceSettings serviceSettings)
    {
        _connectionString = serviceSettings.EccDataConnectionString;
    }
    public async Task<List<string>> HandleAsync(CancellationToken cancellationToken)
    {
        var positions = new List<string>();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        using var command = new SqlCommand("SELECT MachineName FROM PositionList", connection);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var machineName = reader.GetString(reader.GetOrdinal("MachineName"));
            positions.Add(machineName);
        }
        return positions;
    }
}