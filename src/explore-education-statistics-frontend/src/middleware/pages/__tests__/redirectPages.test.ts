import _redirectService, {
  Redirects,
} from '@frontend/services/redirectService';
import redirectPages from '@frontend/middleware/pages/redirectPages';
import { NextResponse, NextRequest } from 'next/server';

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
    const req = new NextRequest(
      new Request('https://my-env/methodology/original-slug'),
    );
    await redirectPages(req);

    expect(redirectService.list).toHaveBeenCalledTimes(1);

    const req2 = new NextRequest(
      new Request('https://my-env/methodology/another-slug'),
    );
    await redirectPages(req2);

    expect(redirectService.list).toHaveBeenCalledTimes(1);
  });

  describe('redirect methodology pages', () => {
    test('redirects the request when the slug for the requested page has changed', async () => {
      redirectService.list.mockResolvedValue(testRedirects);
      const req = new NextRequest(
        new Request('https://my-env/methodology/original-slug-1'),
      );
      await redirectPages(req);

      expect(redirectSpy).toHaveBeenCalledTimes(1);
      expect(redirectSpy).toHaveBeenCalledWith(
        new URL('https://my-env/methodology/updated-slug-1'),
      );
      expect(nextSpy).not.toHaveBeenCalled();
    });

    test('does not redirect when the slug for the requested page has not changed', async () => {
      redirectService.list.mockResolvedValue(testRedirects);
      const req = new NextRequest(
        new Request('https://my-env/methodology/my-methodology'),
      );
      await redirectPages(req);

      expect(redirectSpy).not.toHaveBeenCalled();
      expect(nextSpy).toHaveBeenCalledTimes(1);
    });

    test('does not redirect if the `fromSlug` only partially matches', async () => {
      redirectService.list.mockResolvedValue(testRedirects);
      const req = new NextRequest(
        new Request('https://my-env/methodology/original'),
      );
      await redirectPages(req);

      expect(redirectSpy).not.toHaveBeenCalled();
      expect(nextSpy).toHaveBeenCalledTimes(1);

      const req2 = new NextRequest(
        new Request('https://my-env/methodology/original-slug-and-something'),
      );
      await redirectPages(req2);

      expect(redirectSpy).not.toHaveBeenCalled();
      expect(nextSpy).toHaveBeenCalledTimes(2);
    });

    test('redirects child pages', async () => {
      redirectService.list.mockResolvedValue(testRedirects);
      const req = new NextRequest(
        new Request('https://my-env/methodology/original-slug-1/child-page'),
      );
      await redirectPages(req);

      expect(redirectSpy).toHaveBeenCalledTimes(1);
      expect(redirectSpy).toHaveBeenCalledWith(
        new URL('https://my-env/methodology/updated-slug-1/child-page'),
      );
      expect(nextSpy).not.toHaveBeenCalled();
    });

    test('redirects with anchor links', async () => {
      redirectService.list.mockResolvedValue(testRedirects);
      const req = new NextRequest(
        new Request('https://my-env/methodology/original-slug-1#anchor-link'),
      );
      await redirectPages(req);

      expect(redirectSpy).toHaveBeenCalledTimes(1);
      expect(redirectSpy).toHaveBeenCalledWith(
        new URL('https://my-env/methodology/updated-slug-1#anchor-link'),
      );
      expect(nextSpy).not.toHaveBeenCalled();
    });

    test('redirects with query params', async () => {
      redirectService.list.mockResolvedValue(testRedirects);
      const req = new NextRequest(
        new Request(
          'https://my-env/methodology/original-slug-1?search=something',
        ),
      );
      await redirectPages(req);

      expect(redirectSpy).toHaveBeenCalledTimes(1);
      expect(redirectSpy).toHaveBeenCalledWith(
        new URL('https://my-env/methodology/updated-slug-1?search=something'),
      );
      expect(nextSpy).not.toHaveBeenCalled();
    });

    test('does not redirect when the slug matches a `fromSlug` in a different page type', async () => {
      redirectService.list.mockResolvedValue(testRedirects);
      const req = new NextRequest(
        new Request('https://my-env/methodology/original-slug-4'),
      );

      await redirectPages(req);

      expect(redirectSpy).not.toHaveBeenCalled();
      expect(nextSpy).toHaveBeenCalledTimes(1);
    });
  });

  describe('redirect publication pages', () => {
    test('redirects the request when the slug for the requested page has changed', async () => {
      redirectService.list.mockResolvedValue(testRedirects);
      const req = new NextRequest(
        new Request('https://my-env/find-statistics/original-slug-3'),
      );
      await redirectPages(req);

      expect(redirectSpy).toHaveBeenCalledTimes(1);
      expect(redirectSpy).toHaveBeenCalledWith(
        new URL('https://my-env/find-statistics/updated-slug-3'),
      );
      expect(nextSpy).not.toHaveBeenCalled();
    });

    test('does not redirect when the slug for the requested page has not changed', async () => {
      redirectService.list.mockResolvedValue(testRedirects);
      const req = new NextRequest(
        new Request('https://my-env/find-statistics/my-publication'),
      );
      await redirectPages(req);

      expect(redirectSpy).not.toHaveBeenCalled();
      expect(nextSpy).toHaveBeenCalledTimes(1);
    });

    test('does not redirect if the `fromSlug` only partially matches', async () => {
      redirectService.list.mockResolvedValue(testRedirects);
      const req = new NextRequest(
        new Request('https://my-env/find-statistics/original'),
      );
      await redirectPages(req);

      expect(redirectSpy).not.toHaveBeenCalled();
      expect(nextSpy).toHaveBeenCalledTimes(1);

      const req2 = new NextRequest(
        new Request(
          'https://my-env/find-statistics/original-slug-and-something',
        ),
      );
      await redirectPages(req2);

      expect(redirectSpy).not.toHaveBeenCalled();
      expect(nextSpy).toHaveBeenCalledTimes(2);
    });

    test('redirects child pages', async () => {
      redirectService.list.mockResolvedValue(testRedirects);
      const req = new NextRequest(
        new Request(
          'https://my-env/find-statistics/original-slug-3/child-page',
        ),
      );
      await redirectPages(req);

      expect(redirectSpy).toHaveBeenCalledTimes(1);
      expect(redirectSpy).toHaveBeenCalledWith(
        new URL('https://my-env/find-statistics/updated-slug-3/child-page'),
      );
      expect(nextSpy).not.toHaveBeenCalled();
    });

    test('redirects with anchor links', async () => {
      redirectService.list.mockResolvedValue(testRedirects);
      const req = new NextRequest(
        new Request(
          'https://my-env/find-statistics/original-slug-3#anchor-link',
        ),
      );
      await redirectPages(req);

      expect(redirectSpy).toHaveBeenCalledTimes(1);
      expect(redirectSpy).toHaveBeenCalledWith(
        new URL('https://my-env/find-statistics/updated-slug-3#anchor-link'),
      );
      expect(nextSpy).not.toHaveBeenCalled();
    });

    test('redirects with query params', async () => {
      redirectService.list.mockResolvedValue(testRedirects);
      const req = new NextRequest(
        new Request(
          'https://my-env/find-statistics/original-slug-3?search=something',
        ),
      );
      await redirectPages(req);

      expect(redirectSpy).toHaveBeenCalledTimes(1);
      expect(redirectSpy).toHaveBeenCalledWith(
        new URL(
          'https://my-env/find-statistics/updated-slug-3?search=something',
        ),
      );
      expect(nextSpy).not.toHaveBeenCalled();
    });

    test('does not redirect when the slug matches a `fromSlug` in a different page type', async () => {
      redirectService.list.mockResolvedValue(testRedirects);
      const req = new NextRequest(
        new Request('https://my-env/find-statistics/original-slug-1'),
      );

      await redirectPages(req);

      expect(redirectSpy).not.toHaveBeenCalled();
      expect(nextSpy).toHaveBeenCalledTimes(1);
    });
  });
});
