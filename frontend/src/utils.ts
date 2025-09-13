export default {
  debounce(func: Function, delay: number) {
    let timeoutId: number
    return function (this: unknown, ...args: any[]) {
      clearTimeout(timeoutId)
      timeoutId = setTimeout(() => func.apply(this, args), delay)
    }
  },
}
