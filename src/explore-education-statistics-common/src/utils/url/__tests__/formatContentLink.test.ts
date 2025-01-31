import formatContentLink from '@common/utils/url/formatContentLink';

describe('formatContentLink', () => {
  test('converts EES links to lowercase', () => {
    expect(
      formatContentLink(
        'https://explore-education-statistics.service.gov.uk/find-statistics/Pupil-Attendance-In-Schools',
      ),
    ).toEqual(
      'https://explore-education-statistics.service.gov.uk/find-statistics/pupil-attendance-in-schools',
    );
  });

  test('does not convert query params on EES links to lowercase', () => {
    expect(
      formatContentLink(
        'https://explore-education-statistics.service.gov.uk/find-statistics?testParam=Something',
      ),
    ).toEqual(
      'https://explore-education-statistics.service.gov.uk/find-statistics?testParam=Something',
    );
  });

  test('does not convert anchors on EES links to lowercase', () => {
    expect(
      formatContentLink(
        'https://explore-education-statistics.service.gov.uk/find-statistics#Something',
      ),
    ).toEqual(
      'https://explore-education-statistics.service.gov.uk/find-statistics#Something',
    );
  });

  test('trims EES links', () => {
    expect(
      formatContentLink(
        '  https://explore-education-statistics.service.gov.uk/find-statistics  ',
      ),
    ).toEqual(
      'https://explore-education-statistics.service.gov.uk/find-statistics',
    );
  });

  test('encodes EES links', () => {
    expect(
      formatContentLink(
        'https://explore-education-statistics.service.gov.uk/find statistics',
      ),
    ).toEqual(
      'https://explore-education-statistics.service.gov.uk/find%20statistics',
    );
  });

  test('does not convert external links to lowercase', () => {
    expect(formatContentLink('https://gov.uk/Something')).toEqual(
      'https://gov.uk/Something',
    );
  });

  test('trims external links', () => {
    expect(formatContentLink('  https://gov.uk/Something  ')).toEqual(
      'https://gov.uk/Something',
    );
  });

  test('encodes external links', () => {
    expect(formatContentLink('https://gov.uk/Some thing')).toEqual(
      'https://gov.uk/Some%20thing',
    );
  });

  test('does not format anchor links', () => {
    expect(formatContentLink('#Something')).toEqual('#Something');
  });
});
