import getUrlOrigin from '@common/utils/url/getUrlOrigin';
import * as hostUrl from '@common/utils/url/hostUrl';

jest.mock('@common/utils/url/hostUrl');

describe('getExternality - public', () => {
  beforeEach(() => {
    jest
      .spyOn(hostUrl, 'getHostUrl')
      .mockReturnValue(
        new URL('https://explore-education-statistics.servce.gov.uk/'),
      );
  });

  test.each([
    'https://explore-education-statistics.service.gov.uk',
    'https://EXPLORE-eDuCaTiOn-statistics.service.gov.uk',
    'https://EXPLORE-eDuCaTiOn-statistics.service.gov.uk/find-statistics',
  ])('returns public for public URL', (url: string) => {
    expect(getUrlOrigin(url)).toBe('public');
  });

  test.each([
    'https://admin.explore-education-statistics.service.gov.uk',
    'https://ADMIN.EXPLORE-eDuCaTiOn-statistics.service.gov.uk',
    'https://admin.explore-education-statistics.service.gov.uk/some-admin-route',
  ])('returns admin for admin URL', (url: string) => {
    expect(getUrlOrigin(url)).toBe('admin');
  });

  test.each([
    'https://www.gov.uk',
    'https://www.GOV.uk',
    'https://www.gov.uk/browse/housing-local-services',
  ])('returns external-trusted for gov.uk links', (url: string) => {
    expect(getUrlOrigin(url)).toBe('external-trusted');
  });

  test('returns external for external URL', () => {
    expect(getUrlOrigin('https://stackoverflow.com/')).toBe('external');
  });

  test.each(['/find-statistics', '?page1'])(
    'returns public for relative URL',
    (url: string) => {
      expect(getUrlOrigin(url)).toBe('public');
    },
  );
});

describe('getExternality - admin', () => {
  beforeEach(() => {
    jest
      .spyOn(hostUrl, 'getHostUrl')
      .mockReturnValue(
        new URL('https://admin.explore-education-statistics.servce.gov.uk/'),
      );
  });

  afterEach(() => {
    jest.clearAllMocks();
  });

  test.each([
    'https://explore-education-statistics.service.gov.uk',
    'https://EXPLORE-eDuCaTiOn-statistics.service.gov.uk',
    'https://EXPLORE-eDuCaTiOn-statistics.service.gov.uk/find-statistics',
  ])('returns public for public URL', (url: string) => {
    expect(getUrlOrigin(url)).toBe('public');
  });

  test.each([
    'https://admin.explore-education-statistics.service.gov.uk',
    'https://ADMIN.EXPLORE-eDuCaTiOn-statistics.service.gov.uk',
    'https://admin.explore-education-statistics.service.gov.uk/some-admin-route',
  ])('returns admin for admin URL', (url: string) => {
    expect(getUrlOrigin(url)).toBe('admin');
  });

  test.each([
    'https://www.gov.uk',
    'https://www.GOV.uk',
    'https://www.gov.uk/browse/housing-local-services',
  ])('returns external-trusted for gov.uk links', (url: string) => {
    expect(getUrlOrigin(url)).toBe('external-trusted');
  });

  test('returns external for external URL', () => {
    expect(getUrlOrigin('https://stackoverflow.com/')).toBe('external');
  });

  test.each(['/find-statistics', '?page1'])(
    'returns admin for relative URL',
    (url: string) => {
      expect(getUrlOrigin(url)).toBe('admin');
    },
  );
});
