import failOnConsole from 'jest-fail-on-console';

export default function errorOnConsoleError() {
  failOnConsole({
    allowMessage: errorMessage =>
      !errorMessage.includes(
        'An update to Tabs inside a test was not wrapped in act(...)',
      ),
    shouldFailOnWarn: false,
  });
}
