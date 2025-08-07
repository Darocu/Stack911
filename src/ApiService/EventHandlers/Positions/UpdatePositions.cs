using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiService.Models.Positions;
using TriTech.VisiCAD;

namespace ApiService.EventHandlers.Positions;

public class UpdatePositions
{
    private readonly CADManager _apiCadManager;

    public UpdatePositions(CADManager apiCadManager)
    {
        _apiCadManager = apiCadManager;
    }

    public Task<List<PositionResponse>> Handle(List<string> positions, CancellationToken cancellationToken)
    {
        var positionList = new List<PositionResponse>();
        var sectors = new Dictionary<int, bool>
        {
            { 2, false }, // sectorOne
            { 3, false }, // sectorTwo
            { 4, false }, // sectorThree
            { 5, false }, // sectorFour
            { 21, false }, // sectorCbs
            { 23, false }  // sectorCpw
        };

        foreach (var position in positions)
        {
            var machineId = _apiCadManager.GeneralQueryEngine.GetMachineInfoIDByName(position);
            var machineInfo = _apiCadManager.GeneralQueryEngine.GetMachineInfo(machineId);
            var machineName = machineInfo.Name;
            var personnel = machineInfo.Personnel;

            var cadUserId = 0;
            var employeeId = "";
            var lastName = "";
            var firstName = "";
            var controlledSectorName = "";

            if (personnel != null)
            {
                cadUserId = personnel.PersonnelID;
                employeeId = personnel.Code;
                var nameParts = personnel.Name.Split(',');
                lastName = nameParts[0].Trim();
                var firstNameAndTitle = nameParts[1].Trim().Split(' ');
                firstName = firstNameAndTitle[0];

                // TODO: Test if this works with the new API
                var controlledSectorNames = sectors.Keys
                    .Where(sector => _apiCadManager.PersonQueryEngine.IsSectorControlledByMachine(sector, machineId))
                    .Select(sector => _apiCadManager.HierarchyQueryEngine.GetSector(sector).Name)
                    .ToList();

                controlledSectorName = string.Join("/", controlledSectorNames);
            }

            positionList.Add(new PositionResponse
            {
                MachineId = machineId,
                MachineName = machineName,
                CadUserId = cadUserId,
                EmployeeId = employeeId,
                LastName = lastName,
                FirstName = firstName,
                ControlledSectorName = controlledSectorName,
                ReportedTime = DateTime.UtcNow
            });
        }

        return Task.FromResult(positionList);
    }
}