export default {
  debounce(func: Function, delay: number) {
    let timeoutId: number
    return function (this: unknown, ...args: any[]) {
      clearTimeout(timeoutId)
      timeoutId = setTimeout(() => func.apply(this, args), delay)
    }
  },
  throttle(func: Function, limit: number) {
    let inThrottle = false
    return function (this: unknown, ...args: any[]) {
      if (!inThrottle) {
        func.apply(this, args)
        inThrottle = true
        setTimeout(() => (inThrottle = false), limit)
      }
    }
  },
  buscarLocalStorage(chave: string) {
    const item = localStorage.getItem(chave)
    return item ? JSON.parse(item) : null
  },
  exibirLoading(mensagem: string) {
    ui('#loader')
    document.querySelector('.loader-message')!.innerHTML = mensagem
  },
  async removerLoading() {
    await new Promise((resolve) => setTimeout(() => resolve(true), 500))
    ui('#loader')
    document.querySelector('.loader-message')!.innerHTML = ''
  },
  limparDadosSessao() {
    localStorage.removeItem('logado')
    localStorage.removeItem('usuario')
  },
}
