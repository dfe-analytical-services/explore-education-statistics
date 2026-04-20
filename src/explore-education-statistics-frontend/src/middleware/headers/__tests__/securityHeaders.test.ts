import securityHeaders from '@frontend/middleware/headers/securityHeaders';
import runMiddleware from '@frontend/middleware/__tests__/util/runMiddleware';

describe('securityHeaders', () => {
  test('applies an X-Content-Type-Options header to requests', async () => {
    const response = await getResponseForUrl('https://my-env/any-request');
    expect(response.headers.get('X-Content-Type-Options')).toBe('nosniff');
  });

  async function getResponseForUrl(url: string) {
    const middlewareResult = await runMiddleware(securityHeaders, url);
    return middlewareResult!;
  }
});
