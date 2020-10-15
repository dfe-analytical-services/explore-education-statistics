type Callback = (...args: unknown[]) => void;

export default (fn: Callback, timeout: number) => (...args: unknown[]) => {
  setTimeout(() => {
    fn(...args);
  }, timeout);
};
