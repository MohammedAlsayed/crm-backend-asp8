using CRM.Helpers;
using CRM.Models;
using Microsoft.AspNetCore.Identity;

namespace CRM.Data;
public static class DbInitializer
{
    public static void Initialize(DbAppContext context)
    {

        if (!context.Clients.Any())
        {
            PopulateClients("./DB/Client_Accounts.csv", context);
        }
        if(!context.Contacts.Any()){
            PopulateContacts("./DB/Contacts/", context);
            PopulateContacts1("./DB/contacts.csv", context);
        }
        return;
        
    }
    private static void PopulateClients(string filePath, DbAppContext context)
    {
        try
        {
            using (StreamReader reader = new StreamReader(filePath))
            {   
                // skip the first line
                reader.ReadLine();

                List<Client> clients = new List<Client>();

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] values = line.Split(',');
                    string arabicName = values[2];
                    string EnName = values[3];
                    string website = values[4];
                    string phone = values[5];
                    string city = values[7];

                    // create client in database
                    Client newClient = new Client
                    {
                        ArName = arabicName,
                        EnName = EnName,
                        Website = website,
                        Phone = phone,
                        City = city
                    };
                    clients.Add(newClient);
                }
                context.Clients.AddRange(clients);
                context.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading CSV file: {ex.Message}");
        }
    }

    private static void PopulateContacts(string folderPath, DbAppContext context){
        // read file names in directory and populate contacts table with each file name as fk to client id 
        // and the file content as the contact name
        // skip the first line
        foreach (string file in Directory.EnumerateFiles(folderPath, "*.csv"))
        {

            string content = File.ReadAllText(file);
            string[] lines = content.Split(Environment.NewLine);

            int clientId = int.Parse(Path.GetFileNameWithoutExtension(file));
            Client? client = context.Clients.Where(c => c.Id == clientId).FirstOrDefault();

            foreach(string line in lines[1..]){

                string[] values = line.Split(',');
                string arName = values[1];
                string enName = values[2];
                string grade = values[3];
                string email = values[4];
                string department = values[5];
                string phone = values[6];

                if(client != null){
                    Contact newContact = new Contact{
                        ArName = arName,
                        EnName = enName,
                        Grade = grade,
                        Email = email,
                        Department = department,
                        Phone = phone,
                        ClientId = clientId
                    };
                    Console.WriteLine(newContact);
                    context.Contacts.Add(newContact);
                    context.SaveChanges();
                }                

            }
        }
    }

    private static void PopulateContacts1(string filePath, DbAppContext context){
        try
        {
            using (StreamReader reader = new StreamReader(filePath))
            {   
                // skip the first line
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] values = line.Split(',');
                    string client = values[0];
                    var clientName = client.Split(':')[1].Trim();
                    string englishName = values[1].Trim();

                    var clientObj = context.Clients.FirstOrDefault(c => c.EnName.ToLower() == clientName.ToLower());
                    if (clientObj == null){
                        Console.WriteLine($"Client {clientName} not found");
                        continue;
                    }
                    if(englishName == ""){
                        Console.WriteLine($"Contact {englishName} is empty");
                        continue;
                    }
                    // create contact in database
                    Contact newContact = new Contact
                    {
                        EnName = englishName,
                        ClientId = clientObj.Id
                    };

                    var isNewContact = context.Contacts.FirstOrDefault(c => c.EnName.ToLower() == englishName.ToLower());
                    if(isNewContact == null){
                        context.Add(newContact);
                        context.SaveChanges();
                    }
                }
                
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading CSV file: {ex.Message}");
        }
    }

    private static void PopulateUsers(string filePath, DbAppContext context){
        
    }

    public static async Task SeedRoles(IApplicationBuilder applicationBuilder){
        using(var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
        {
            var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            if (!await roleManager.RoleExistsAsync(UserRoles.Manager))
                await roleManager.CreateAsync(new IdentityRole(UserRoles.Manager));
            if (!await roleManager.RoleExistsAsync(UserRoles.User))
                await roleManager.CreateAsync(new IdentityRole(UserRoles.User));
            

        }

    }
}
