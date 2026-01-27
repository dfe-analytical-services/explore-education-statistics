import * as redirectPagesModule from '@frontend/middleware/pages/redirectPages';
import _redirectService, {
  Redirects,
} from '@frontend/services/redirectService';
import { NextResponse } from 'next/server';
import { Dictionary } from '@common/types';
import runMiddleware from './util/runMiddleware';

jest.mock('@frontend/services/redirectService');
const redirectService = _redirectService as jest.Mocked<
  typeof _redirectService
>;

describe('redirectPages', () => {
  interface OldSlugNewSlugPair {
    oldSlug: string;
    newSlug: string;
  }

  interface NonRedirectedCase {
    oldSlugsByPlaceholder: Dictionary<string>;
  }

  interface RedirectedCase {
    oldSlugNewSlugPairsByPlaceholder: Dictionary<OldSlugNewSlugPair>;
  }

  interface RoutePatternTestCases {
    routePattern: string;
    redirectedCases: RedirectedCase[];
    nonRedirectedCases: NonRedirectedCase[];
  }

  interface NonRedirectedCaseTestData {
    routePattern: string;
    oldSlugsByPlaceholder: Dictionary<string>;
  }

  interface RedirectedCaseTestData {
    routePattern: string;
    oldSlugNewSlugPairsByPlaceholder: Dictionary<OldSlugNewSlugPair>;
  }

  const methodologySlugPlaceholder = '{methodology-slug}';
  const publicationSlugPlaceholder = '{publication-slug}';
  const releaseSlugPlaceholder = '{release-slug}';

  const testRedirects: Redirects = {
    methodologyRedirects: [
      {
        fromSlug: 'original-methodology-slug-1',
        toSlug: 'updated-methodology-slug-1',
      },
    ],
    publicationRedirects: [
      {
        fromSlug: 'original-publication-slug-1',
        toSlug: 'updated-publication-slug-1',
      },
      {
        fromSlug: 'original-publication-slug-3',
        toSlug: 'updated-publication-slug-3',
      },
    ],
    releaseRedirectsByPublicationSlug: {
      'updated-publication-slug-1': [
        {
          fromSlug: 'original-release-slug-1',
          toSlug: 'updated-release-slug-1',
        },
      ],
      'original-publication-slug-2': [
        {
          fromSlug: 'original-release-slug-1',
          toSlug: 'updated-release-slug-1',
        },
      ],
    },
  };

  const mixedPublicationReleasePageRedirectedCases: RedirectedCase[] = [
    // Publication slug IS NOT latest + Release slug IS NOT latest
    {
      oldSlugNewSlugPairsByPlaceholder: {
        [publicationSlugPlaceholder]: {
          oldSlug: 'original-publication-slug-1',
          newSlug: 'updated-publication-slug-1',
        },
        [releaseSlugPlaceholder]: {
          oldSlug: 'original-release-slug-1',
          newSlug: 'updated-release-slug-1',
        },
      },
    },
    // Publication slug IS NOT latest + Release slug IS latest
    {
      oldSlugNewSlugPairsByPlaceholder: {
        [publicationSlugPlaceholder]: {
          oldSlug: 'original-publication-slug-1',
          newSlug: 'updated-publication-slug-1',
        },
        [releaseSlugPlaceholder]: {
          oldSlug: 'updated-release-slug-1',
          newSlug: 'updated-release-slug-1',
        },
      },
    },
    // Publication slug IS NOT latest + Release slug HAS NO redirect
    {
      oldSlugNewSlugPairsByPlaceholder: {
        [publicationSlugPlaceholder]: {
          oldSlug: 'original-publication-slug-1',
          newSlug: 'updated-publication-slug-1',
        },
        [releaseSlugPlaceholder]: {
          oldSlug: 'original-release-slug-2',
          newSlug: 'original-release-slug-2',
        },
      },
    },
    // Publication slug IS NOT latest + Publication HAS NO Release redirects
    {
      oldSlugNewSlugPairsByPlaceholder: {
        [publicationSlugPlaceholder]: {
          oldSlug: 'original-publication-slug-3',
          newSlug: 'updated-publication-slug-3',
        },
        [releaseSlugPlaceholder]: {
          oldSlug: 'original-release-slug-1',
          newSlug: 'original-release-slug-1',
        },
      },
    },
    // Publication slug IS latest + Release slug IS NOT latest
    {
      oldSlugNewSlugPairsByPlaceholder: {
        [publicationSlugPlaceholder]: {
          oldSlug: 'updated-publication-slug-1',
          newSlug: 'updated-publication-slug-1',
        },
        [releaseSlugPlaceholder]: {
          oldSlug: 'original-release-slug-1',
          newSlug: 'updated-release-slug-1',
        },
      },
    },
    // Publication slug HAS NO redirect + Release slug IS NOT latest
    {
      oldSlugNewSlugPairsByPlaceholder: {
        [publicationSlugPlaceholder]: {
          oldSlug: 'original-publication-slug-2',
          newSlug: 'original-publication-slug-2',
        },
        [releaseSlugPlaceholder]: {
          oldSlug: 'original-release-slug-1',
          newSlug: 'updated-release-slug-1',
        },
      },
    },
  ];
  const mixedPublicationReleasePageNonRedirectedCases: NonRedirectedCase[] = [
    // Publication slug IS latest + Release slug IS latest
    {
      oldSlugsByPlaceholder: {
        [publicationSlugPlaceholder]: 'updated-publication-slug-1',
        [releaseSlugPlaceholder]: 'updated-release-slug-1',
      },
    },
    // Publication slug IS latest + Release slug HAS NO redirect
    {
      oldSlugsByPlaceholder: {
        [publicationSlugPlaceholder]: 'updated-publication-slug-1',
        [releaseSlugPlaceholder]: 'original-release-slug-2',
      },
    },
    // Publication slug IS latest + Publication HAS NO Release redirects
    {
      oldSlugsByPlaceholder: {
        [publicationSlugPlaceholder]: 'updated-publication-slug-3',
        [releaseSlugPlaceholder]: 'original-release-slug-1',
      },
    },
    // Publication slug HAS NO redirect + Release slug IS latest
    {
      oldSlugsByPlaceholder: {
        [publicationSlugPlaceholder]: 'original-publication-slug-2',
        [releaseSlugPlaceholder]: 'updated-release-slug-1',
      },
    },
    // Publication slug HAS NO redirect + Release slug HAS NO redirect
    {
      oldSlugsByPlaceholder: {
        [publicationSlugPlaceholder]: 'original-publication-slug-2',
        [releaseSlugPlaceholder]: 'original-release-slug-2',
      },
    },
    // Publication slug HAS NO redirect + Publication HAS NO Release redirects
    {
      oldSlugsByPlaceholder: {
        [publicationSlugPlaceholder]: 'original-publication-slug-4',
        [releaseSlugPlaceholder]: 'original-release-slug-1',
      },
    },
    // Matches redirect slugs from another redirect type
    {
      oldSlugsByPlaceholder: {
        [publicationSlugPlaceholder]: 'original-methodology-slug-1',
        [releaseSlugPlaceholder]: 'original-methodology-slug-1',
      },
    },
  ];

  const methodologyPageTestData: RoutePatternTestCases = {
    routePattern: `methodology/${methodologySlugPlaceholder}`,
    redirectedCases: [
      {
        oldSlugNewSlugPairsByPlaceholder: {
          [methodologySlugPlaceholder]: {
            oldSlug: 'original-methodology-slug-1',
            newSlug: 'updated-methodology-slug-1',
          },
        },
      },
    ],
    nonRedirectedCases: [
      {
        oldSlugsByPlaceholder: {
          [methodologySlugPlaceholder]: 'original-methodology-slug-2',
        },
      },
      // Matches redirect slugs from another redirect type
      {
        oldSlugsByPlaceholder: {
          [methodologySlugPlaceholder]: 'original-publication-slug-1',
        },
      },
    ],
  };

  const findStatisticsPublicationPageTestData: RoutePatternTestCases = {
    routePattern: `find-statistics/${publicationSlugPlaceholder}`,
    redirectedCases: [
      {
        oldSlugNewSlugPairsByPlaceholder: {
          [publicationSlugPlaceholder]: {
            oldSlug: 'original-publication-slug-1',
            newSlug: 'updated-publication-slug-1',
          },
        },
      },
    ],
    nonRedirectedCases: [
      {
        oldSlugsByPlaceholder: {
          [publicationSlugPlaceholder]: 'original-publication-slug-2',
        },
      },
      // Matches redirect slugs from another redirect type
      {
        oldSlugsByPlaceholder: {
          [publicationSlugPlaceholder]: 'original-methodology-slug-1',
        },
      },
    ],
  };

  const findStatisticsPublicationReleasesPageTestData: RoutePatternTestCases = {
    routePattern: `find-statistics/${publicationSlugPlaceholder}/releases`,
    redirectedCases: findStatisticsPublicationPageTestData.redirectedCases,
    nonRedirectedCases:
      findStatisticsPublicationPageTestData.nonRedirectedCases,
  };

  const findStatisticsReleasePageTestData: RoutePatternTestCases = {
    routePattern: `find-statistics/${publicationSlugPlaceholder}/${releaseSlugPlaceholder}`,
    redirectedCases: mixedPublicationReleasePageRedirectedCases,
    nonRedirectedCases: mixedPublicationReleasePageNonRedirectedCases,
  };

  const findStatisticsReleaseExplorePageTestData: RoutePatternTestCases = {
    routePattern: `find-statistics/${publicationSlugPlaceholder}/${releaseSlugPlaceholder}/explore`,
    redirectedCases: findStatisticsReleasePageTestData.redirectedCases,
    nonRedirectedCases: findStatisticsReleasePageTestData.nonRedirectedCases,
  };

  const findStatisticsReleaseMethodologyPageTestData: RoutePatternTestCases = {
    routePattern: `find-statistics/${publicationSlugPlaceholder}/${releaseSlugPlaceholder}/methodology`,
    redirectedCases: findStatisticsReleasePageTestData.redirectedCases,
    nonRedirectedCases: findStatisticsReleasePageTestData.nonRedirectedCases,
  };

  const findStatisticsReleaseHelpPageTestData: RoutePatternTestCases = {
    routePattern: `find-statistics/${publicationSlugPlaceholder}/${releaseSlugPlaceholder}/help`,
    redirectedCases: findStatisticsReleasePageTestData.redirectedCases,
    nonRedirectedCases: findStatisticsReleasePageTestData.nonRedirectedCases,
  };

  const findStatisticsReleaseUpdatesPageTestData: RoutePatternTestCases = {
    routePattern: `find-statistics/${publicationSlugPlaceholder}/${releaseSlugPlaceholder}/updates`,
    redirectedCases: findStatisticsReleasePageTestData.redirectedCases,
    nonRedirectedCases: findStatisticsReleasePageTestData.nonRedirectedCases,
  };

  const dataTablesPublicationPageTestData: RoutePatternTestCases = {
    routePattern: `data-tables/${publicationSlugPlaceholder}`,
    redirectedCases: [
      {
        oldSlugNewSlugPairsByPlaceholder: {
          [publicationSlugPlaceholder]: {
            oldSlug: 'original-publication-slug-1',
            newSlug: 'updated-publication-slug-1',
          },
        },
      },
    ],
    nonRedirectedCases: [
      {
        oldSlugsByPlaceholder: {
          [publicationSlugPlaceholder]: 'original-publication-slug-2',
        },
      },
      // Matches redirect slugs from another redirect type
      {
        oldSlugsByPlaceholder: {
          [publicationSlugPlaceholder]: 'original-methodology-slug-1',
        },
      },
    ],
  };

  const dataTablesReleasePageTestData: RoutePatternTestCases = {
    routePattern: `data-tables/${publicationSlugPlaceholder}/${releaseSlugPlaceholder}`,
    redirectedCases: mixedPublicationReleasePageRedirectedCases,
    nonRedirectedCases: mixedPublicationReleasePageNonRedirectedCases,
  };

  const routePatternTestCases: RoutePatternTestCases[] = [
    methodologyPageTestData,
    findStatisticsPublicationPageTestData,
    findStatisticsPublicationReleasesPageTestData,
    findStatisticsReleasePageTestData,
    findStatisticsReleaseExplorePageTestData,
    findStatisticsReleaseMethodologyPageTestData,
    findStatisticsReleaseHelpPageTestData,
    findStatisticsReleaseUpdatesPageTestData,
    dataTablesPublicationPageTestData,
    dataTablesReleasePageTestData,
  ];

  const redirectedCases: RedirectedCaseTestData[] =
    routePatternTestCases.flatMap(td => {
      return td.redirectedCases.map(
        (rc): RedirectedCaseTestData => ({
          routePattern: td.routePattern,
          oldSlugNewSlugPairsByPlaceholder: rc.oldSlugNewSlugPairsByPlaceholder,
        }),
      );
    });

  const nonRedirectedCases: NonRedirectedCaseTestData[] =
    routePatternTestCases.flatMap(td => {
      return td.nonRedirectedCases.map(
        (rc): NonRedirectedCaseTestData => ({
          routePattern: td.routePattern,
          oldSlugsByPlaceholder: rc.oldSlugsByPlaceholder,
        }),
      );
    });

  // These are routes that have slugs defined that would normally be redirected, but shouldn't be due
  // to them being for a child-route that is not explicitly captured in the redirect patterns we look for.
  const childRouteCases: NonRedirectedCaseTestData[] = [
    {
      routePattern: `methodology/${methodologySlugPlaceholder}/child-route`,
      oldSlugsByPlaceholder: {
        [methodologySlugPlaceholder]: 'original-methodology-slug-1',
      },
    },
    {
      routePattern: `find-statistics/${publicationSlugPlaceholder}/releases/child-route`,
      oldSlugsByPlaceholder: {
        [publicationSlugPlaceholder]: 'original-publication-slug-1',
      },
    },
    {
      routePattern: `find-statistics/${publicationSlugPlaceholder}/${releaseSlugPlaceholder}/child-route`,
      oldSlugsByPlaceholder: {
        [publicationSlugPlaceholder]: 'original-publication-slug-1',
        [releaseSlugPlaceholder]: 'original-release-slug-1',
      },
    },
    {
      routePattern: `find-statistics/${publicationSlugPlaceholder}/${releaseSlugPlaceholder}/updates/child-route`,
      oldSlugsByPlaceholder: {
        [publicationSlugPlaceholder]: 'original-publication-slug-1',
        [releaseSlugPlaceholder]: 'original-release-slug-1',
      },
    },
    {
      routePattern: `find-statistics/${publicationSlugPlaceholder}/${releaseSlugPlaceholder}/explore/child-route`,
      oldSlugsByPlaceholder: {
        [publicationSlugPlaceholder]: 'original-publication-slug-1',
        [releaseSlugPlaceholder]: 'original-release-slug-1',
      },
    },
    {
      routePattern: `find-statistics/${publicationSlugPlaceholder}/${releaseSlugPlaceholder}/methodology/child-route`,
      oldSlugsByPlaceholder: {
        [publicationSlugPlaceholder]: 'original-publication-slug-1',
        [releaseSlugPlaceholder]: 'original-release-slug-1',
      },
    },
    {
      routePattern: `find-statistics/${publicationSlugPlaceholder}/${releaseSlugPlaceholder}/help/child-route`,
      oldSlugsByPlaceholder: {
        [publicationSlugPlaceholder]: 'original-publication-slug-1',
        [releaseSlugPlaceholder]: 'original-release-slug-1',
      },
    },
    {
      routePattern: `data-tables/${publicationSlugPlaceholder}/${releaseSlugPlaceholder}/child-route`,
      oldSlugsByPlaceholder: {
        [publicationSlugPlaceholder]: 'original-publication-slug-1',
        [releaseSlugPlaceholder]: 'original-release-slug-1',
      },
    },
  ];

  let redirectSpy: jest.SpyInstance;
  const nextSpy = jest.spyOn(NextResponse, 'next');

  let redirectPages: typeof redirectPagesModule.default;

  function buildRoute(
    routePattern: string,
    slugsByPlaceholder: Dictionary<string>,
  ): string {
    let route = routePattern;

    Object.entries(slugsByPlaceholder).forEach(([key, value]) => {
      route = route.replace(key, value);
    });

    return route;
  }

  function buildOldRoute(
    routePattern: string,
    oldSlugNewSlugPairsByPlaceholder: Dictionary<OldSlugNewSlugPair>,
  ): string {
    return buildRoute(
      routePattern,
      Object.fromEntries(
        Object.entries(oldSlugNewSlugPairsByPlaceholder).map(([k, v]) => [
          k,
          v.oldSlug,
        ]),
      ),
    );
  }

  function buildNewRoute(
    routePattern: string,
    oldSlugNewSlugPairsByPlaceholder: Dictionary<OldSlugNewSlugPair>,
  ): string {
    return buildRoute(
      routePattern,
      Object.fromEntries(
        Object.entries(oldSlugNewSlugPairsByPlaceholder).map(([k, v]) => [
          k,
          v.newSlug,
        ]),
      ),
    );
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

  describe('General Checks', () => {
    it.each(redirectedCases)(
      'does not re-request the list of redirects once it has been fetched',
      async redirectedCase => {
        redirectService.list.mockResolvedValue(testRedirects);

        const route = buildOldRoute(
          redirectedCase.routePattern,
          redirectedCase.oldSlugNewSlugPairsByPlaceholder,
        );

        await runMiddleware(redirectPages, `https://my-env/${route}`);

        expect(redirectService.list).toHaveBeenCalledTimes(1);

        const anotherRoute = buildOldRoute(
          redirectedCase.routePattern,
          redirectedCase.oldSlugNewSlugPairsByPlaceholder,
        );

        await runMiddleware(redirectPages, `https://my-env/${anotherRoute}`);

        expect(redirectService.list).toHaveBeenCalledTimes(1);
      },
    );

    test('does not check for redirects for non release/publication/methodology pages', async () => {
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
  });

  describe('Does Redirect', () => {
    test('redirects non-redirect urls with uppercase characters to lowercase', async () => {
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

    it.each(redirectedCases)(
      'redirects the request when the slug for the requested page has changed',
      async redirectedCase => {
        redirectService.list.mockResolvedValue(testRedirects);

        const oldRoute = buildOldRoute(
          redirectedCase.routePattern,
          redirectedCase.oldSlugNewSlugPairsByPlaceholder,
        );
        const newRoute = buildNewRoute(
          redirectedCase.routePattern,
          redirectedCase.oldSlugNewSlugPairsByPlaceholder,
        );

        await runMiddleware(redirectPages, `https://my-env/${oldRoute}`);

        expect(redirectSpy).toHaveBeenCalledTimes(1);
        expect(redirectSpy).toHaveBeenCalledWith(
          expect.objectContaining({
            href: `https://my-env/${newRoute}`,
          }),
          301,
        );
        expect(nextSpy).not.toHaveBeenCalled();
      },
    );

    it.each(redirectedCases)(
      'redirects the request when the slug for the requested page has changed, and the url has a trailing slash',
      async redirectedCase => {
        redirectService.list.mockResolvedValue(testRedirects);

        const oldRoute = buildOldRoute(
          redirectedCase.routePattern,
          redirectedCase.oldSlugNewSlugPairsByPlaceholder,
        );
        const newRoute = buildNewRoute(
          redirectedCase.routePattern,
          redirectedCase.oldSlugNewSlugPairsByPlaceholder,
        );

        await runMiddleware(redirectPages, `https://my-env/${oldRoute}/`);

        expect(redirectSpy).toHaveBeenCalledTimes(1);
        expect(redirectSpy).toHaveBeenCalledWith(
          expect.objectContaining({
            href: `https://my-env/${newRoute}/`,
          }),
          301,
        );
        expect(nextSpy).not.toHaveBeenCalled();
      },
    );

    it.each(redirectedCases)(
      'redirects with query params',
      async redirectedCase => {
        redirectService.list.mockResolvedValue(testRedirects);

        const oldRoute = buildOldRoute(
          redirectedCase.routePattern,
          redirectedCase.oldSlugNewSlugPairsByPlaceholder,
        );
        const newRoute = buildNewRoute(
          redirectedCase.routePattern,
          redirectedCase.oldSlugNewSlugPairsByPlaceholder,
        );

        await runMiddleware(
          redirectPages,
          `https://my-env/${oldRoute}?search=something`,
        );

        expect(redirectSpy).toHaveBeenCalledTimes(1);
        expect(redirectSpy).toHaveBeenCalledWith(
          expect.objectContaining({
            href: `https://my-env/${newRoute}?search=something`,
          }),
          301,
        );
        expect(nextSpy).not.toHaveBeenCalled();
      },
    );

    it.each(redirectedCases)(
      'redirects with uppercase characters',
      async redirectedCase => {
        redirectService.list.mockResolvedValue(testRedirects);

        const oldRoute = buildOldRoute(
          redirectedCase.routePattern,
          redirectedCase.oldSlugNewSlugPairsByPlaceholder,
        ).toUpperCase();
        const newRoute = buildNewRoute(
          redirectedCase.routePattern,
          redirectedCase.oldSlugNewSlugPairsByPlaceholder,
        );

        await runMiddleware(redirectPages, `https://my-env/${oldRoute}`);

        expect(redirectSpy).toHaveBeenCalledTimes(1);
        expect(redirectSpy).toHaveBeenCalledWith(
          expect.objectContaining({
            href: `https://my-env/${newRoute}`,
          }),
          301,
        );
        expect(nextSpy).not.toHaveBeenCalled();
      },
    );
  });

  describe('Does Not Redirect', () => {
    it.each(nonRedirectedCases)(
      'does not redirect when the slug(s) for the requested page have no redirect(s) defined for the redirect type(s)',
      async nonRedirectedCase => {
        redirectService.list.mockResolvedValue(testRedirects);

        const route = buildRoute(
          nonRedirectedCase.routePattern,
          nonRedirectedCase.oldSlugsByPlaceholder,
        );

        await runMiddleware(redirectPages, `https://my-env/${route}`);

        expect(redirectSpy).not.toHaveBeenCalled();
        expect(nextSpy).toHaveBeenCalledTimes(1);
      },
    );

    it.each(childRouteCases)(
      'does not redirect the request for child-routes not explicitly accounted for, when the slug has changed',
      async childRouteCase => {
        redirectService.list.mockResolvedValue(testRedirects);

        const route = buildRoute(
          childRouteCase.routePattern,
          childRouteCase.oldSlugsByPlaceholder,
        );

        await runMiddleware(redirectPages, `https://my-env/${route}`);

        expect(redirectSpy).not.toHaveBeenCalled();
        expect(nextSpy).toHaveBeenCalledTimes(1);
      },
    );

    it.each(nonRedirectedCases)(
      'does not redirect if the `fromSlug` only partially matches a slug with a redirect defined',
      async nonRedirectedCase => {
        redirectService.list.mockResolvedValue(testRedirects);

        const partiallyMatchedSlugSubstringsByPlaceholder = Object.fromEntries(
          Object.entries(nonRedirectedCase.oldSlugsByPlaceholder).map(
            ([k, v]) => [k, v.substring(0, v.length - 1)],
          ),
        );

        const routeWithRedirectSlugSubstring = buildRoute(
          nonRedirectedCase.routePattern,
          partiallyMatchedSlugSubstringsByPlaceholder,
        );

        await runMiddleware(
          redirectPages,
          `https://my-env/${routeWithRedirectSlugSubstring}`,
        );

        expect(redirectSpy).not.toHaveBeenCalled();
        expect(nextSpy).toHaveBeenCalledTimes(1);

        const partiallyMatchedSlugsWithSuffixByPlaceholder = Object.fromEntries(
          Object.entries(nonRedirectedCase.oldSlugsByPlaceholder).map(
            ([k, v]) => [k, `${v}-extra-suffix`],
          ),
        );

        const routeWithRedirectSlugWithSuffix = buildRoute(
          nonRedirectedCase.routePattern,
          partiallyMatchedSlugsWithSuffixByPlaceholder,
        );

        await runMiddleware(
          redirectPages,
          `https://my-env/${routeWithRedirectSlugWithSuffix}`,
        );

        expect(redirectSpy).not.toHaveBeenCalled();
        expect(nextSpy).toHaveBeenCalledTimes(2);
      },
    );
  });
});
