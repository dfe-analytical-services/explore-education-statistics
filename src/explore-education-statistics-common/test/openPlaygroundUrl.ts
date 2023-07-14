import { screen } from '@testing-library/react';
import open from 'open';

/**
 * Render the current DOM and open it in the browser (via Testing Playground).
 *
 * @param element Optionally provide a specific element to render.
 */
export default async function openPlaygroundUrl(element?: Element) {
  // Don't try to open in CI if we've left these in by accident
  if (process.env.CI) {
    return;
  }

  // eslint-disable-next-line no-console
  const originalLog = console.log;

  let url = '';

  // eslint-disable-next-line no-console
  console.log = (message: string, ...args: unknown[]) => {
    const match = message.match(/https:\/\/testing-playground\.com.+[^\s$]/);

    if (match) {
      [url] = match;
    }

    originalLog(message, ...args);
  };

  await screen.logTestingPlaygroundURL(element);

  // eslint-disable-next-line no-console
  console.log = originalLog;

  if (!url) {
    fail(
      'Could not load playground URL. Check `screen.logTestingPlaygroundURL` still outputs URL in expected format.',
    );

    return;
  }

  await open(url);
}
