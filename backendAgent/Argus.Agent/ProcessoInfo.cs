namespace Argus.Agent
{
    internal class ProcessoInfo
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public long MemoriaUsoMB { get; set; }
        public long MemoriaVirtualUsoMB { get; set; }
        public int NumeroThreads { get; set; }
        public double UsoCpuModoUsuario { get; set; }
        public double UsoCpuModoPrivilegiado { get; set; }
        public double UsoCpuTotal { get; set; }
    }
}
