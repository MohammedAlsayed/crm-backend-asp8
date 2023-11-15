using CRM.Models;
using CRM.Services;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientController : ControllerBase
{
    ClientService _service;
    public ClientController(ClientService service)
    {
        _service = service;
    }

    // GET all action
    [HttpGet]
    public ActionResult<List<Client>> GetAll() =>
        _service.GetAll();
    
    // GET by Id action
    [HttpGet("{id}")]
    public ActionResult<Client> Get(int id)
    {
        var client = _service.GetById(id);

        if(client == null)
            return NotFound();

        return client;
    }
    
    [HttpGet("nextId/")]
    public ActionResult<int> NextId() =>
        _service.GetNextId();

    [HttpGet("searchNames/{name}")]
    public ActionResult<Object> SearchNames(string name)
    {
        var names = _service.SearchEnNames(name);

        if(names == null)
            return NotFound();

        return Ok(names);
    }

    // POST action
    [HttpPost]
    public IActionResult Create(Client client)
    {   
        _service.Create(client);
        return CreatedAtAction(nameof(Create), new { id = client.Id }, client);
    }

    // PUT action
    [HttpPut("{id}")]
    public IActionResult Update(int id, Client client)
    {
        if (id != client.Id)
            return BadRequest();

        var existingClient = _service.Update(client);
        if (existingClient is null)
            return NotFound();

        return NoContent();
    }

    // DELETE action
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var existingClient = _service.GetById(id);

        if (existingClient is null)
            return NotFound();

        _service.DeleteById(id);

        return NoContent();
    }
}
