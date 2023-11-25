using CRM.Data;
using CRM.Models;
using Microsoft.EntityFrameworkCore;

namespace CRM.Services;

public class UserService
{
    private readonly DbAppContext _context;
    public UserService(DbAppContext context)
    {
        _context = context;
    }
    // public User? GetById(int id)
    // {
    //     return _context.Users.AsNoTracking().SingleOrDefault(p => p.Id == id);
    // }
    public User Create(User newUser)
    {
        _context.Users.Add(newUser);
        _context.SaveChanges();

        return newUser;
    }
    
}
