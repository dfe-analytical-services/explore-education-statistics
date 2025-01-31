import chalk from 'chalk';

export const logColours = {
  info: chalk.cyan,
  error: chalk.red,
};

export function logInfo(message: string): void {
  console.info(logColours.info(message));
}

export function logError(message: string): void {
  console.error(logColours.error(message));
}
