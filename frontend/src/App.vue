<template lang="pug">
main
  RouterView
  .snackbar.error.top.bold(id="erroCredenciaisLogar") 
    .max Preencha usuário e senha
    i.extra warning
  .snackbar.error.top.bold(id="erroLogar") 
    .max Erro ao logar
    i.extra warning
  .loader-overlay#loader
    .loader-container
      .spinner
      .loader-message
</template>
<script setup lang="ts">
import { RouterView } from 'vue-router'
</script>
