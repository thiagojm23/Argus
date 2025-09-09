export type Processo = {
  id: number
  nome: string
  nomeExecutavel: string
  memoriaUsoMB: number
  memoriaVirtualUsoMB: number
  numeroThreads: number
  usoCpuModoUsuario: number
  usoCpuModoPrivilegiado: number
  usoCpuTotal: number
}

export const LABELS_PROCESSES = {
  id: 'PID',
  nome: 'Nome',
  nomeExecutavel: 'Executável',
  memoriaUsoMB: 'Uso de memória RAM',
  memoriaVirtualUsoMB: 'Uso de memória virtual',
  numeroThreads: 'Threads',
  usoCpuModoUsuario: 'CPU (modo usuário)',
  usoCpuModoPrivilegiado: 'CPU (modo privilegiado)',
  usoCpuTotal: 'CPU (total)',
} as const
