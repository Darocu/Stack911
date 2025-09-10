using System.Threading.Tasks;
using System.Web.Http;
using owinapiservice.Authorization;
using owinapiservice.EventHandlers.Units;

namespace owinapiservice.Controllers;

[Authorize]
[RoutePrefix("api/v1/units")]
public class UnitsController : ApiController
{
    private readonly GetUnits _getUnits;
    private readonly GetUnit _getUnit;
    
    public UnitsController(
        GetUnits getUnits,
        GetUnit getUnit)
    {
        _getUnits = getUnits;
        _getUnit = getUnit;
    }
    
    [HttpGet]
    [Route("")]
    [RequirePermission("UnitView")]
    public async Task<IHttpActionResult> GetAllActive()
    {
        var units = await _getUnits.GetUnitsAsync();

        if (units == null || !units.Any()) return NotFound();
        
        return Ok(units);
    }
    
    [HttpGet]
    [Route("{unitName}")]
    [RequirePermission("UnitView")]
    public IHttpActionResult Get(string unitName)
    {
        if (string.IsNullOrWhiteSpace(unitName))
            return BadRequest("Unit name is required.");
        
        var unit = _getUnit.GetUnitAsync(unitName).Result;
        
        if (unit == null)
            return NotFound();
        
        return Ok(unit);
    }
}