using System.Text;
using ArgusCloud.API.Hubs;
using ArgusCloud.Application.Comandos;
using ArgusCloud.Application.Contratos;
using ArgusCloud.Domain.Entities;
using ArgusCloud.Infrastructure.Data;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

//Config do Mapper
TypeAdapterConfig<UsuarioContrato, Usuario>
    .NewConfig()
    .Ignore(dest => dest.Id)
    //.Map(dest => dest.Nome, src => src.Nome) // ex: nomes diferentes
    .IgnoreNullValues(true);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

//Repos
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CadastrarComando).Assembly));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        builder => builder
            .WithOrigins("http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});


var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key não configurado");
var keyBytes = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Defina como true em produção se usar HTTPS
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = true, // Valide se o emissor é quem você espera
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true, // Valide se o token é para sua aplicação
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true, // Verifique se o token não expirou
        ClockSkew = TimeSpan.Zero // Remove a tolerância padrão de 5 minutos na expiração
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(token) && path.StartsWithSegments("/argus"))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };

    //Caso o front envie no cabeçalho e back na query
    //options.Events = new JwtBearerEvents
    //{
    //    OnMessageReceived = context =>
    //    {
    //        // Tentar extrair o token do cabeçalho Authorization
    //        if (context.Request.Headers.TryGetValue("Authorization", out var authHeader) &&
    //            authHeader.ToString().StartsWith("Bearer "))
    //        {
    //            context.Token = authHeader.ToString().Substring("Bearer ".Length);
    //        }
    //        // Fallback para query string
    //        else if (context.Request.Query.TryGetValue("access_token", out var accessToken))
    //        {
    //            context.Token = accessToken;
    //        }

    //        return Task.CompletedTask;
    //    }
    //};
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowVueApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<MonitoramentoHub>("/argus/monitoramento");

app.MapControllers();

//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    var dbContext = services.GetRequiredService<AppDbContext>();
//    dbContext.Database.Migrate();
//}

app.Run();
