import failOnConsole from 'jest-fail-on-console';

export default function errorOnConsoleError() {
  failOnConsole({
    allowMessage: errorMessage =>
      !errorMessage.includes('a test was not wrapped in act(...).'),
    shouldFailOnWarn: false,
  });
}
