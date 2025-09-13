namespace Argus.Agent
{
    public class ProcessoInfoBase
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string NomeExecutavel { get; set; }
        public long MemoriaUsoMB { get; set; }
        public long MemoriaVirtualUsoMB { get; set; }
        public int NumeroThreads { get; set; }
        public double UsoCpuModoUsuario { get; set; }
        public double UsoCpuModoPrivilegiado { get; set; }
        public double UsoCpuTotal { get; set; }
    }

    public class ProcessoInfo : ProcessoInfoBase
    {
        public required List<ProcessoInfoBase> SubProcessos { get; set; }
    }
}
