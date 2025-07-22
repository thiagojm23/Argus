using System.Security.Cryptography;
using System.Text;

namespace Argus.Agent
{
    public static class GerenciadorCredencial
    {
        private static readonly byte[] s_entropy = [0x4A, 0xB2, 0x1C, 0xF9, 0x3D, 0xE7, 0x8A, 0x5C];
        private static readonly string s_nomeArquivoCredencial = "agent.token";
        private static readonly string s_appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Argus");

        private static string BuscarArquivoCredencial()
        {
            Directory.CreateDirectory(s_appDataPath);
            return Path.Combine(s_appDataPath, s_nomeArquivoCredencial);
        }

        public static string BuscarCaminhoDados()
        {
            return s_appDataPath;
        }

        public static void SalvarToken(string token)
        {
            var tokenBytes = Encoding.UTF8.GetBytes(token);

            var dadoCriptografado = ProtectedData.Protect(tokenBytes, s_entropy, DataProtectionScope.LocalMachine);

            File.WriteAllBytes(BuscarArquivoCredencial(), dadoCriptografado);
        }

        public static string? LerToken()
        {
            var caminhoArquivo = BuscarArquivoCredencial();
            if (!File.Exists(caminhoArquivo))
                return null;

            try
            {
                var dadoCriptografado = File.ReadAllBytes(caminhoArquivo);

                var tokenBytes = ProtectedData.Unprotect(dadoCriptografado, s_entropy, DataProtectionScope.LocalMachine);

                return Encoding.UTF8.GetString(tokenBytes);
            }
            catch (CryptographicException)
            {
                File.Delete(caminhoArquivo);
                return null;
            }
        }

        public static void DeletarToken()
        {
            var caminhoArquivo = BuscarArquivoCredencial();
            if (File.Exists(caminhoArquivo))
                File.Delete(caminhoArquivo);
        }
    }
}
