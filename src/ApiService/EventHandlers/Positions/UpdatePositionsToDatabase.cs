using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ApiService.Models;
using ApiService.Models.Positions;
using Microsoft.Data.SqlClient;

namespace ApiService.EventHandlers.Positions;

public class UpdatePositionsToDatabase
{
    private readonly string _hostName = Environment.MachineName;

    private readonly string _connectionString;

    public UpdatePositionsToDatabase(ServiceSettings serviceSettings)
    {
        _connectionString = serviceSettings.EccDataConnectionString;
    }
    
    public async Task HandleAsync(List<PositionResponse> positions, CancellationToken cancellationToken)
    {
        var tableName = _hostName == "ECCCADAPI" ? "dbo.Positions" : "dbo.Positions_Test";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var bulkCopy = new SqlBulkCopy(connection);
        bulkCopy.DestinationTableName = tableName;

        var table = new DataTable();
        table.Columns.Add("MachineId", typeof(int));
        table.Columns.Add("MachineName", typeof(string));
        table.Columns.Add("CadUserId", typeof(int));
        table.Columns.Add("EmployeeId", typeof(string));
        table.Columns.Add("LastName", typeof(string));
        table.Columns.Add("FirstName", typeof(string));
        table.Columns.Add("ControlledSectorName", typeof(string));
        table.Columns.Add("AdvisorRole", typeof(string));
        table.Columns.Add("ReportedTime", typeof(DateTime));

        foreach (var position in positions)
        {
            table.Rows.Add(position.MachineId, position.MachineName, position.CadUserId, position.EmployeeId,
                position.LastName, position.FirstName, position.ControlledSectorName, null, position.ReportedTime);
        }

        await bulkCopy.WriteToServerAsync(table, cancellationToken);
    }
}