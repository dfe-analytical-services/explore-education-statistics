import { NextResponse } from 'next/server';
import runMiddleware from '../../__tests__/util/runMiddleware';
import updateRequestDestinations from '../updateRequestDestinations';

describe('updateRequestDestinations', () => {
  const redirectSpy = jest.spyOn(NextResponse, 'redirect');
  const nextSpy = jest.spyOn(NextResponse, 'next');

  test('temporary redirects data-catalogue csv downloads to file download endpoint', async () => {
    const testId = 'test-id-1';
    process.env.CONTENT_API_BASE_URL = 'https://my-content-env';

    await runMiddleware(
      updateRequestDestinations,
      `https://my-env/data-catalogue/data-set/${testId}/csv`,
    );

    expect(redirectSpy).toHaveBeenCalledTimes(1);
    expect(redirectSpy).toHaveBeenCalledWith(
      `${process.env.CONTENT_API_BASE_URL}/data-set-files/${testId}/download`,
      {
        status: 307,
      },
    );
    expect(nextSpy).not.toHaveBeenCalled();
  });
});
