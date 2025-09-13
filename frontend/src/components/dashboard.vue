<template lang="pug">
header.absolute.wdt100
  nav.right-align
    button.transparent.square.right-margin
      i.extra logout
.center.middle.wd70
  article.medium-padding.border.purple-border
    nav.scroll
      span.max.large-text Seu computador
        br
        span.small-text NOTEBOOK-THIAGO
      label.switch.icon
        p.small-margin.right-margin.large-text Exibir processos para todos
        input(type="checkbox" v-model="dadosUsuario.exporProcessos")
        span
          i visibility
      .space
      label.switch.icon(:class="{ 'medium-opacity': !dadosUsuario.exporProcessos }")
        p.small-margin.right-margin.large-text Permitir espelharem seus processos
        input(type="checkbox" v-model="dadosUsuario.permiteEspelharemProcessos" :disabled="!dadosUsuario.exporProcessos")
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
          tr.wsnw(v-for="usuario in 12")
            td {{ usuario }}
            td {{ `Descrição ${usuario}` }}
            td
              a.link.underline(@click="modalDetalhes.abrirModalDetalhes(temporary)") {{ concatProcesses(dadosDashboard) }}
              //- a.link.underline(@click="modalDetalhes.abrirModalDetalhes(usuario.processos)") {{ concatProcesses(dadosDashboard) }}
ModalDetalhes(ref="modalDetalhes")
</template>
<script setup lang="ts">
import { ref, watch } from 'vue'
import dadosDashboard from '../dadosDashboard'
import { type IProcessos } from '../interfaces/dashboard'
import ModalDetalhes from './modalDetalhes.vue'
import dadosUsuario from '../dadosUsuario'
import utils from '../utils'

const modalDetalhes = ref<InstanceType<typeof ModalDetalhes>>()

const temporary = <IProcessos[]>dadosDashboard

const exibirProcessosDebounce = utils.debounce(() => {
  console.log('Chamada API exibir processos', dadosUsuario.exporProcessos)
}, 3000)
const espelharProcessosDebounce = utils.debounce(() => {
  console.log('Chamada API espelhar processos', dadosUsuario.permiteEspelharemProcessos)
}, 3000)

watch(
  () => dadosUsuario.exporProcessos,
  (valor) => {
    if (!valor) dadosUsuario.permiteEspelharemProcessos = false
    exibirProcessosDebounce()
  },
)
watch(
  () => dadosUsuario.permiteEspelharemProcessos,
  () => {
    espelharProcessosDebounce()
  },
)

function concatProcesses(processes: IProcessos[]): string {
  return processes
    .map((p) => p.nome.split('-')[0].trim())
    .slice(0, 4)
    .join(' -- ')
}
</script>
