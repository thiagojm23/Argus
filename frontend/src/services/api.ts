import axios from 'axios'
import router from '@/router'

const api = axios.create({
  baseURL: 'https://localhost:7090/',
  withCredentials: true,
})

api.interceptors.response.use(
  (response) => {
    return response
  },
  (error) => {
    if (error.response && error.response.status === 401) {
      localStorage.removeItem('logado')
      router.push({ name: 'login' })
    }

    return Promise.reject(error)
  },
)

export default api
