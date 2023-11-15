using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CRM.Data;
using CRM.Services;

var builder = WebApplication.CreateBuilder(args);

// Add DB
builder.Services.AddDbContext<CrmContext>(options =>
    options.UseSqlite("Data Source=crm.db"));

// Add services
builder.Services.AddScoped<ClientService>();
builder.Services.AddScoped<ContactService>();
builder.Services.AddScoped<UserService>();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.CreateDbIfNotExists();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// add CORS policy
app.UseCors("AllowAll");

app.MapControllers();

app.Run();
