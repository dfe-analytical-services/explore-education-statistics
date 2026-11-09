import type * as PublicEnvModule from '@frontend/utils/publicEnv';

describe('publicEnv', () => {
  const originalEnv = process.env;

  function loadModule(): typeof PublicEnvModule {
    // Load a fresh copy each time, as the module
    // caches the page payload after the first read.
    let publicEnvModule: typeof PublicEnvModule | undefined;

    jest.isolateModules(() => {
      publicEnvModule = jest.requireActual('@frontend/utils/publicEnv');
    });

    if (!publicEnvModule) {
      throw new Error('Could not load module');
    }

    return publicEnvModule;
  }

  function renderPayload(content: string) {
    const element = document.createElement('script');
    element.id = '__PUBLIC_ENV__';
    element.type = 'application/json';
    element.textContent = content;
    document.head.appendChild(element);
  }

  beforeEach(() => {
    process.env = { ...originalEnv };
  });

  afterEach(() => {
    process.env = originalEnv;
    document.head.innerHTML = '';
  });

  describe('getPublicEnv', () => {
    test('reads from the page payload when it is present', () => {
      process.env.CONTENT_API_BASE_URL = 'http://from-process-env';
      renderPayload(
        JSON.stringify({ CONTENT_API_BASE_URL: 'http://from-payload' }),
      );

      const { default: getPublicEnv } = loadModule();

      expect(getPublicEnv('CONTENT_API_BASE_URL')).toBe('http://from-payload');
    });

    test('does not fall back to process.env for keys missing from the payload', () => {
      process.env.GA_TRACKING_ID = 'from-process-env';
      renderPayload(JSON.stringify({}));

      const { default: getPublicEnv } = loadModule();

      expect(getPublicEnv('GA_TRACKING_ID')).toBeUndefined();
    });

    test('falls back to process.env when there is no page payload', () => {
      process.env.CONTENT_API_BASE_URL = 'http://from-process-env';

      const { default: getPublicEnv } = loadModule();

      expect(getPublicEnv('CONTENT_API_BASE_URL')).toBe(
        'http://from-process-env',
      );
    });

    test('reads process.env at call time when there is no page payload', () => {
      process.env.CONTENT_API_BASE_URL = 'http://first';

      const { default: getPublicEnv } = loadModule();

      expect(getPublicEnv('CONTENT_API_BASE_URL')).toBe('http://first');

      process.env.CONTENT_API_BASE_URL = 'http://second';

      expect(getPublicEnv('CONTENT_API_BASE_URL')).toBe('http://second');
    });

    test('removes trailing slashes from PUBLIC_URL in the page payload', () => {
      renderPayload(JSON.stringify({ PUBLIC_URL: 'http://public-url//' }));

      const { default: getPublicEnv } = loadModule();

      expect(getPublicEnv('PUBLIC_URL')).toBe('http://public-url');
    });

    test('removes trailing slashes from PUBLIC_URL in process.env', () => {
      process.env.PUBLIC_URL = 'http://public-url/';

      const { default: getPublicEnv } = loadModule();

      expect(getPublicEnv('PUBLIC_URL')).toBe('http://public-url');
    });
  });

  describe('getPublicEnvPayload', () => {
    test('only includes public keys', () => {
      process.env = {
        NODE_ENV: 'test',
        BUILD_NUMBER: '1',
        DEFAULT_CACHE_MAX_AGE_SECONDS: '30',
        APP_ENV: 'Test',
        AZURE_SEARCH_QUERY_KEY: 'secret',
        BASIC_AUTH_PASSWORD: 'secret',
        CONTENT_API_BASE_URL: 'http://content',
        GA_TRACKING_ID: '',
        PUBLIC_URL: 'http://public-url/',
      };

      const { getPublicEnvPayload } = loadModule();

      expect(getPublicEnvPayload()).toEqual({
        APP_ENV: 'Test',
        CONTENT_API_BASE_URL: 'http://content',
        GA_TRACKING_ID: '',
        PUBLIC_URL: 'http://public-url',
      });
    });
  });

  describe('serialisePublicEnvPayload', () => {
    test('escapes characters that could close the script element', () => {
      const { serialisePublicEnvPayload } = loadModule();

      const serialised = serialisePublicEnvPayload({
        PUBLIC_URL: '</script><script>alert(1)</script>',
      });

      expect(serialised).not.toContain('<');
      expect(JSON.parse(serialised)).toEqual({
        PUBLIC_URL: '</script><script>alert(1)</script>',
      });
    });
  });
});
