// src/router/modules/auth.routes.ts
import type { RouteRecordRaw } from 'vue-router'

export const authRotas: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'login',
    component: () => import('@/views/login.vue'),
  },
]
