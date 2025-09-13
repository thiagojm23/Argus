export interface IProcessos {
  id: number
  nome: string
  nomeExecutavel: string
  memoriaUsoMB: number
  memoriaVirtualUsoMB: number
  numeroThreads: number
  usoCpuModoUsuario: number
  usoCpuModoPrivilegiado: number
  usoCpuTotal: number
  subProcessos: Omit<IProcessos, 'subProcessos'>[]
}
