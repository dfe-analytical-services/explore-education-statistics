import formatContentLinkUrl from '@common/utils/url/formatContentLinkUrl';

describe('formatContentLinkUrl', () => {
  test('converts EES links to lowercase', () => {
    expect(
      formatContentLinkUrl(
        'https://explore-education-statistics.service.gov.uk/find-statistics/Pupil-Attendance-In-Schools',
      ),
    ).toEqual(
      'https://explore-education-statistics.service.gov.uk/find-statistics/pupil-attendance-in-schools',
    );
  });

  test('does not convert query params on EES links to lowercase', () => {
    expect(
      formatContentLinkUrl(
        'https://explore-education-statistics.service.gov.uk/find-statistics?testParam=Something',
      ),
    ).toEqual(
      'https://explore-education-statistics.service.gov.uk/find-statistics?testParam=Something',
    );
  });

  test('does not convert anchors on EES links to lowercase', () => {
    expect(
      formatContentLinkUrl(
        'https://explore-education-statistics.service.gov.uk/find-statistics#Something',
      ),
    ).toEqual(
      'https://explore-education-statistics.service.gov.uk/find-statistics#Something',
    );
  });

  test('trims EES links', () => {
    expect(
      formatContentLinkUrl(
        '  https://explore-education-statistics.service.gov.uk/find-statistics  ',
      ),
    ).toEqual(
      'https://explore-education-statistics.service.gov.uk/find-statistics',
    );
  });

  test('encodes EES links', () => {
    expect(
      formatContentLinkUrl(
        'https://explore-education-statistics.service.gov.uk/find statistics',
      ),
    ).toEqual(
      'https://explore-education-statistics.service.gov.uk/find%20statistics',
    );
  });

  test('does not convert external links to lowercase', () => {
    expect(formatContentLinkUrl('https://gov.uk/Something')).toEqual(
      'https://gov.uk/Something',
    );
  });

  test('trims external links', () => {
    expect(formatContentLinkUrl('  https://gov.uk/Something  ')).toEqual(
      'https://gov.uk/Something',
    );
  });

  test('encodes external links', () => {
    expect(formatContentLinkUrl('https://gov.uk/Some thing')).toEqual(
      'https://gov.uk/Some%20thing',
    );
  });

  test('does not format anchor links', () => {
    expect(formatContentLinkUrl('#Something')).toEqual('#Something');
  });
});
