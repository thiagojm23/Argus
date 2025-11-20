import { reactive } from 'vue'
import { type IUsuario } from './interfaces/usuario'

const objetoInicial: IUsuario = {
  id: '',
  nome: '',
  dataExpiracao: '',
  exporProcessos: false,
  permiteEspelharemProcessos: false,
  maquina: {
    id: '',
    nome: '',
    sistemaOperacional: '',
    localizacaoMaquina: '',
  },
}

const dadosUsuario = reactive<IUsuario>(objetoInicial)

// export function limparDadosUsuario() {
//   Object.assign(dadosUsuario, objetoInicial)
// }

declare global {
  interface Window {
    dadosUsuario: IUsuario
  }
}

window.dadosUsuario = dadosUsuario

export default dadosUsuario
