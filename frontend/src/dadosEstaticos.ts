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
  id: {
    label: 'PID',
    complemento: '',
  },
  nome: {
    label: 'Nome',
    complemento: '',
  },
  nomeExecutavel: {
    label: 'Executável',
    complemento: '',
  },
  memoriaUsoMB: {
    label: 'Uso de memória RAM',
    complemento: 'MB',
  },
  memoriaVirtualUsoMB: {
    label: 'Uso de memória virtual',
    complemento: 'MB',
  },
  numeroThreads: {
    label: 'Threads',
    complemento: '',
  },
  usoCpuModoUsuario: {
    label: 'CPU (modo usuário)',
    complemento: '%',
  },
  usoCpuModoPrivilegiado: {
    label: 'CPU (modo privilegiado)',
    complemento: '%',
  },
  usoCpuTotal: {
    label: 'CPU (total)',
    complemento: '%',
  },
} as const
