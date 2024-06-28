import getExternality from '@common/utils/url/getExternality';

describe('getExternality', () => {
  test.each([
    'https://explore-education-statistics.service.gov.uk',
    'https://EXPLORE-eDuCaTiOn-statistics.service.gov.uk',
    'https://EXPLORE-eDuCaTiOn-statistics.service.gov.uk/find-statistics',
  ])('returns internal-public for public URL', (url: string) => {
    expect(getExternality(url)).toBe('internal');
  });

  test.each([
    'https://admin.explore-education-statistics.service.gov.uk',
    'https://ADMIN.EXPLORE-eDuCaTiOn-statistics.service.gov.uk',
    'https://admin.explore-education-statistics.service.gov.uk/some-admin-route',
  ])('returns external-admin for admin URL', (url: string) => {
    expect(getExternality(url)).toBe('external-admin');
  });

  test.each([
    'https://www.gov.uk',
    'https://www.GOV.uk',
    'https://www.gov.uk/browse/housing-local-services',
  ])('returns external-trusted for gov.uk links', (url: string) => {
    expect(getExternality(url)).toBe('external-trusted');
  });

  test('returns external for external URL', () => {
    expect(getExternality('https://stackoverflow.com/')).toBe('external');
  });

  test.each(['/find-statistics', '?page1'])(
    'returns internal for relative URL',
    (url: string) => {
      expect(getExternality(url)).toBe('internal');
    },
  );
});
