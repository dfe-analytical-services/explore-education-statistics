import getPropsForExternality from '../getPropsForExternality';

describe('getPropsForExternality', () => {
  test('internal links', () => {
    const anchorTagProps = getPropsForExternality(
      'https://EXPLORE-education-statistics.service.gov.uk/find-statistics/Pupil-Attendance-In-Schools',
      'Click this link!',
    );

    expect(anchorTagProps).toEqual({
      url: 'https://explore-education-statistics.service.gov.uk/find-statistics/pupil-attendance-in-schools',
      target: undefined,
      text: 'Click this link!',
      externality: 'internal',
      rel: undefined,
    });
  });

  test('admin links', () => {
    const { url, target, text, externality, rel } = getPropsForExternality(
      'https://aDmIn.EXPLORE-education-statistics.service.gov.uk/find-statistics/Pupil-Attendance-In-Schools',
      'Click this admin link!',
    );

    expect(url).toBe(
      'https://admin.explore-education-statistics.service.gov.uk/find-statistics/pupil-attendance-in-schools',
    );
    expect(target).toBe('_blank');
    expect(text).toBe('Click this admin link! (opens in a new tab)');
    expect(externality).toBe('external-admin');
    expect(rel).toContain('nofollow');
    expect(rel).toContain('noopener');
    expect(rel).toContain('noreferrer');
  });

  test('external links', () => {
    const { url, target, text, externality, rel } = getPropsForExternality(
      'https://stackoverflow.com/Some-Upper-PATH',
      'Click this external link!',
    );

    expect(url).toBe('https://stackoverflow.com/Some-Upper-PATH');
    expect(target).toBe('_blank');
    expect(text).toBe('Click this external link! (opens in a new tab)');
    expect(externality).toBe('external');
    expect(rel).toContain('nofollow');
    expect(rel).toContain('noopener');
    expect(rel).toContain('noreferrer');
    expect(rel).toContain('external');
  });
});
