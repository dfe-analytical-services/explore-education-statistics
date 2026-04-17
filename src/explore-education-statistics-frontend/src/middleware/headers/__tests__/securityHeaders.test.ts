import { NextRequest } from 'next/server';
import securityHeaders from '@frontend/middleware/headers/securityHeaders';

describe('securityHeaders', () => {
  test('ignores any requests to /_next/* paths', async () => {
    const response = getResponseForUrl('https://my-env/_next/a-url');
    expect(response.headers.get('X-Content-Type-Options')).toBeNull();
  });

  test('applies an X-Content-Type-Options header for all other requests', async () => {
    const response = getResponseForUrl('https://my-env/any-other-request');
    expect(response.headers.get('X-Content-Type-Options')).toBe('nosniff');
  });

  function getResponseForUrl(url: string) {
    return securityHeaders(new NextRequest(url));
  }
});
