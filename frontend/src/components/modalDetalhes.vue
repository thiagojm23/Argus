<template lang="pug">
dialog(id="modalDetalhes")
  h6 20 processos que mais consomem memória no computador&nbsp;
    span.primary-text NOTEBOOK-THIAGO
  ul.list.border.top-margin.no-space.large-margin#header
    li(v-for="process in processesDetails" :key="process.id")
      details
        summary 
          span.max {{ process.nome }}
          i expand_more
        ul.list.border.left-padding.small-space
          li.gap0(v-for="(obj) in Object.entries(process)" :key="process.id + process.numeroThreads")
            span.primary-text {{ LABELS_PROCESSES[obj[0]] }}:&nbsp;
            | {{ obj[1] }}
  nav.right-align.no-space
    button.transparent.link(data-ui="#modalDetalhes") FECHAR
</template>
<script lang="ts" setup>
import { reactive } from 'vue'
import { type IProcessos } from '../interfaces/dashboard'
import { LABELS_PROCESSES } from '../dadosEstaticos'

const processesDetails = reactive<IProcessos[]>([])

function abrirModalDetalhes(processes: IProcessos[]) {
  Object.assign(processesDetails, processes)
  ui('#modalDetalhes')
}

defineExpose({ abrirModalDetalhes })
</script>
<style scoped>
#header > li::before {
  background-color: #4e198f !important;
}
summary::before {
  background-color: #4e198f !important;
}
</style>
