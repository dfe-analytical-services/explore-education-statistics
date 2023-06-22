type Callback = (...args: unknown[]) => void;

export default function debounce(fn: Callback, timeout: number) {
  return (...args: unknown[]) => {
    setTimeout(() => {
      fn(...args);
    }, timeout);
  };
}
