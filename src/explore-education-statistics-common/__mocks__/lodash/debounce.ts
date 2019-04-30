export default (fn: Function, timeout: number) => (...args: unknown[]) => {
  setTimeout(() => {
    fn(...args);
  }, timeout);
};
