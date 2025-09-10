using System;
using System.Threading.Tasks;
using TriTech.Common.Interface;
using TriTech.VisiCAD;

namespace owinapiservice.EventHandlers.Incidents;

public class CreateComment
{
    private readonly CADManager _cadManager;

    public CreateComment(CADManager cadManager)
    {
        _cadManager = cadManager;
    }

    public async Task<int> Handle(string comment, int incidentId, string employeeId)
    {
        if (string.IsNullOrEmpty(comment))
            throw new ArgumentNullException(nameof(comment), "Comment data not found, comment not created");

        return await AddIncidentCommentAsync(comment, incidentId, employeeId);
    }

    public async Task<int> Handle(string comment, string incidentNumber, string employeeId)
    {
        if (string.IsNullOrEmpty(comment))
            throw new ArgumentNullException(nameof(comment), "Comment data not found, comment not created");

        var incidentId = _cadManager.IncidentQueryEngine.GetIncidentIDByIncidentNumber(incidentNumber);
        if (!incidentId.HasValue)
            throw new ArgumentException($"Incident not found for number {incidentNumber}", nameof(incidentNumber));

        return await AddIncidentCommentAsync(comment, incidentId.Value, employeeId);
    }

    private async Task<int> AddIncidentCommentAsync(string comment, int incidentId, string employeeId)
    {
        var incident = _cadManager.IncidentQueryEngine.GetIncident(incidentId);
        if (incident == null)
            throw new ArgumentNullException(nameof(employeeId), "Incident not found");
        
        if (!_cadManager.IncidentQueryEngine.IsIncidentActive(incidentId))
            throw new ArgumentException($"Incident with ID {incidentId} is not active", nameof(incidentId));
        
        var incidentCommentId = 0;

        var formattedComment = comment.Replace(@"\r\n", Environment.NewLine)
                                     .Replace(@"\n", Environment.NewLine)
                                     .Replace(@"\r", Environment.NewLine);

        const VisiCADDefinition.EnumCommentType commentType = VisiCADDefinition.EnumCommentType.ResponseComments;
        const VisiCADDefinition.CommentCategory commentCategory = VisiCADDefinition.CommentCategory.BrowserUserEntered;

        await Task.Run(() =>
            _cadManager.IncidentActionEngine.AddIncidentComment(
                incidentId,
                commentType,
                employeeId,
                $"[ECCAPI] {formattedComment}",
                DateTime.Now,
                false,
                0,
                commentCategory,
                out incidentCommentId,
                out _));

        if (incidentCommentId <= 0)
            throw new InvalidOperationException(
                $"Failed to create Incident Comment ID for incident {incidentId}");

        return incidentCommentId;
    }

}