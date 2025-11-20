<template lang="pug">
dialog.modal(id="modalDetalhes")
  template(v-if="dadosDashboard.processosDetalhados.length")
    .middle-align
      h6 20 processos que mais consomem memória no computador&nbsp;
        span.primary-text {{ dadosUsuario.maquina.nome }}&nbsp;
          i.vat.cp(@click="abrirDetalhesMaquina") open_in_new
    ul.headerProcessos.list.border.top-margin.no-space.large-margin.scroll
      li(v-for="processo in dadosDashboard.processosDetalhados" :key="processo.id")
        DadosProcesso(:processo="processo")
  template(v-else)
    .middle-align.center-align
      div
        progress.circle.large
        .space
        p.large-text Carregando detalhes dos processos
  nav.right-align.no-space
    button.transparent.link(@click="pararCompartilhamentoDetalhado" data-ui="#modalDetalhes") FECHAR
</template>
<script lang="ts" setup>
import { ref } from 'vue'
import dadosDashboard from '../dadosDashboard'
import dadosUsuario from '../dadosUsuario'
import router from '../router'
import { signalRServico } from '../services/signalRservico'
import DadosProcesso from './dadosProcesso.vue'

const maquinaIdObservada = ref('')

function abrirModalDetalhes(maquinaIdObservadaParm: string) {
  maquinaIdObservada.value = maquinaIdObservadaParm
  ui('#modalDetalhes')
}

function pararCompartilhamentoDetalhado() {
  signalRServico.alterarCompartilhamentoDetalhado({
    maquinaIdObservada: maquinaIdObservada.value,
    novoValor: false,
  })
}

function abrirDetalhesMaquina() {
  router.push({ name: 'detalheMaquina', params: { maquinaId: maquinaIdObservada.value } })
}

defineExpose({ abrirModalDetalhes })
</script>
<style scoped>
body.light {
  li {
    overflow-x: auto;
    scrollbar-width: thin;
    scrollbar-color: #b3b3b3 #ece7eb;
  }
}

body.dark {
  li {
    overflow-x: auto;
    scrollbar-width: thin;
    scrollbar-color: #492450 #2b292d;
  }
}
</style>
