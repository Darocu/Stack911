using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using owinapiservice.Authorization;
using owinapiservice.EventHandlers.Positions;

namespace owinapiservice.Controllers;

[Authorize]
[RoutePrefix("api/v1/positions")]
public class PositionsController : ApiController
{
    private readonly GetPositionsFromDatabase _getPositionsFromDatabase;
    private readonly UpdatePositions _updatePositions;
    private readonly UpdatePositionsToDatabase _updatePositionsToDatabase;
    
    public PositionsController(
        GetPositionsFromDatabase getPositionsFromDatabase,
        UpdatePositions updatePositions,
        UpdatePositionsToDatabase updatePositionsToDatabase)
    {
        _getPositionsFromDatabase = getPositionsFromDatabase;
        _updatePositions = updatePositions;
        _updatePositionsToDatabase = updatePositionsToDatabase;
    }

    [HttpGet]
    [Route("")]
    [RequirePermission("PositionView")]
    public async Task<IHttpActionResult> GetAll()
    {
        try
        {
            var trackedPositions = await _getPositionsFromDatabase.HandleAsync(CancellationToken.None);
            
            if (trackedPositions == null || trackedPositions.Count == 0)
                return NotFound();
            
            var positions = await _updatePositions.Handle(trackedPositions, CancellationToken.None);

            await _updatePositionsToDatabase.HandleAsync(positions, CancellationToken.None);

            var result = positions;
            
            return Ok(result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}