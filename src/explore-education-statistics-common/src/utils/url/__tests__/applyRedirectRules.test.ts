import applyRedirectRules from '@common/utils/url/applyRedirectRules';
import _redirectService, { Redirects } from '@common/services/redirectService';

jest.mock('@common/services/redirectService');
const redirectService = _redirectService as jest.Mocked<
  typeof _redirectService
>;

function generateTestName(url: string, result: string) {
  return url === result
    ? `URL: ${url} is unchanged`
    : `URL: ${url} is transformed to: ${result}`;
}

describe('applyRedirectRules', () => {
  beforeEach(() => {
    redirectService.list.mockImplementation(async () => {
      const mockRedirects: Redirects = {
        methodologies: [
          {
            fromSlug: 'test-methodology',
            toSlug: 'test-methodology-revised',
          },
        ],
        publications: [
          {
            fromSlug: 'test-publication',
            toSlug: 'test-publication-revised',
          },
        ],
      };
      return Promise.resolve(mockRedirects);
    });
  });

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
  ])(
    `${generateTestName('%s', '%s')}`,
    async (url: string, result: string | undefined) => {
      expect(await applyRedirectRules(url)).toEqual(result);
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
    `${generateTestName('%s', '%s')}`,
    async (url: string, result: string | undefined) => {
      expect(await applyRedirectRules(url)).toEqual(result);
    },
  );

  test.each([
    [
      'https://www.explore-education-statistics.service.gov.uk/data-catalogue',
      'https://explore-education-statistics.service.gov.uk/data-catalogue',
    ],
    [
      'https://www.admin.explore-education-statistics.service.gov.uk/',
      'https://admin.explore-education-statistics.service.gov.uk/',
    ],
  ])(
    `${generateTestName('%s', '%s')}`,
    async (url: string, result: string | undefined) => {
      expect(await applyRedirectRules(url)).toEqual(result);
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
    [
      'https://explore-education-statistics.service.gov.uk/find-statistics/test-publication',
      'https://explore-education-statistics.service.gov.uk/find-statistics/test-publication-revised',
    ],
    [
      'https://explore-education-statistics.service.gov.uk/find-statistics/this-publication-slug-should-not-change',
      'https://explore-education-statistics.service.gov.uk/find-statistics/this-publication-slug-should-not-change',
    ],
  ])(
    `${generateTestName('%s', '%s')}`,
    async (url: string, result: string | undefined) => {
      expect(await applyRedirectRules(url)).toEqual(result);
    },
  );
});
