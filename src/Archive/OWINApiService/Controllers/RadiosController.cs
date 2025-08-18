using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using owinapiservice.Authorization;
using owinapiservice.EventHandlers.Personnel;
using owinapiservice.EventHandlers.Radios;
using owinapiservice.Models.Radios;

namespace owinapiservice.Controllers;

[Authorize]
[RoutePrefix("api/v1/radios")]
public class RadiosController : ApiController
{
    private readonly GetPersonnel _getPersonnel;
    private readonly GetPerson _getPerson;
    private readonly GetPersonnelRadiosIncludingTemporary _getPersonnelRadiosIncludingTemporary;
    private readonly CreatePersonnelRadioFields _createPersonnelRadioFields;
    private readonly DeletePersonnelRadio _deletePersonnelRadio;
    private readonly GetPersonnelRadiosByRadioCode _getPersonnelRadiosByRadioCode;
    private readonly SavePersonnelRadio _savePersonnelRadio;
    
    public RadiosController(
        GetPersonnel getPersonnel,
        GetPerson getPerson,
        GetPersonnelRadiosIncludingTemporary getPersonnelRadiosIncludingTemporary,
        CreatePersonnelRadioFields createPersonnelRadioFields,
        DeletePersonnelRadio deletePersonnelRadio,
        GetPersonnelRadiosByRadioCode getPersonnelRadiosByRadioCode,
        SavePersonnelRadio savePersonnelRadio)
    {
        _getPersonnel = getPersonnel;
        _getPerson = getPerson;
        _getPersonnelRadiosIncludingTemporary = getPersonnelRadiosIncludingTemporary;
        _createPersonnelRadioFields = createPersonnelRadioFields;
        _deletePersonnelRadio = deletePersonnelRadio;
        _getPersonnelRadiosByRadioCode = getPersonnelRadiosByRadioCode;
        _savePersonnelRadio = savePersonnelRadio;
    }

    [HttpGet]
    [Route("")]
    [RequirePermission("RadioView")]
    public IHttpActionResult GetAll()
    {
        var personnel = _getPersonnel.Handle()
            .Result.Where(person =>
                person.AgencyID == 2 &&
                person.JurisdictionID == 2 &&
                person.GetPersonnelDivision().ToString() != "INACT")
            .ToList();
        
        var personnelList = personnel.Select(person => new
        {
            CHRISID = person.Code,
            person.Name,
            District = person.GetPersonnelDivision().GetSector().GetDisplayName(),
            RadioID = _getPersonnelRadiosIncludingTemporary.Handle(person.ID)
        }).ToList();

        return Ok(personnelList);
    }
    
    [HttpGet]
    [Route("{id}")]
    [RequirePermission("RadioView")]
    public IHttpActionResult Get(string id)
    {
        // TODO: Logic to retrieve a radio
        
        return Ok("Retrieved radio successfully.");
    }
    
    [HttpPost]
    [Route("create")]
    [RequirePermission("Administrator")]
    public async Task<IHttpActionResult> Create(CreateRadioRequest payload)
    {
        if (!int.TryParse(payload.ChrisId, out _) || !int.TryParse(payload.RadioId, out var rId))
            throw new Exception("Invalid Chris ID or Radio ID.");

        var resultPersonnelId = _getPerson.Handle(payload.ChrisId)
            .Result?.PersonnelID;
        
        if (resultPersonnelId == null) return Ok("Failed to create radio.");
        
        var personnelId = (int)resultPersonnelId;
        
        var personnelRadio = _getPersonnelRadiosByRadioCode.Handle(payload.RadioId)
            .Result;
        
        foreach (var radio in personnelRadio.Where(radio => radio.Code == rId.ToString()))
            _deletePersonnelRadio.Handle(radio.ID);
        
        var radioFields = await _createPersonnelRadioFields.Handle(
            null, personnelId, rId.ToString(), rId.ToString(), "Radio Name");
            
        _savePersonnelRadio.Handle(radioFields);


        return Ok("Created radio successfully.");
    }
    
    [HttpPost]
    [Route("{id}/update")]
    [RequirePermission("Administrator")]
    public async Task<IHttpActionResult> Update(UpdateRadioRequest payload)
    {
        if (!int.TryParse(payload.ChrisId, out _) || !int.TryParse(payload.OldRadioId, out var oldRadioId) || !int.TryParse(payload.NewRadioId, out var newRadioId))
            throw new Exception("Invalid Chris ID or Radio ID.");
        
        var personnelRadio = _getPersonnelRadiosByRadioCode.Handle(payload.OldRadioId)
            .Result;        
        
        foreach (var radio in personnelRadio.Where(radio => radio.Code == oldRadioId.ToString()))
            _deletePersonnelRadio.Handle(radio.ID);
        
        var personnelId = _getPerson.Handle(payload.ChrisId)
            .Result?.PersonnelID;
        
        if (personnelId == null) return Ok("Failed to update radio.");
        
        var pId = (int)personnelId;
        
        personnelRadio = _getPersonnelRadiosByRadioCode.Handle(payload.NewRadioId)
            .Result;
        
        foreach (var radio in personnelRadio.Where(radio => radio.Code == newRadioId.ToString()))
            _deletePersonnelRadio.Handle(radio.ID);
        
        var radioFields = await _createPersonnelRadioFields.Handle(
            null, pId, newRadioId.ToString(), newRadioId.ToString(), "Radio Name");
        
        _savePersonnelRadio.Handle(radioFields);
        
        return Ok("Updated radio successfully.");
    }
    
    [HttpDelete]
    [Route("{id}/delete")]
    [RequirePermission("Administrator")]
    public async Task<IHttpActionResult> Delete(DeleteRadioRequest payload)
    {
        if (!int.TryParse(payload.ChrisId, out var cId) || !int.TryParse(payload.RadioId, out var rId))
            throw new Exception("Invalid Chris ID or Radio ID.");
        
        var personnelRadio = _getPersonnelRadiosByRadioCode.Handle(payload.RadioId)
            .Result;

        var personnelRadios = personnelRadio.ToList();
        
        if (personnelRadios.All(radio => radio.Code != rId.ToString()))
            throw new Exception($"Unable to delete radio ID {rId}, it is not associated with personnel {cId}...");
        
        foreach (var radio in personnelRadios.Where(radio => radio.Code == rId.ToString()))
            _deletePersonnelRadio.Handle(radio.ID);
        
        return Ok("Deleted radio successfully.");
    }
}