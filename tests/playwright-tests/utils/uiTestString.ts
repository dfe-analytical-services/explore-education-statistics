import environment from './env';

export default function uiTestString(str: string): string {
  return `UI Test - ${str} - ${environment.RUN_IDENTIFIER}`;
}
