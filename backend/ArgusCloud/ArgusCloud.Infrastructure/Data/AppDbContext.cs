using Argus.Agent;
using ArgusCloud.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArgusCloud.Infrastructure.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Maquina> Maquinas { get; set; }

    }
}
