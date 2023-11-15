using CRM.Models;
using CRM.Services;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactController : ControllerBase
{
    ContactService _service;
    public ContactController(ContactService service)
    {
        _service = service;
    }

    // POST action
    [HttpPost]
    public IActionResult Create(Contact contact)
    {   
        _service.Create(contact);
        return CreatedAtAction(nameof(Create), new { id = contact.Id }, contact);
    }

    // PUT action
    [HttpPut("{id}")]
    public IActionResult Update(int id, Contact contact)
    {
        if (id != contact.Id)
            return BadRequest();

        var existingContact = _service.GetById(id);
        if (existingContact is null)
            return NotFound();

        _service.Update(contact);

        return NoContent();
    }

     // DELETE action
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var existingContact = _service.GetById(id);

        if (existingContact is null)
            return NotFound();

        _service.DeleteById(id);

        return NoContent();
    }
}