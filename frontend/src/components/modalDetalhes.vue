<template lang="pug">
dialog(id="modalDetalhes")
  h6 20 processos que mais consomem memória no computador&nbsp;
    span.primary-text NOTEBOOK-THIAGO
  ul.list.border.top-margin.no-space.large-margin#header
    li(v-for="processo in processesDetails" :key="processo.id")
      details
        summary 
          span.max {{ processo.nome }}
          i expand_more
        ul.list.border.left-padding.small-space
          li.gap0(v-for="(obj) in Object.entries(processo).filter(([key]) => key !== 'subProcessos')" :key="processo.id + processo.numeroThreads")
            span.primary-text {{ LABELS_PROCESSES[obj[0]] }}:&nbsp;
            | {{ obj[1] }}&nbsp;
            span(v-if="exibirValorEmGB(obj[0])") /
              span.primary-text &nbsp;{{ calcularGB(processo.memoriaVirtualUsoMB) }}
          ul.list.border.no-space#header
            li
              details
                summary 
                  span.max Subprocessos
                  i expand_more
                ul.left-padding.list.border.no-space#header(v-for="subProcesso in processo.subProcessos")
                  li
                    details
                      summary 
                        span.max {{ subProcesso.nome }}
                        i expand_more
                      ul.list.border.left-padding.small-space
                        li.gap0(v-for="(obj) in Object.entries(subProcesso)" :key="subProcesso.id + subProcesso.numeroThreads")
                          span.primary-text {{ LABELS_PROCESSES[obj[0]] }}:&nbsp;
                          | {{ obj[1] }}&nbsp;
                          span(v-if="exibirValorEmGB(obj[0])") /
                            span.primary-text &nbsp;{{ calcularGB(subProcesso.memoriaVirtualUsoMB) }}
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

function calcularGB(memoriaMB: number): string {
  return (memoriaMB / 1024).toFixed(2) + ' GB'
}

function exibirValorEmGB(key: string): boolean {
  return ['memoriaVirtualUsoMB'].includes(key)
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
