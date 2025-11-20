// src/router/modules/app.routes.ts
import Pagina from '@/pagina.vue'
import type { RouteRecordRaw } from 'vue-router'

export const appRotas: RouteRecordRaw[] = [
  {
    path: '/',
    component: Pagina,
    meta: { requiresAuth: true },
    redirect: '/dashboard',
    children: [
      {
        path: 'dashboard',
        name: 'dashboard',
        component: () => import('@/views/dashboard.vue'),
      },
      {
        path: 'detalheMaquina/:maquinaId',
        name: 'detalheMaquina',
        component: () => import('@/views/detalheMaquina.vue'),
      },
    ],
  },
]
