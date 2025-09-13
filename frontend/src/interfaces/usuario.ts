export interface IUsuario {
  id: string
  nome: string
  dataExpiracao: string
  exporProcessos: boolean
  maquinaId: string | null
  permiteEspelharemProcessos: boolean
  maquina: IMaquina
}

export interface IMaquina {
  id: string
  nome: string
  sistemaOperacional: string
  localizacaoMaquina: string | null
}
