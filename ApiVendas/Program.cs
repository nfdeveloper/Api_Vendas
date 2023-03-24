using ApiVendas.Context;
using ApiVendas.Models;
using FastReport.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => 
{
	c.SwaggerDoc("v1", new OpenApiInfo{ Title = "ApiFornecedor", Version="v1",Description="Api para consulta de Vendas e Pedidos." });
	
	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
	{
		Name = "Authorization",
		Type = SecuritySchemeType.ApiKey,
		Scheme = "Bearer",
		BearerFormat = "JWT",
		In = ParameterLocation.Header,
		Description = "Header de autorização JWT usando o esquema Bearer.\r\n\r\nInforme 'Bearer'[espaço] e o seu Token.\r\n\r\nExemplo: \'Bearer 12345abcde\'",
	});
	c.AddSecurityRequirement(new OpenApiSecurityRequirement
	{		
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			new string[] {}
		}
	});
});

var postgresConnection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseNpgsql(postgresConnection, opt => opt.SetPostgresVersion(new Version(9, 3))));

builder.Services.AddIdentity<ObrasUser, IdentityRole>()
	.AddEntityFrameworkStores<AppDbContext>()
	.AddDefaultTokenProviders();

builder.Services.AddAuthentication(
				JwtBearerDefaults.AuthenticationScheme).
				AddJwtBearer(options =>
				 options.TokenValidationParameters = new TokenValidationParameters
				 {
					 ValidateIssuer = true,
					 ValidateAudience = true,
					 ValidateLifetime = true,
					 ValidAudience = builder.Configuration["TokenConfiguration:Audience"],
					 ValidIssuer = builder.Configuration["TokenConfiguration:Issuer"],
					 ValidateIssuerSigningKey = true,
					 IssuerSigningKey = new SymmetricSecurityKey(
						 Encoding.UTF8.GetBytes(builder.Configuration["Jwt:key"]))
				 });

builder.Services.Configure<IdentityOptions>(options =>
{
	// Default Password settings.
	options.Password.RequireDigit = false; // N�mero
	options.Password.RequireLowercase = false; //letra em Caixa Baixa
	options.Password.RequireNonAlphanumeric = false; // letra
	options.Password.RequireUppercase = false; // letra em Caixa Alta
	options.Password.RequiredLength = 5; // Tamanho m�nimo
	options.Password.RequiredUniqueChars = 0; // Caracteres especiais
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
