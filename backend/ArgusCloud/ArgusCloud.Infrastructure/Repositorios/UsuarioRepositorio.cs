using ArgusCloud.Domain.Entities;
using ArgusCloud.Domain.Interfaces.Repositorios;
using ArgusCloud.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArgusCloud.Infrastructure.Repositorios
{
    public class UsuarioRepositorio(AppDbContext context) : IUsuarioRepositorio
    {
        protected readonly AppDbContext _context = context;
        protected readonly DbSet<Usuario> _dbSet = context.Set<Usuario>();

        public async Task AtualizarAsync(Usuario usuario)
        {
            _context.Entry(usuario).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<Usuario> CadastrarUsuarioAsync(Usuario usuario)
        {
            await _dbSet.AddAsync(usuario);
            await _context.SaveChangesAsync();
            var usuarioCadastrado = await ObterPorIdAsync(usuario.Id);
            return usuarioCadastrado!;
        }

        public async Task<Usuario?> ObterPorIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }
        public async Task<Usuario?> ObterPorMaquinaIdAsync(Guid maquinaId)
        {
            return await _dbSet.Include(usuario => usuario.Maquina).FirstOrDefaultAsync(usuario => usuario.MaquinaId == maquinaId);
        }

        public async Task<Usuario?> ObterPorNomeAsync(string nome)
        {
            return await _dbSet.Include(usuario => usuario.Maquina).FirstOrDefaultAsync(usuario => usuario.Nome.Equals(nome, StringComparison.Ordinal));
        }

        public async Task<Usuario?> ObterPorNomeESenhaAsync(string nome, string senhaHash)
        {
            return await _dbSet.Include(usuario => usuario.Maquina).FirstOrDefaultAsync(usuario => usuario.Nome == nome && usuario.SenhaHash == senhaHash);
        }
    }
}
