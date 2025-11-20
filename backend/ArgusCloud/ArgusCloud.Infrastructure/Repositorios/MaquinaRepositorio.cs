using Argus.Agent;
using ArgusCloud.Domain.Interfaces.Repositorios;
using ArgusCloud.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArgusCloud.Infrastructure.Repositorios
{
    public class MaquinaRepositorio(AppDbContext context) : IMaquinaRepositorio
    {
        protected readonly AppDbContext _context = context;
        protected readonly DbSet<Maquina> _dbSet = context.Set<Maquina>();

        public async Task AtualizarAsync(Maquina Maquina)
        {
            _context.Entry(Maquina).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<Maquina> CadastrarMaquinaAsync(Maquina Maquina)
        {
            await _dbSet.AddAsync(Maquina);
            await _context.SaveChangesAsync();
            var MaquinaCadastrado = await ObterPorIdAsync(Maquina.Id);
            return MaquinaCadastrado!;
        }

        public async Task<Maquina?> ObterPorIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }
        public async Task<Maquina?> ObterPorMaquinaIdAsync(Guid maquinaId)
        {
            return await _dbSet.FirstOrDefaultAsync(Maquina => Maquina.Id == maquinaId);
        }

        public async Task<Maquina?> ObterPorNomeAsync(string nome)
        {
            return await _dbSet.FirstOrDefaultAsync(Maquina => Maquina.Nome.Equals(nome, StringComparison.Ordinal));
        }
    }
}
