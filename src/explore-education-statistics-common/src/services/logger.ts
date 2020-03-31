const logger = {
  debugGroup(name: string, ...args: unknown[]) {
    if (process.env.NODE_ENV === 'development') {
      // eslint-disable-next-line no-console
      console.group(name, ...args);
    }
  },
  debugGroupEnd() {
    if (process.env.NODE_ENV === 'development') {
      // eslint-disable-next-line no-console
      console.groupEnd();
    }
  },
  debug(...args: unknown[]) {
    if (process.env.NODE_ENV === 'development') {
      // eslint-disable-next-line no-console
      console.log(...args);
    }
  },
};

export default logger;
