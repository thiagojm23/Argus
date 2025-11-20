import { createRouter, createWebHistory } from 'vue-router'
import { appRotas } from './appRotas'
import { authRotas } from './authRotas'
import utils from '@/utils'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [...appRotas, ...authRotas],
})

router.beforeEach((to, from, next) => {
  const isAuthenticated = localStorage.getItem('logado') === 'true'

  if (to.matched.some((record) => record.meta.requiresAuth) && !isAuthenticated) {
    utils.limparDadosSessao()
    next({ name: 'login' })
  } else if (to.name === 'login' && isAuthenticated) {
    next({ name: 'dashboard' })
  } else {
    next()
  }
})

export default router
