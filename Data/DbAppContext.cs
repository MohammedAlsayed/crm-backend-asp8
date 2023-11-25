using Microsoft.EntityFrameworkCore;
using CRM.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace CRM.Data;
public class DbAppContext : IdentityDbContext<User>
{
    public DbAppContext (DbContextOptions<DbAppContext> options): base(options)
    {
    }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Contact> Contacts => Set<Contact>();

}