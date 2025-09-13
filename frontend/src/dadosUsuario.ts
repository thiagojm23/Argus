import { reactive } from 'vue'
import { type IUsuario } from './interfaces/usuario'

const dadosUsuarioMock = {
  id: 'usr_001',
  nome: 'Usuário Exemplo',
  dataExpiracao: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
  exporProcessos: true,
  maquinaId: 'maq_001',
  permiteEspelharemProcessos: false,
  maquina: {
    id: 'maq_001',
    nome: 'Desktop-01',
    sistemaOperacional: 'Windows 11',
    localizacaoMaquina: 'Escritório',
  },
}

export default reactive<IUsuario>(dadosUsuarioMock)
