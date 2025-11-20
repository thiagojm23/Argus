<template lang="pug">
.center.middle.wd70
  article.medium-padding.border.purple-border
    nav.scroll
      span.max.large-text Seu computador
        br
        span.small-text {{ dadosUsuario.maquina.nome }}
      label.switch.icon
        p.small-margin.right-margin.large-text Exibir processos para todos
        input(type="checkbox" @change="exibirProcessosDebounce" v-model="dadosUsuario.exporProcessos")
        span
          i visibility
      .space
      label.switch.icon(:class="{ 'medium-opacity': !dadosUsuario.exporProcessos }")
        p.small-margin.right-margin.large-text Permitir espelharem seus processos
        input(type="checkbox" @change="espelharProcessosDebounce" v-model="dadosUsuario.permiteEspelharemProcessos" :disabled="!dadosUsuario.exporProcessos")
        span
          i screen_share
  article.hg60.large-padding.border.purple-border
    .bsi.scroll.medium-height.surface-container-low.small-round.right-padding.small-padding
      table.border.large-space.large-text
        thead.fixed
          tr.wsnw
            th Nome PC
            th Tempo em monitoramento
            th Top 20 processos
        tbody
          tr.wsnw(v-for="usuario in 1")
            td {{ usuario }}
            td {{ `Descrição ${usuario}` }}
            td
              a.link.underline(v-if="dadosUsuario.exporProcessos" @click="exibirProcessosDetalhados(dadosUsuario.maquina.id)") {{ concatenarProcessos(dadosDashboard.top4Processos) }}
ModalDetalhes(ref="modalDetalhes")
</template>
<script setup lang="ts">
import { onMounted, ref } from 'vue'
import ModalDetalhes from '../components/modalDetalhes.vue'
import { signalRServico } from '../services/signalRservico'
import dadosUsuario from '../dadosUsuario'
import utils from '../utils'
import dadosDashboard from '../dadosDashboard'

const modalDetalhes = ref<InstanceType<typeof ModalDetalhes>>()

const exibirProcessosDebounce = utils.debounce(() => {
  signalRServico.alterarProcessosVisiveisParaRede(dadosUsuario.exporProcessos)
  console.log('Chamada API exibir processos', dadosUsuario.exporProcessos)
}, 3000)
const espelharProcessosDebounce = utils.debounce(() => {
  console.log('Chamada API espelhar processos', dadosUsuario.permiteEspelharemProcessos)
}, 3000)

onMounted(() => {
  signalRServico.iniciarConexao()
})

function exibirProcessosDetalhados(maquinaIdObservada: string) {
  signalRServico.alterarCompartilhamentoDetalhado({ maquinaIdObservada, novoValor: true })
  modalDetalhes.value?.abrirModalDetalhes(maquinaIdObservada)
}

function concatenarProcessos(processos: string[]): string {
  return processos.map((p) => p.split('-')[0].trim()).join(' -- ')
}
</script>
