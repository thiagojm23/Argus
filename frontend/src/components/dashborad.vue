<template lang="pug">
.listagem-pcs
  .dash
    table
      thead
        tr
          th Nome PC
          th Tempo em monitoramento
          th Compartilhando dados
      tbody
        tr(v-for="pc in 5")
          td {{ pc }}
          td {{ `Descrição ${pc}` }}
          td {{ `Compartilhando ${pc}` }}
</template>
<style scoped>
.listagem-pcs {
  flex: 1;
  display: flex;
  justify-content: center;
  align-items: center;
}
.dash {
  padding: 24px;
  border: 2px solid #ccc;
  border-radius: 8px;
  background-color: black;
}
table {
  border-spacing: 30px 10px;
}
</style>
