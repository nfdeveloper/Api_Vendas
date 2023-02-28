using ApiVendas.Context;
using ApiVendas.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var postgresConnection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(postgresConnection, opt => opt.SetPostgresVersion(new Version(9, 3))));

builder.Services.AddIdentity<ObrasUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Default Password settings.
    options.Password.RequireDigit = false; // Número
    options.Password.RequireLowercase = false; //letra em Caixa Baixa
    options.Password.RequireNonAlphanumeric = false; // letra
    options.Password.RequireUppercase = false; // letra em Caixa Alta
    options.Password.RequiredLength = 5; // Tamanho mínimo
    options.Password.RequiredUniqueChars = 0; // Caracteres especiais
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
