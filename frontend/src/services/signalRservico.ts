import * as signalR from '@microsoft/signalr'
import type { IProcesso } from '@/interfaces/dashboard'
import router from '@/router'
import dadosUsuario from '@/dadosUsuario'
import dadosDashboard from '@/dadosDashboard'
import utils from '@/utils'
import { reactive } from 'vue'

const HUB_URL = 'https://localhost:7090/argus/monitoramento'

let connection: signalR.HubConnection | null = null

const dadosAtualizados = reactive({
  top4Processos: [] as string[],
  processosDetalhados: [] as IProcesso[],
})

export const signalRServico = {
  async iniciarConexao() {
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
      console.log('Conexão com o Hub SignalR já está ativa.')
      return
    }

    const tipoCliente = 'front'
    const urlComQueryParams = `${HUB_URL}?maquinaId=${dadosUsuario.maquina.id}&tipoCliente=${tipoCliente}`

    connection = new signalR.HubConnectionBuilder()
      .withUrl(urlComQueryParams, {
        withCredentials: true,
      })
      .withAutomaticReconnect()
      .build()

    try {
      await connection.start()
    } catch (err: any) {
      if (ehUsuarioNaoAutorizado(err.message)) {
        utils.limparDadosSessao()
        router.push({ name: 'login' })
        console.error('Não autorizado. Redirecionando para login.', err)
      } else console.error('Erro ao conectar ao Hub SignalR: ', err)
    }

    connection.on('AtualizarProcessosDash', (dados: { maquinaId: string; processos: string[] }) => {
      Object.assign(dadosAtualizados.top4Processos, dados.processos)
      atualizarProcessosRede()
    })

    connection.on(
      'AtualizarProcessosDetalhados',
      (dados: { maquinaId: string; processos: IProcesso[] }) => {
        Object.assign(dadosAtualizados.processosDetalhados, dados.processos)
        atualizarProcessosDetalhados()
      },
    )

    connection.onclose((err) => {
      if (err) {
        if (ehUsuarioNaoAutorizado(err.message)) {
          utils.limparDadosSessao()
          router.push({ name: 'login' })
          console.error('Não autorizado. Redirecionando para login.', err)
        } else console.error('Conexão com o Hub SignalR encerrada com erro: ', err)
      } else {
        console.log('Conexão com o Hub SignalR encerrada.')
      }
    })
  },

  async alterarProcessosVisiveisParaRede(novoValor: boolean) {
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
      try {
        await connection.invoke(
          'AlterarProcessosVisiveisParaRede',
          dadosUsuario.maquina.id,
          novoValor,
        )
      } catch (err: any) {
        if (ehUsuarioNaoAutorizado(err.message)) {
          utils.limparDadosSessao()
          router.push({ name: 'login' })
          console.error('Não autorizado. Redirecionando para login.', err)
        } else console.error('Erro ao alterar processos visíveis para rede: ', err)
      }
    } else console.error('Conexão com o Hub SignalR não está ativa.')
  },

  async alterarCompartilhamentoDetalhado({
    maquinaIdObservada,
    novoValor,
  }: {
    maquinaIdObservada: string
    novoValor: boolean
  }) {
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
      try {
        await connection.invoke('AlterarCompartilhamentoDetalhado', maquinaIdObservada, novoValor)
      } catch (err: any) {
        if (ehUsuarioNaoAutorizado(err.message)) {
          utils.limparDadosSessao()
          router.push({ name: 'login' })
          console.error('Não autorizado. Redirecionando para login.', err)
        } else console.error('Erro ao alterar processos visíveis para rede: ', err)
      }
    } else console.error('Conexão com o Hub SignalR não está ativa.')
  },

  async pararConexao() {
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
      await connection.stop()
      console.log('Conexão com o Hub SignalR encerrada.')
      connection = null
    } else console.log('Nenhuma conexão ativa para encerrar.')
  },
}

function ehUsuarioNaoAutorizado(mensagemErro: string): boolean {
  const regex = /(?=.*rejeitada)(?=.*segurança)/i
  return regex.test(mensagemErro) || /\b401\b/.test(mensagemErro)
}

const atualizarProcessosRede = utils.throttle(() => {
  Object.assign(dadosDashboard.top4Processos, dadosAtualizados.top4Processos)
}, 500)

const atualizarProcessosDetalhados = utils.throttle(() => {
  Object.assign(dadosDashboard.processosDetalhados, dadosAtualizados.processosDetalhados)
}, 500)
