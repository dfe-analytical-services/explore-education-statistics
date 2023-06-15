type Callback = (...args: unknown[]) => void;

const debounce =
  (fn: Callback, timeout: number) =>
  (...args: unknown[]) => {
    setTimeout(() => {
      fn(...args);
    }, timeout);
  };

export default debounce;
