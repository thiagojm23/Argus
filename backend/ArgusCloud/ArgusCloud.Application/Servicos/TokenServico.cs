using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ArgusCloud.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace ArgusCloud.Application.Servicos
{
    public class TokenServico : ITokenServico
    {
        private readonly IConfiguration _configuration;
        private readonly JwtSecurityTokenHandler _tokenHandler = new();
        private readonly string _issuer;
        private readonly string _audience;
        private readonly SigningCredentials _signingCredentials;
        private readonly TokenValidationParameters _validationParameters;
        private readonly ILogger<TokenServico> _logger;

        public TokenServico(IConfiguration configuration, ILogger<TokenServico> logger)
        {
            _logger = logger;
            _configuration = configuration;

            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key não configurado");
            var keyBytes = Encoding.ASCII.GetBytes(jwtKey);
            _issuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer não configurado");
            _audience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience não configurado");

            var securityKey = new SymmetricSecurityKey(keyBytes);
            _signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            _validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        }

        public string GerarTokenDefinitivoAgente(Guid idUsuario, Guid maquinaId)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, idUsuario.ToString()),
                new("maquinaId", maquinaId.ToString()),
                new("tipoCliente", "agente")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _issuer,
                Expires = DateTime.UtcNow.AddMonths(6),
                Audience = _audience,
                SigningCredentials = _signingCredentials
            };

            var token = _tokenHandler.CreateToken(tokenDescriptor);
            return _tokenHandler.WriteToken(token);
        }

        public string GerarTokenTemporarioAgente(string nomeUsuario)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, nomeUsuario.ToString()),
                new("tipoCliente", "agente")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _issuer,
                Audience = _audience,
                Expires = DateTime.UtcNow.AddMonths(3),
                SigningCredentials = _signingCredentials
            };

            var token = _tokenHandler.CreateToken(tokenDescriptor);
            return _tokenHandler.WriteToken(token);
        }

        //public string GerarAccessTokenFront(Guid usuarioId, string nomeUsuario)
        //{
        //    var claims = new List<Claim>
        //    {
        //        new(ClaimTypes.NameIdentifier, usuarioId.ToString()),
        //        new(ClaimTypes.Name, nomeUsuario),
        //        new("tipoCliente", "front")
        //    };

        //    var tokenDescriptor = new SecurityTokenDescriptor
        //    {
        //        Subject = new ClaimsIdentity(claims),
        //        Issuer = _issuer,
        //        Expires = DateTime.UtcNow.AddMinutes(20),
        //        Audience = _audience,
        //        SigningCredentials = _signingCredentials
        //    };

        //    var token = _tokenHandler.CreateToken(tokenDescriptor);
        //    return _tokenHandler.WriteToken(token);
        //}

        public string GerarTokenFront(Guid usuarioId, string nomeUsuario)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuarioId.ToString()),
                new(ClaimTypes.Name, nomeUsuario),
                new("tipoCliente", "front")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _issuer,
                Expires = DateTime.UtcNow.AddHours(2),
                Audience = _audience,
                SigningCredentials = _signingCredentials
            };

            var token = _tokenHandler.CreateToken(tokenDescriptor);
            return _tokenHandler.WriteToken(token);
        }

        //public string GerarRefreshTokenFront(Guid usuarioId, string nomeUsuario)
        //{
        //    var claims = new List<Claim>
        //    {
        //        new(ClaimTypes.NameIdentifier, usuarioId.ToString()),
        //        new(ClaimTypes.Name, nomeUsuario),
        //        new("tipoCliente", "front")
        //    };

        //    var tokenDescriptor = new SecurityTokenDescriptor
        //    {
        //        Subject = new ClaimsIdentity(claims),
        //        Issuer = _issuer,
        //        Expires = DateTime.UtcNow.AddDays(7),
        //        Audience = _audience,
        //        SigningCredentials = _signingCredentials
        //    };

        //    var token = _tokenHandler.CreateToken(tokenDescriptor);
        //    return _tokenHandler.WriteToken(token);
        //}

        public static string HashearComSha256(string dado)
        {
            var bytes = Encoding.UTF8.GetBytes(dado);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }

        public bool ValidarToken(string token)
        {
            try
            {
                _tokenHandler.ValidateToken(token, _validationParameters, out _);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ValidarClaimsToken(string token, string maquinaId, string tipoCliente)
        {
            if (string.IsNullOrWhiteSpace(token) ||
                string.IsNullOrWhiteSpace(maquinaId) ||
                string.IsNullOrWhiteSpace(tipoCliente))
                return false;

            ClaimsPrincipal claims;
            try
            {
                claims = _tokenHandler.ValidateToken(token, _validationParameters, out _);
            }
            catch
            {
                return false;
            }

            if (claims == null) return false;

            var claimMaquinaId = claims.FindFirst("maquinaId")?.Value;
            var claimTipoCliente = claims.FindFirst("tipoCliente")?.Value;

            if (claimMaquinaId != maquinaId)
            {
                _logger.LogError("Claim maquinaId divergente: claim: {claim}, header: {hrader}", claimMaquinaId, maquinaId);
                return false;
            }

            if (claimTipoCliente != tipoCliente)
            {
                _logger.LogError("Claim tipoCliente divergente: claim: {claim}, header: {hrader}", claimTipoCliente, tipoCliente);
                return false;
            }

            return true;
        }
    }
}
