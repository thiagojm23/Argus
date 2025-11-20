import { reactive } from 'vue'
import { type IProcesso } from './interfaces/dashboard'

export default reactive({
  top4Processos: [] as string[],
  processosDetalhados: [] as IProcesso[],
})
