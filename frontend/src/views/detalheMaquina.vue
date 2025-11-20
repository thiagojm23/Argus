<template lang="pug">
.center.middle.wd90
  article.hg70.large-padding.border.purple-border
    .bsi.scroll.surface-container-low.right-padding.small-padding
      h5 Processos da máquina 
        span.primary-text {{ dadosUsuario.maquina.nome }} -&nbsp;
        span Windows 
        span.primary-text {{ dadosUsuario.maquina.sistemaOperacional }}
        span.primary-text.cp(@click="voltarParaDashboard")
          i.extra.left-margin.bottom-margin.small-margin arrow_back
          .tooltip.bottom Voltar para dashboard
      ul.headerProcessos.list.border.top-margin.no-space.large-margin.scroll
        li(v-for="processo in dadosDashboard.processosDetalhados" :key="processo.id")
          DadosProcesso(:processo="processo")
</template>
<script lang="ts" setup>
import dadosDashboard from '../dadosDashboard'
import dadosUsuario from '../dadosUsuario'
import DadosProcesso from '../components/dadosProcesso.vue'
import { onMounted, onUnmounted } from 'vue'
import { signalRServico } from '../services/signalRservico'
import router from '../router'

const maquinaIdObservada = window.location.pathname.split('/').pop() || ''

onMounted(() => {
  solicitarMonitoramentoDetalhado()
})

onUnmounted(() => {
  console.log('Desmontando detalhe máquina', maquinaIdObservada)
  signalRServico.alterarCompartilhamentoDetalhado({
    maquinaIdObservada,
    novoValor: false,
  })
})

function solicitarMonitoramentoDetalhado() {
  const maquinaIdObservada = window.location.pathname.split('/').pop() || ''
  signalRServico.alterarCompartilhamentoDetalhado({
    maquinaIdObservada,
    novoValor: true,
  })
}

function voltarParaDashboard() {
  router.push({ name: 'dashboard' })
}
</script>
