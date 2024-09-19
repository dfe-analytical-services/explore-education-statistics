import redirectPages from '@frontend/middleware/pages/redirectPages';
import _redirectService, {
  Redirects,
} from '@frontend/services/redirectService';
import { NextResponse } from 'next/server';
import middlewareTestRun from './util/middlewareTetsRun';

jest.mock('@frontend/services/redirectService');
const redirectService = _redirectService as jest.Mocked<
  typeof _redirectService
>;

describe('redirectPages', () => {
  const redirectSpy = jest.spyOn(NextResponse, 'redirect');
  const nextSpy = jest.spyOn(NextResponse, 'next');

  const testRedirects: Redirects = {
    methodologies: [
      { fromSlug: 'original-slug-1', toSlug: 'updated-slug-1' },
      { fromSlug: 'original-slug-2', toSlug: 'updated-slug-2' },
    ],
    publications: [
      { fromSlug: 'original-slug-3', toSlug: 'updated-slug-3' },
      { fromSlug: 'original-slug-4', toSlug: 'updated-slug-4' },
    ],
  };

  test('does not re-request the list of redirects once it has been fetched', async () => {
    redirectService.list.mockResolvedValue(testRedirects);

    await middlewareTestRun(
      redirectPages,
      'https://my-env/methodology/original-slug',
    );

    expect(redirectService.list).toHaveBeenCalledTimes(1);

    await middlewareTestRun(
      redirectPages,
      'https://my-env/methodology/another-slug',
    );

    expect(redirectService.list).toHaveBeenCalledTimes(1);
  });

  test('does not check for redirects for non release or methodology pages', async () => {
    await middlewareTestRun(redirectPages, 'https://my-env/find-statistics');

    expect(redirectService.list).not.toHaveBeenCalled();
    expect(redirectSpy).not.toHaveBeenCalled();
    expect(nextSpy).toHaveBeenCalledTimes(1);

    await middlewareTestRun(redirectPages, 'https://my-env/methodology');

    expect(redirectService.list).not.toHaveBeenCalled();
    expect(redirectSpy).not.toHaveBeenCalled();
    expect(nextSpy).toHaveBeenCalledTimes(2);

    await middlewareTestRun(
      redirectPages,
      'https://my-env/data-tables/something',
    );

    expect(redirectService.list).not.toHaveBeenCalled();
    expect(redirectSpy).not.toHaveBeenCalled();
    expect(nextSpy).toHaveBeenCalledTimes(3);
  });

  test('redirects urls with uppercase characters to lowercase', async () => {
    redirectService.list.mockResolvedValue(testRedirects);
    await middlewareTestRun(redirectPages, 'https://my-env/Find-Statistics');

    expect(redirectSpy).toHaveBeenCalledTimes(1);
    expect(redirectSpy).toHaveBeenCalledWith(
      expect.objectContaining({
        href: 'https://my-env/find-statistics',
      }),
      301,
    );
    expect(nextSpy).not.toHaveBeenCalled();

    await middlewareTestRun(
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

  describe('redirect methodology pages', () => {
    test('redirects the request when the slug for the requested page has changed', async () => {
      redirectService.list.mockResolvedValue(testRedirects);

      await middlewareTestRun(
        redirectPages,
        'https://my-env/methodology/original-slug-1',
      );

      expect(redirectSpy).toHaveBeenCalledTimes(1);
      expect(redirectSpy).toHaveBeenCalledWith(
        expect.objectContaining({
          href: 'https://my-env/methodology/updated-slug-1',
        }),
        301,
      );
      expect(nextSpy).not.toHaveBeenCalled();
    });

    test('does not redirect when the slug for the requested page has not changed', async () => {
      redirectService.list.mockResolvedValue(testRedirects);

      await middlewareTestRun(
        redirectPages,
        'https://my-env/methodology/my-methodology',
      );

      expect(redirectSpy).not.toHaveBeenCalled();
      expect(nextSpy).toHaveBeenCalledTimes(1);
    });

    test('does not redirect if the `fromSlug` only partially matches', async () => {
      redirectService.list.mockResolvedValue(testRedirects);

      await middlewareTestRun(
        redirectPages,
        'https://my-env/methodology/original',
      );

      expect(redirectSpy).not.toHaveBeenCalled();
      expect(nextSpy).toHaveBeenCalledTimes(1);

      await middlewareTestRun(
        redirectPages,
        'https://my-env/methodology/original-slug-and-something',
      );

      expect(redirectSpy).not.toHaveBeenCalled();
      expect(nextSpy).toHaveBeenCalledTimes(2);
    });

    test('redirects child pages', async () => {
      redirectService.list.mockResolvedValue(testRedirects);

      await middlewareTestRun(
        redirectPages,
        'https://my-env/methodology/original-slug-1/child-page',
      );

      expect(redirectSpy).toHaveBeenCalledTimes(1);
      expect(redirectSpy).toHaveBeenCalledWith(
        expect.objectContaining({
          href: 'https://my-env/methodology/updated-slug-1/child-page',
        }),
        301,
      );
      expect(nextSpy).not.toHaveBeenCalled();
    });

    test('redirects with query params', async () => {
      redirectService.list.mockResolvedValue(testRedirects);

      await middlewareTestRun(
        redirectPages,
        'https://my-env/methodology/original-slug-1?search=something',
      );

      expect(redirectSpy).toHaveBeenCalledTimes(1);
      expect(redirectSpy).toHaveBeenCalledWith(
        expect.objectContaining({
          href: 'https://my-env/methodology/updated-slug-1?search=something',
        }),
        301,
      );
      expect(nextSpy).not.toHaveBeenCalled();
    });

    test('does not redirect when the slug matches a `fromSlug` in a different page type', async () => {
      redirectService.list.mockResolvedValue(testRedirects);

      await middlewareTestRun(
        redirectPages,
        'https://my-env/methodology/original-slug-4',
      );

      expect(redirectSpy).not.toHaveBeenCalled();
      expect(nextSpy).toHaveBeenCalledTimes(1);
    });

    test('redirects with uppercase characters', async () => {
      redirectService.list.mockResolvedValue(testRedirects);

      await middlewareTestRun(
        redirectPages,
        'https://my-env/Methodology/original-SLUG-1',
      );

      expect(redirectSpy).toHaveBeenCalledTimes(1);
      expect(redirectSpy).toHaveBeenCalledWith(
        expect.objectContaining({
          href: 'https://my-env/methodology/updated-slug-1',
        }),
        301,
      );
      expect(nextSpy).not.toHaveBeenCalled();
    });
  });

  describe('redirect publication pages', () => {
    test('redirects the request when the slug for the requested page has changed', async () => {
      redirectService.list.mockResolvedValue(testRedirects);

      await middlewareTestRun(
        redirectPages,
        'https://my-env/find-statistics/original-slug-3',
      );

      expect(redirectSpy).toHaveBeenCalledTimes(1);
      expect(redirectSpy).toHaveBeenCalledWith(
        expect.objectContaining({
          href: 'https://my-env/find-statistics/updated-slug-3',
        }),
        301,
      );
      expect(nextSpy).not.toHaveBeenCalled();
    });

    test('does not redirect when the slug for the requested page has not changed', async () => {
      redirectService.list.mockResolvedValue(testRedirects);

      await middlewareTestRun(
        redirectPages,
        'https://my-env/find-statistics/my-publication',
      );

      expect(redirectSpy).not.toHaveBeenCalled();
      expect(nextSpy).toHaveBeenCalledTimes(1);
    });

    test('does not redirect if the `fromSlug` only partially matches', async () => {
      redirectService.list.mockResolvedValue(testRedirects);

      await middlewareTestRun(
        redirectPages,
        'https://my-env/find-statistics/original',
      );

      expect(redirectSpy).not.toHaveBeenCalled();
      expect(nextSpy).toHaveBeenCalledTimes(1);

      await middlewareTestRun(
        redirectPages,
        'https://my-env/find-statistics/original-slug-and-something',
      );

      expect(redirectSpy).not.toHaveBeenCalled();
      expect(nextSpy).toHaveBeenCalledTimes(2);
    });

    test('redirects child pages', async () => {
      redirectService.list.mockResolvedValue(testRedirects);

      await middlewareTestRun(
        redirectPages,
        'https://my-env/find-statistics/original-slug-3/child-page',
      );

      expect(redirectSpy).toHaveBeenCalledTimes(1);
      expect(redirectSpy).toHaveBeenCalledWith(
        expect.objectContaining({
          href: 'https://my-env/find-statistics/updated-slug-3/child-page',
        }),
        301,
      );
      expect(nextSpy).not.toHaveBeenCalled();
    });

    test('redirects with query params', async () => {
      redirectService.list.mockResolvedValue(testRedirects);

      await middlewareTestRun(
        redirectPages,
        'https://my-env/find-statistics/original-slug-3?search=something',
      );

      expect(redirectSpy).toHaveBeenCalledTimes(1);

      expect(redirectSpy).toHaveBeenCalledWith(
        expect.objectContaining({
          href: 'https://my-env/find-statistics/updated-slug-3?search=something',
        }),
        301,
      );
      expect(nextSpy).not.toHaveBeenCalled();
    });

    test('does not redirect when the slug matches a `fromSlug` in a different page type', async () => {
      redirectService.list.mockResolvedValue(testRedirects);

      await middlewareTestRun(
        redirectPages,
        'https://my-env/find-statistics/original-slug-1',
      );

      expect(redirectSpy).not.toHaveBeenCalled();
      expect(nextSpy).toHaveBeenCalledTimes(1);
    });

    test('redirects with uppercase characters', async () => {
      redirectService.list.mockResolvedValue(testRedirects);

      await middlewareTestRun(
        redirectPages,
        'https://my-env/find-Statistics/original-SLUG-3',
      );

      expect(redirectSpy).toHaveBeenCalledTimes(1);
      expect(redirectSpy).toHaveBeenCalledWith(
        expect.objectContaining({
          href: 'https://my-env/find-statistics/updated-slug-3',
        }),
        301,
      );
      expect(nextSpy).not.toHaveBeenCalled();
    });
  });
});
