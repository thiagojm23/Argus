using ArgusCloud.Application.Contratos;
using ArgusCloud.Application.Interfaces;
using ArgusCloud.Application.Servicos;
using ArgusCloud.Domain.Entities;
using ArgusCloud.Domain.Interfaces.Repositorios;
using MediatR;

namespace ArgusCloud.Application.Comandos
{
    public class UsuarioComandosHandlers
    {
        public class CadastrarComandoHandler(IUsuarioRepositorio usuarioRepositorio, ITokenServico tokenServico) : IRequestHandler<CadastrarComando, AdicionarUsuarioContratoResponse>
        {
            private readonly IUsuarioRepositorio _usuarioRepositorio = usuarioRepositorio;
            private readonly ITokenServico _tokenServico = tokenServico;

            public async Task<AdicionarUsuarioContratoResponse> Handle(CadastrarComando request, CancellationToken cancellationToken)
            {
                var senhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);
                var tokenTemporarioAgente = _tokenServico.GerarTokenTemporarioAgente(request.NomeUsuario);

                var novoUsuario = new Usuario { Nome = request.NomeUsuario, SenhaHash = senhaHash, TokenAgenteHash = null, TokenTemporarioAgente = tokenTemporarioAgente };

                novoUsuario = await _usuarioRepositorio.CadastrarUsuarioAsync(novoUsuario);

                var retorno = new AdicionarUsuarioContratoResponse { Nome = novoUsuario.Nome, Senha = request.Senha, TokenAgente = tokenTemporarioAgente, DataExpiracao = null };

                return retorno;
            }
        }

        public class GerarTokenDefinitivoComandoHandler(IUsuarioRepositorio usuarioRepositorio, ITokenServico tokenServico) : IRequestHandler<GerarTokenDefinitivoComando, AdicionarUsuarioContratoResponse?>
        {
            private readonly IUsuarioRepositorio _usuarioRepositorio = usuarioRepositorio;
            private readonly ITokenServico _tokenServico = tokenServico;

            public async Task<AdicionarUsuarioContratoResponse?> Handle(GerarTokenDefinitivoComando request, CancellationToken cancellationToken)
            {
                if (!_tokenServico.ValidarToken(request.TokenTemporario))
                    return null;


                var maquinaId = Guid.NewGuid();
                var tokenDefinitivoAgente = _tokenServico.GerarTokenDefinitivoAgente(request.Usuario.Id, maquinaId);

                request.Usuario.TokenAgenteHash = TokenServico.HashearComSha256(tokenDefinitivoAgente);
                request.Usuario.DataExpiracao = DateTime.Now.AddYears(1);
                request.Usuario.MaquinaId = maquinaId;

                await _usuarioRepositorio.AtualizarAsync(request.Usuario);

                var retorno = new AdicionarUsuarioContratoResponse { Nome = request.Usuario.Nome, Senha = request.Senha, TokenAgente = tokenDefinitivoAgente, DataExpiracao = request.Usuario.DataExpiracao };

                return retorno;
            }
        }

        public class ValidarClaimsTokenComandoHandler(ITokenServico tokenServico) : IRequestHandler<ValidarClaimsTokenComando, bool>
        {
            private readonly ITokenServico _tokenServico = tokenServico;

            public Task<bool> Handle(ValidarClaimsTokenComando request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_tokenServico.ValidarClaimsToken(request.Token, request.MaquinaId, request.TipoCliente));
            }
        }
    }
}
