import './main.css'
import 'beercss'

import { createApp } from 'vue'
import App from './App.vue'
import router from './router'

const app = createApp(App)

app.use(router)
ui('mode', 'dark')
app.mount('#app')
