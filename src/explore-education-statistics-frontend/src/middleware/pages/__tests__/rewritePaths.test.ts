import { NextResponse } from 'next/server';
import runMiddleware from './util/runMiddleware';
import rewritePaths from '../rewritePaths';

describe('rewritePaths', () => {
  const rewriteSpy = jest.spyOn(NextResponse, 'rewrite');
  const nextSpy = jest.spyOn(NextResponse, 'next');

  test('rewrites data-catalogue csv downloads to file download endpoint', async () => {
    const testId = 'test-id-1';
    process.env.CONTENT_API_BASE_URL = 'https://my-content-env';

    await runMiddleware(
      rewritePaths,
      `https://my-env/data-catalogue/data-set/${testId}/csv`,
    );

    expect(rewriteSpy).toHaveBeenCalledTimes(1);
    expect(rewriteSpy).toHaveBeenCalledWith(
      `${process.env.CONTENT_API_BASE_URL}/data-set-files/${testId}/download`,
    );
    expect(nextSpy).not.toHaveBeenCalled();
  });
});
