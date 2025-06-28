<template lang="pug">
.login
  .camposParaLogin
    h5.noMargin Login
    input(type="text" placeholder="Usuário")
    input(type="password" placeholder="Senha")

</template>
<style scoped>
.login {
  flex: 1;
  display: flex;
  justify-content: center;
  align-items: center;
}
.camposParaLogin {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  align-items: center;
  padding: 24px;
  border: 2px solid #ccc;
  border-radius: 8px;
  background-color: black;
  width: 40%;
}
.noMargin {
  margin: 0;
}
input {
  width: 100%;
  padding: 8px;
  border: none;
  border-radius: 4px;
}
</style>
