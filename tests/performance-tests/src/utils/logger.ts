import pino from 'pino';

const logger = pino({
  name: 'EES performance tests',
  level: 'info',
  transport: {
    target: 'pino-pretty',
    options: {
      colorize: true,
    },
  },
});
export default logger;
