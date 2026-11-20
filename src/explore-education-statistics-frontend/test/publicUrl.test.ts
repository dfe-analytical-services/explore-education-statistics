import { frontendApi } from '@common/services/api';
import getPublicEnv from '@frontend/utils/publicEnv';

describe('PUBLIC_URL', () => {
  test('never ends with a slash, so paths can be appended to it', () => {
    expect(getPublicEnv('PUBLIC_URL')).not.toMatch(/\/$/);
  });

  test('builds the frontend api base url', () => {
    expect(frontendApi.baseURL).toBe('http://localhost:3000/api');
  });
});
