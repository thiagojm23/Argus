<template lang="pug">
details
  summary 
    span.max {{ processo.nome }}
    i expand_more
  ul.list.border.left-padding.small-space
    li.gap0(v-for="(obj) in Object.entries(processo).filter(([key]) => key !== 'subProcessos')" :key="processo.id + obj[0]")
      span.primary-text {{ LABELS_PROCESSES[obj[0]].label }}:&nbsp;
      | {{ `${obj[1]}${LABELS_PROCESSES[obj[0]].complemento}` }}&nbsp;
      span(v-if="exibirValorEmGB(obj[0])") /
        span.primary-text &nbsp;{{ calcularGB(processo.memoriaVirtualUsoMB) }}
    ul.list.border.no-space.headerProcessos
      li.fdc.ain.gap0
        nav.paddingSubprocesso(@click="toggleSubProcessos(processo.id)")
          //- summary
          div.max Subprocessos
          i expand_more
        ul.left-padding.list.border.no-space.headerProcessos(v-if="subProcessosVisiveis[processo.id]" v-for="subProcesso in processo.subProcessos" :key="subProcesso.id")
          li.fdc.ain.gap0
            nav.paddingSubprocesso(@click="toggleInformacoesSubprocesso(processo.id, subProcesso.id)")
              //- summary
              div.max {{ subProcesso.nome }}
              i expand_more
            ul.list.border.left-padding.small-space(v-if="subProcessosVisiveis[processo.id].includes(subProcesso.id)")
              li.ain.gap0(v-for="(obj) in Object.entries(subProcesso)" :key="subProcesso.id + obj[0]")
                span.primary-text {{ LABELS_PROCESSES[obj[0]].label }}:&nbsp;
                | {{ `${obj[1]}${LABELS_PROCESSES[obj[0]].complemento}` }}&nbsp;
                span(v-if="exibirValorEmGB(obj[0])") /
                  span.primary-text &nbsp;{{ calcularGB(subProcesso.memoriaVirtualUsoMB) }}
</template>
<script lang="ts" setup>
import { ref } from 'vue'
import { LABELS_PROCESSES } from '../dadosEstaticos'
import { type IProcesso } from '../interfaces/dashboard'

const props = defineProps<{
  processo: IProcesso
}>()

const subProcessosVisiveis = ref<Record<string, number[]>>({})

function calcularGB(memoriaMB: number): string {
  return (memoriaMB / 1024).toFixed(2) + 'GB'
}

function exibirValorEmGB(key: string): boolean {
  return ['memoriaVirtualUsoMB'].includes(key)
}

function toggleSubProcessos(id: number) {
  if (subProcessosVisiveis.value[id]) delete subProcessosVisiveis.value[id]
  else subProcessosVisiveis.value[id] = []
}

function toggleInformacoesSubprocesso(processoId: number, subProcessoId: number) {
  const subProcessos = subProcessosVisiveis.value[processoId]
  if (subProcessos.includes(subProcessoId)) {
    const index = subProcessos.indexOf(subProcessoId)
    if (index > -1) {
      subProcessos.splice(index, 1)
    }
  } else subProcessos.push(subProcessoId)
}
</script>
<style scoped>
.paddingSubprocesso {
  padding: 0 !important;
}
.headerProcessos {
  & > li > nav:not(:only-child) {
    border-bottom: 1px solid #4e198f !important;
    padding: 0.5rem 1rem 0.5rem 1rem !important;
  }
}
</style>
