import * as redirectPagesModule from '@frontend/middleware/pages/redirectPages';
import _redirectService, {
  Redirects,
} from '@frontend/services/redirectService';
import { NextResponse } from 'next/server';
import runMiddleware from './util/runMiddleware';

jest.mock('@frontend/services/redirectService');
const redirectService = _redirectService as jest.Mocked<
  typeof _redirectService
>;

describe('redirectPages', () => {
  interface TestDataWithoutRedirect {
    routePattern: string;
    fromSlug: string;
  }

  interface TestDataWithRedirect extends TestDataWithoutRedirect {
    toSlug: string;
  }

  const slugPlaceholder = '{slug}';

  const testRedirects: Redirects = {
    methodologies: [{ fromSlug: 'original-slug-1', toSlug: 'updated-slug-1' }],
    publications: [{ fromSlug: 'original-slug-2', toSlug: 'updated-slug-2' }],
    releases: [{ fromSlug: 'original-slug-3', toSlug: 'updated-slug-3' }],
  };

  const testRedirectData: TestDataWithRedirect[] = [
    {
      routePattern: `methodology/${slugPlaceholder}`,
      fromSlug: testRedirects.methodologies[0].fromSlug,
      toSlug: testRedirects.methodologies[0].toSlug,
    },
    {
      routePattern: `find-statistics/${slugPlaceholder}`,
      fromSlug: testRedirects.publications[0].fromSlug,
      toSlug: testRedirects.publications[0].toSlug,
    },
    {
      routePattern: `find-statistics/${slugPlaceholder}/data-guidance`,
      fromSlug: testRedirects.publications[0].fromSlug,
      toSlug: testRedirects.publications[0].toSlug,
    },
    {
      routePattern: `find-statistics/${slugPlaceholder}/prerelease-access-list`,
      fromSlug: testRedirects.publications[0].fromSlug,
      toSlug: testRedirects.publications[0].toSlug,
    },
    {
      routePattern: `find-statistics/publication-slug/${slugPlaceholder}`,
      fromSlug: testRedirects.releases[0].fromSlug,
      toSlug: testRedirects.releases[0].toSlug,
    },
    {
      routePattern: `find-statistics/publication-slug/${slugPlaceholder}/data-guidance`,
      fromSlug: testRedirects.releases[0].fromSlug,
      toSlug: testRedirects.releases[0].toSlug,
    },
    {
      routePattern: `find-statistics/publication-slug/${slugPlaceholder}/prerelease-access-list`,
      fromSlug: testRedirects.releases[0].fromSlug,
      toSlug: testRedirects.releases[0].toSlug,
    },
    {
      routePattern: `data-tables/${slugPlaceholder}`,
      fromSlug: testRedirects.publications[0].fromSlug,
      toSlug: testRedirects.publications[0].toSlug,
    },
    {
      routePattern: `data-tables/${slugPlaceholder}/fast-track/child-route`,
      fromSlug: testRedirects.publications[0].fromSlug,
      toSlug: testRedirects.publications[0].toSlug,
    },
    {
      routePattern: `data-tables/${slugPlaceholder}/permalink/child-route`,
      fromSlug: testRedirects.publications[0].fromSlug,
      toSlug: testRedirects.publications[0].toSlug,
    },
    {
      routePattern: `data-tables/publication-slug/${slugPlaceholder}`,
      fromSlug: testRedirects.releases[0].fromSlug,
      toSlug: testRedirects.releases[0].toSlug,
    },
  ];

  const testRedirectDataWithFromSlugMatchingAnotherRedirectType: TestDataWithoutRedirect[] =
    [
      {
        routePattern: `methodology/${slugPlaceholder}`,
        fromSlug: testRedirects.publications[0].fromSlug,
      },
      {
        routePattern: `find-statistics/${slugPlaceholder}`,
        fromSlug: testRedirects.methodologies[0].fromSlug,
      },
      {
        routePattern: `find-statistics/${slugPlaceholder}/data-guidance`,
        fromSlug: testRedirects.methodologies[0].fromSlug,
      },
      {
        routePattern: `find-statistics/${slugPlaceholder}/prerelease-access-list`,
        fromSlug: testRedirects.methodologies[0].fromSlug,
      },
      {
        routePattern: `find-statistics/publication-slug/${slugPlaceholder}`,
        fromSlug: testRedirects.publications[0].fromSlug,
      },
      {
        routePattern: `find-statistics/publication-slug/${slugPlaceholder}/data-guidance`,
        fromSlug: testRedirects.publications[0].fromSlug,
      },
      {
        routePattern: `find-statistics/publication-slug/${slugPlaceholder}/prerelease-access-list`,
        fromSlug: testRedirects.publications[0].fromSlug,
      },
      {
        routePattern: `data-tables/${slugPlaceholder}`,
        fromSlug: testRedirects.releases[0].fromSlug,
      },
      {
        routePattern: `data-tables/${slugPlaceholder}/fast-track/child-route`,
        fromSlug: testRedirects.releases[0].fromSlug,
      },
      {
        routePattern: `data-tables/${slugPlaceholder}/permalink/child-route`,
        fromSlug: testRedirects.releases[0].fromSlug,
      },
      {
        routePattern: `data-tables/publication-slug/${slugPlaceholder}`,
        fromSlug: testRedirects.publications[0].fromSlug,
      },
    ];

  let redirectSpy: jest.SpyInstance;
  const nextSpy = jest.spyOn(NextResponse, 'next');

  let redirectPages: typeof redirectPagesModule.default;

  function buildRoute(routePattern: string, slug: string): string {
    return routePattern.replace(slugPlaceholder, slug);
  }

  beforeEach(async () => {
    await jest.isolateModulesAsync(async () => {
      import('@frontend/middleware/pages/redirectPages').then(module => {
        redirectPages = module.default;
      });

      import('next/server').then(module => {
        redirectSpy = jest.spyOn(module.NextResponse, 'redirect');
      });
    });
  });

  test('does not check for redirects for non release or methodology pages', async () => {
    await runMiddleware(redirectPages, 'https://my-env/find-statistics');

    expect(redirectService.list).not.toHaveBeenCalled();
    expect(redirectSpy).not.toHaveBeenCalled();
    expect(nextSpy).toHaveBeenCalledTimes(1);

    await runMiddleware(redirectPages, 'https://my-env/methodology');

    expect(redirectService.list).not.toHaveBeenCalled();
    expect(redirectSpy).not.toHaveBeenCalled();
    expect(nextSpy).toHaveBeenCalledTimes(2);

    await runMiddleware(redirectPages, 'https://my-env/data-tables');

    expect(redirectService.list).not.toHaveBeenCalled();
    expect(redirectSpy).not.toHaveBeenCalled();
    expect(nextSpy).toHaveBeenCalledTimes(3);
  });

  test('redirects urls with uppercase characters to lowercase', async () => {
    redirectService.list.mockResolvedValue(testRedirects);
    await runMiddleware(redirectPages, 'https://my-env/Find-Statistics');

    expect(redirectSpy).toHaveBeenCalledTimes(1);
    expect(redirectSpy).toHaveBeenCalledWith(
      expect.objectContaining({
        href: 'https://my-env/find-statistics',
      }),
      301,
    );
    expect(nextSpy).not.toHaveBeenCalled();

    await runMiddleware(
      redirectPages,
      'https://my-env/find-statistics/RELEASE-NAME?testParam=Something',
    );

    expect(redirectSpy).toHaveBeenCalledTimes(2);
    expect(redirectSpy).toHaveBeenCalledWith(
      expect.objectContaining({
        href: 'https://my-env/find-statistics/release-name?testParam=Something',
      }),
      301,
    );
    expect(nextSpy).not.toHaveBeenCalled();
  });

  it.each(testRedirectData)(
    'does not re-request the list of redirects once it has been fetched',
    async redirectData => {
      redirectService.list.mockResolvedValue(testRedirects);

      const route = buildRoute(redirectData.routePattern, 'original-slug');

      await runMiddleware(redirectPages, `https://my-env/${route}`);

      expect(redirectService.list).toHaveBeenCalledTimes(1);

      const anotherRoute = buildRoute(
        redirectData.routePattern,
        'another-slug',
      );

      await runMiddleware(redirectPages, `https://my-env/${anotherRoute}`);

      expect(redirectService.list).toHaveBeenCalledTimes(1);
    },
  );

  it.each(testRedirectData)(
    'redirects the request when the slug for the requested page has changed',
    async redirectData => {
      redirectService.list.mockResolvedValue(testRedirects);

      const fromRoute = buildRoute(
        redirectData.routePattern,
        redirectData.fromSlug,
      );
      const toRoute = buildRoute(
        redirectData.routePattern,
        redirectData.toSlug,
      );

      await runMiddleware(redirectPages, `https://my-env/${fromRoute}`);

      expect(redirectSpy).toHaveBeenCalledTimes(1);
      expect(redirectSpy).toHaveBeenCalledWith(
        expect.objectContaining({
          href: `https://my-env/${toRoute}`,
        }),
        301,
      );
      expect(nextSpy).not.toHaveBeenCalled();
    },
  );

  it.each(testRedirectData)(
    'redirects the request when the slug for the requested page has changed, and the url has a trailing slash',
    async redirectData => {
      redirectService.list.mockResolvedValue(testRedirects);

      const fromRoute = buildRoute(
        redirectData.routePattern,
        redirectData.fromSlug,
      );
      const toRoute = buildRoute(
        redirectData.routePattern,
        redirectData.toSlug,
      );

      await runMiddleware(redirectPages, `https://my-env/${fromRoute}/`);

      expect(redirectSpy).toHaveBeenCalledTimes(1);
      expect(redirectSpy).toHaveBeenCalledWith(
        expect.objectContaining({
          href: `https://my-env/${toRoute}/`,
        }),
        301,
      );
      expect(nextSpy).not.toHaveBeenCalled();
    },
  );

  it.each(testRedirectData)(
    'does not redirect when the slug for the requested page has not changed',
    async redirectData => {
      redirectService.list.mockResolvedValue(testRedirects);

      const route = buildRoute(
        redirectData.routePattern,
        'slug-with-no-redirect',
      );

      await runMiddleware(redirectPages, `https://my-env/${route}`);

      expect(redirectSpy).not.toHaveBeenCalled();
      expect(nextSpy).toHaveBeenCalledTimes(1);
    },
  );

  it.each(testRedirectData)(
    'does not redirect if the `fromSlug` only partially matches',
    async redirectData => {
      redirectService.list.mockResolvedValue(testRedirects);

      const routeWithRedirectSlugSubstring = buildRoute(
        redirectData.routePattern,
        redirectData.fromSlug.substring(0, redirectData.fromSlug.length - 1),
      );

      await runMiddleware(
        redirectPages,
        `https://my-env/${routeWithRedirectSlugSubstring}`,
      );

      expect(redirectSpy).not.toHaveBeenCalled();
      expect(nextSpy).toHaveBeenCalledTimes(1);

      const routeWithRedirectSlugWithSuffix = buildRoute(
        redirectData.routePattern,
        `${redirectData.fromSlug}-extra-suffix`,
      );

      await runMiddleware(
        redirectPages,
        `https://my-env/${routeWithRedirectSlugWithSuffix}`,
      );

      expect(redirectSpy).not.toHaveBeenCalled();
      expect(nextSpy).toHaveBeenCalledTimes(2);
    },
  );

  it.each(testRedirectData)(
    'redirects with query params',
    async redirectData => {
      redirectService.list.mockResolvedValue(testRedirects);

      const fromRoute = buildRoute(
        redirectData.routePattern,
        redirectData.fromSlug,
      );
      const toRoute = buildRoute(
        redirectData.routePattern,
        redirectData.toSlug,
      );

      await runMiddleware(
        redirectPages,
        `https://my-env/${fromRoute}?search=something`,
      );

      expect(redirectSpy).toHaveBeenCalledTimes(1);
      expect(redirectSpy).toHaveBeenCalledWith(
        expect.objectContaining({
          href: `https://my-env/${toRoute}?search=something`,
        }),
        301,
      );
      expect(nextSpy).not.toHaveBeenCalled();
    },
  );

  it.each(testRedirectDataWithFromSlugMatchingAnotherRedirectType)(
    'does not redirect when the slug matches a `fromSlug` in a different redirect type',
    async redirectData => {
      redirectService.list.mockResolvedValue(testRedirects);

      const route = buildRoute(
        redirectData.routePattern,
        redirectData.fromSlug,
      );

      await runMiddleware(redirectPages, `https://my-env/${route}`);

      expect(redirectSpy).not.toHaveBeenCalled();
      expect(nextSpy).toHaveBeenCalledTimes(1);
    },
  );

  it.each(testRedirectData)(
    'redirects with uppercase characters',
    async redirectData => {
      redirectService.list.mockResolvedValue(testRedirects);

      const fromRoute = buildRoute(
        redirectData.routePattern,
        redirectData.fromSlug.toUpperCase(),
      );
      const toRoute = buildRoute(
        redirectData.routePattern,
        redirectData.toSlug,
      );

      await runMiddleware(redirectPages, `https://my-env/${fromRoute}`);

      expect(redirectSpy).toHaveBeenCalledTimes(1);
      expect(redirectSpy).toHaveBeenCalledWith(
        expect.objectContaining({
          href: `https://my-env/${toRoute}`,
        }),
        301,
      );
      expect(nextSpy).not.toHaveBeenCalled();
    },
  );
});
