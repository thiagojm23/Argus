<template lang="pug">
article.middle.wdt50.center.border.purple-border
  h5.center-align Login
  form(@submit.prevent="logar")
    .field.large
      input(v-model="model.nomeUsuario" placeholder="Usuário")
    .field.large
      input(type="password" v-model="model.senha" placeholder="Senha")
    button.small-round.center.wdt50.large.purple10.white-text(type="submit")
      span(v-if="!logando") ENTRAR
      progress(v-else)
</template>
<script lang="ts" setup>
import { ref } from 'vue'
import router from '../router'
import api from '../services/api'

const model = ref({
  nomeUsuario: '',
  senha: '',
})
const logando = ref(false)

async function logar() {
  if (!model.value.nomeUsuario || !model.value.senha) {
    return ui('#erroCredenciaisLogar')
  }

  try {
    logando.value = true
    const resposta = await api.post('https://localhost:7090/api/ArgusCloud/Usuario/logar', {
      nomeUsuario: model.value.nomeUsuario,
      senha: model.value.senha,
    })
    if (resposta.status == 200) {
      localStorage.setItem('logado', 'true')
      localStorage.setItem('usuario', JSON.stringify(resposta.data))
      router.push('dashboard')
    }
  } catch (err) {
    console.error('Erro ao logar', err)
    ui('#erroLogar')
  } finally {
    logando.value = false
  }
}
</script>
