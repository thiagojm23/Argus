import { reactive } from 'vue'
import { type IProcessos } from './interfaces/dashboard'
import dadosTeste from './dadosTeste'

export default reactive<IProcessos[]>(dadosTeste)
