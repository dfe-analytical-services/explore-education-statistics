import { NextResponse } from 'next/server';
import rewriteDataCatalogueDownload from '../rewriteDataCatalogue';
import middlewareTestRun from './util/middlewareTetsRun';

describe('rewriteDataCatalogue', () => {
  const redirectSpy = jest.spyOn(NextResponse, 'redirect');
  const nextSpy = jest.spyOn(NextResponse, 'next');

  test('redirects data-catalogue csv downloads to file download endpoint', async () => {
    const testId = 'test-id-1';
    process.env.CONTENT_API_BASE_URL = 'https://my-content-env';

    await middlewareTestRun(
      rewriteDataCatalogueDownload,
      `https://my-env/data-catalogue/data-set/${testId}/csv`,
    );

    expect(redirectSpy).toHaveBeenCalledTimes(1);
    expect(redirectSpy).toHaveBeenCalledWith(
      expect.objectContaining({
        href: `${process.env.CONTENT_API_BASE_URL}/data-set-files/${testId}/download`,
      }),
      301,
    );
    expect(nextSpy).not.toHaveBeenCalled();
  });
});
