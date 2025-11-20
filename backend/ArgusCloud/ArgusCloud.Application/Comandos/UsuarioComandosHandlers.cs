using Argus.Agent;
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

        public class GerarTokenDefinitivoComandoHandler(IUsuarioRepositorio usuarioRepositorio, IMaquinaRepositorio maquinaRepositorio, ITokenServico tokenServico) : IRequestHandler<GerarTokenDefinitivoComando, AdicionarUsuarioContratoResponse?>
        {
            private readonly IUsuarioRepositorio _usuarioRepositorio = usuarioRepositorio;
            private readonly IMaquinaRepositorio _maquinaRepositorio = maquinaRepositorio;
            private readonly ITokenServico _tokenServico = tokenServico;

            public async Task<AdicionarUsuarioContratoResponse?> Handle(GerarTokenDefinitivoComando request, CancellationToken cancellationToken)
            {
                if (!_tokenServico.ValidarToken(request.Contrato.TokenTemporario))
                    return null;


                var maquinaId = Guid.NewGuid();
                var tokenDefinitivoAgente = _tokenServico.GerarTokenDefinitivoAgente(request.Usuario.Id, maquinaId);

                request.Usuario.TokenAgenteHash = TokenServico.HashearComSha256(tokenDefinitivoAgente);
                request.Usuario.DataExpiracao = DateTime.Now.AddYears(1);
                request.Usuario.MaquinaId = maquinaId;

                var maquinaEntidade = ConverterParaMaquina(maquinaId, request.Usuario.Id, request.Contrato);

                await _maquinaRepositorio.CadastrarMaquinaAsync(maquinaEntidade);
                await _usuarioRepositorio.AtualizarAsync(request.Usuario);


                var retorno = new AdicionarUsuarioContratoResponse { Nome = request.Usuario.Nome, Senha = request.Contrato.Senha, TokenAgente = tokenDefinitivoAgente, DataExpiracao = request.Usuario.DataExpiracao };

                return retorno;
            }
        }

        public class ValidarTokenComandoHandler(ITokenServico tokenServico, IUsuarioRepositorio usuarioRepositorio) : IRequestHandler<ValidarTokenComando, bool>
        {
            private readonly ITokenServico _tokenServico = tokenServico;
            private readonly IUsuarioRepositorio _usuarioRepositorio = usuarioRepositorio;

            public async Task<bool> Handle(ValidarTokenComando request, CancellationToken cancellationToken)
            {
                var usuario = await _usuarioRepositorio.ObterPorMaquinaIdAsync(Guid.Parse(request.MaquinaId));

                if (usuario == null) return false;

                var tokenRequestHash = TokenServico.HashearComSha256(request.Token);

                var tokenBateComDB = tokenRequestHash == usuario.TokenAgenteHash;
                var claimsValidas = _tokenServico.ValidarClaimsToken(request.Token, request.MaquinaId, request.TipoCliente);

                return tokenBateComDB && claimsValidas;
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

        public static Maquina ConverterParaMaquina(Guid maquinaId, Guid usuarioId, RegistrarUsuarioContrato contrato)
        {
            return new Maquina
            {
                Id = maquinaId,
                UsuarioId = usuarioId,
                Nome = contrato.NomeMaquina,
                SistemaOperacional = contrato.SistemaOperacional
            };
        }
    }
}
