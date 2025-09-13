using System.ComponentModel.DataAnnotations;

namespace ArgusCloud.Application.Contratos
{
    public class ProcessoContratoBase
    {
        [Required(ErrorMessage = "Id obrigatório")]
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

    public class ProcessoContrato : ProcessoContratoBase
    {
        public required List<ProcessoContratoBase> SubProcessos { get; set; }
    }
}
