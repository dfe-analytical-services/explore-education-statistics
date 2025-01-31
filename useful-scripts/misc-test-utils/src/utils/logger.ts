import pino from 'pino';

const logger = pino({
  name: 'EES CLI',
  level: 'info',
  transport: {
    target: 'pino-pretty',
    options: {
      colorize: true,
    },
  },
});
export default logger;
