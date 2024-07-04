import applyRedirectRules from '@common/utils/url/applyRedirectRules';
import testRedirects from '@common/utils/url/__tests__/__data__/testRedirects';

function generateTestName(url: string, result: string) {
  return url === result
    ? `URL: ${url} is unchanged`
    : `URL: ${url} is transformed to: ${result}`;
}

describe('applyRedirectRules', () => {
  test.each([
    [
      'https://explore-education-statistics.service.gov.uk/',
      'https://explore-education-statistics.service.gov.uk/',
    ],
    [
      'https://explore-education-statistics.service.gov.uk/data-catalogue/',
      'https://explore-education-statistics.service.gov.uk/data-catalogue',
    ],
    [
      'https://explore-education-statistics.service.gov.uk/data-catalogue////',
      'https://explore-education-statistics.service.gov.uk/data-catalogue',
    ],
    ['/', '/'],
    ['/data-catalogue/', '/data-catalogue'],
    ['https://gov.uk/', 'https://gov.uk/'],
    [
      'https://site-with-search-param-and-trailing-slash/?foo=123',
      'https://site-with-search-param-and-trailing-slash/?foo=123',
    ],
  ])(
    `noTrailingSlash: ${generateTestName('%s', '%s')}`,
    async (url: string, result: string | undefined) => {
      expect(await applyRedirectRules(url, testRedirects)).toEqual(result);
    },
  );

  test.each([
    ['/1000', '/'],
    [
      'https://explore-education-statistics.service.gov.uk/data-catalogue/1000',
      'https://explore-education-statistics.service.gov.uk/data-catalogue',
    ],
    [
      'https://explore-education-statistics.service.gov.uk/data-catalogue/1000/1000',
      'https://explore-education-statistics.service.gov.uk/data-catalogue',
    ],
  ])(
    `no /1000: ${generateTestName('%s', '%s')}`,
    async (url: string, result: string | undefined) => {
      expect(await applyRedirectRules(url, testRedirects)).toEqual(result);
    },
  );

  test.each([
    [
      'https://www.explore-education-statistics.service.gov.uk/data-catalogue',
      'https://explore-education-statistics.service.gov.uk/data-catalogue',
    ],
    [
      'https://www.admin.explore-education-statistics.service.gov.uk/',
      'https://www.admin.explore-education-statistics.service.gov.uk/',
    ],
    [
      'https://www.some-external-site.com/',
      'https://www.some-external-site.com/',
    ],
  ])(
    `no www: ${generateTestName('%s', '%s')}`,
    async (url: string, result: string | undefined) => {
      expect(await applyRedirectRules(url, testRedirects)).toEqual(result);
    },
  );

  test.each([
    [
      'https://explore-education-statistics.service.gov.uk/methodology/test-methodology',
      'https://explore-education-statistics.service.gov.uk/methodology/test-methodology-revised',
    ],
    [
      'https://explore-education-statistics.service.gov.uk/methodology/this-methodology-slug-should-not-change',
      'https://explore-education-statistics.service.gov.uk/methodology/this-methodology-slug-should-not-change',
    ],
    ['/methodology/test-methodology/', '/methodology/test-methodology-revised'],
    [
      '/methodology/this-methodology-slug-should-not-change/',
      '/methodology/this-methodology-slug-should-not-change',
    ],
    [
      'https://explore-education-statistics.service.gov.uk/find-statistics/test-publication',
      'https://explore-education-statistics.service.gov.uk/find-statistics/test-publication-revised',
    ],
    [
      'https://explore-education-statistics.service.gov.uk/find-statistics/this-publication-slug-should-not-change',
      'https://explore-education-statistics.service.gov.uk/find-statistics/this-publication-slug-should-not-change',
    ],
  ])(
    `no ContentAPI redirect: ${generateTestName('%s', '%s')}`,
    async (url: string, result: string | undefined) => {
      expect(await applyRedirectRules(url, testRedirects)).toEqual(result);
    },
  );
});
