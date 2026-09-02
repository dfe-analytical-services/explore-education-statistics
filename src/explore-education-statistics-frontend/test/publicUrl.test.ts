import { frontendApi } from '@common/services/api';

describe('PUBLIC_URL', () => {
  test('never ends with a slash, so paths can be appended to it', () => {
    expect(process.env.PUBLIC_URL).not.toMatch(/\/$/);
  });

  test('builds the frontend api base url', () => {
    expect(frontendApi.baseURL).toBe('http://localhost:3000/api');
  });
});
