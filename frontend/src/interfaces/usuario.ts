export interface IUsuario {
  id: string
  nome: string
  dataExpiracao: string
  exporProcessos: boolean
  permiteEspelharemProcessos: boolean
  maquina: IMaquina
}

export interface IMaquina {
  id: string
  nome: string
  sistemaOperacional: string
  localizacaoMaquina: string | null
}
