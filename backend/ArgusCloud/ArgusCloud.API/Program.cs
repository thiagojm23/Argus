using System.Text;
using ArgusCloud.API.Hubs;
using ArgusCloud.Application.Autenticacao.Handlers;
using ArgusCloud.Application.Autenticacao.Requisitos;
using ArgusCloud.Application.Comandos;
using ArgusCloud.Application.Contratos;
using ArgusCloud.Application.Interfaces;
using ArgusCloud.Application.Servicos;
using ArgusCloud.Domain.Entities;
using ArgusCloud.Domain.Interfaces.Repositorios;
using ArgusCloud.Infrastructure.Data;
using ArgusCloud.Infrastructure.Repositorios;
using ArgusCloud.Infrastructure.Servicos;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; // adicionada

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
builder.Services.AddSingleton<IAuthorizationHandler, MaquinaIdHandler>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddSingleton<ITokenServico, TokenServico>();
builder.Services.AddSingleton<IProcessoTempoRealServico, InMemoryProcessosTempoRealServico>();

//
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ArgusCloud API", Version = "v1" });
    // Segurança JWT
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Informe o token JWT no formato: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, new List<string>() }
    });
});

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

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequisitoMaquinaId", policy =>
        policy.AddRequirements(new MaquinaIdRequisito()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ArgusCloud API v1");
        c.DisplayRequestDuration();
    });
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
