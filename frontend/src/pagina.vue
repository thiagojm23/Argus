<template lang="pug">
header.no-padding.absolute.wd100
  nav.right-align
    button.transparent.square.right-margin(@click="deslogar")
      i.extra logout
RouterView(v-if="usuarioLogado")
</template>
<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { RouterView } from 'vue-router'
import router from './router'
import { type IUsuario } from './interfaces/usuario'
import dadosUsuario from './dadosUsuario'
import utils from './utils'

const usuarioLogado = ref(false)

onMounted(() => {
  buscarUsuarioLogado()
})

function buscarUsuarioLogado() {
  const usuario: IUsuario | null = utils.buscarLocalStorage('usuario')

  if (!usuario || !usuario.maquina?.id) {
    console.error('Usuário não encontrado. Redirecionando para login.')
    utils.limparDadosSessao()
    router.push({ name: 'login' })
  }

  usuarioLogado.value = true
  Object.assign(dadosUsuario, usuario)
}

function deslogar() {
  utils.limparDadosSessao()
  router.push({ name: 'login' })
}
</script>
