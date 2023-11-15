using System.Text;
using CRM.Data;
using CRM.Models;
using Microsoft.EntityFrameworkCore;
using System.Web;

namespace CRM.Services;

public class ClientService
{
    private readonly CrmContext _context;
    public ClientService(CrmContext context)
    {
        _context = context;
    }

    public List<Client> GetAll()
    {
        return _context.Clients.AsNoTracking().ToList();
    }
    
    public Client? GetById(int id)
    {
        return _context.Clients
            .Include(p => p.Contacts)
            .AsNoTracking()
            .SingleOrDefault(p => p.Id == id);
    }

    public int GetNextId()
    {
        return _context.Clients.Max(c => c.Id) + 1;
    }  

    // search by name and return name and id only for now 
    public IEnumerable<Object> SearchEnNames(string name) 
    {   
        var decodedName = HttpUtility.UrlDecode(name, Encoding.UTF8);
        return _context.Clients
            .Where(c => c.EnName.ToLower().Contains(decodedName.ToLower()))
            .Select(c => new { Id = c.Id, EnName = c.EnName }).ToList();
    } 

    public Client Create(Client newClient)
    {
        _context.Clients.Add(newClient);
        _context.SaveChanges();

        return newClient;
    }

    public Client Update(Client updateClient)
    {
        _context.Clients.Update(updateClient);
        _context.SaveChanges();
        return updateClient;
    }

    public bool DeleteById(int id)
    {
        var client = _context.Clients.Find(id);
        if (client is not null)
        {
            _context.Clients.Remove(client);
            _context.SaveChanges();
            return true;
        }

        return false;
    }


    // public List<string?> SearchEnNames(string name) => _context.Clients.Where(c => c.EnName.ToLower().Contains(name.ToLower())).Select(c => new {c.Id, c.EnName}).ToList();
}