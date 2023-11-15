using Microsoft.EntityFrameworkCore;
using CRM.Models;

namespace CRM.Data;
public class CrmContext : DbContext
{
    public CrmContext (DbContextOptions<CrmContext> options): base(options)
    {
    }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Contact> Contacts => Set<Contact>();

}