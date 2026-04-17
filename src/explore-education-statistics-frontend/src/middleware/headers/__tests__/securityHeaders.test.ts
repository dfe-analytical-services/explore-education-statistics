import securityHeaders from '@frontend/middleware/headers/securityHeaders';
import runMiddleware from '@frontend/middleware/__tests__/util/runMiddleware';

describe('securityHeaders', () => {
  test('ignores any requests to /_next/* paths', async () => {
    const response = await getResponseForUrl('https://my-env/_next/a-url');
    expect(response.headers.get('X-Content-Type-Options')).toBeNull();
  });

  test('applies an X-Content-Type-Options header for all other requests', async () => {
    const response = await getResponseForUrl(
      'https://my-env/any-other-request',
    );
    expect(response.headers.get('X-Content-Type-Options')).toBe('nosniff');
  });

  async function getResponseForUrl(url: string) {
    const middlewareResult = await runMiddleware(securityHeaders, url);
    return middlewareResult!;
  }
});
