<template lang="pug">
Login(v-if="!usuarioLogado")
Dashborad(v-else)
</template>
<script setup lang="ts">
import Dashborad from './components/dashborad.vue'
import Login from './components/login.vue'

const usuarioLogado = false
</script>
