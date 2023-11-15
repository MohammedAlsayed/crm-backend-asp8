using CRM.Data;
using CRM.Models;
using Microsoft.EntityFrameworkCore;

namespace CRM.Services;

public class ContactService
{
    private readonly CrmContext _context;
    public ContactService(CrmContext context)
    {
        _context = context;
    }
    public Contact? GetById(int id)
    {
        return _context.Contacts
            .AsNoTracking()
            .SingleOrDefault(p => p.Id == id);
    }
    public Contact Create(Contact newContact)
    {
        _context.Contacts.Add(newContact);
        _context.SaveChanges();

        return newContact;
    }

    public Contact Update(Contact updateContact)
    {
        _context.Contacts.Update(updateContact);
        _context.SaveChanges();
        return updateContact;
    }

    public bool DeleteById(int id)
    {
        var contact = _context.Contacts.Find(id);
        if (contact is not null)
        {
            _context.Contacts.Remove(contact);
            _context.SaveChanges();
            return true;
        }

        return false;
    }

}