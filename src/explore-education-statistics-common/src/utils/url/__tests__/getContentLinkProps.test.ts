import getContentLinkProps, {
  addNewTabWarning,
} from '@common/utils/url/getContentLinkProps';
import * as hostUrl from '@common/utils/url/hostUrl';

jest.mock('@common/utils/url/hostUrl');
jest
  .spyOn(hostUrl, 'getHostUrl')
  .mockReturnValue(
    new URL('https://explore-education-statistics.servce.gov.uk/'),
  );

describe('getContentLinkProps', () => {
  test('public links', () => {
    const anchorTagProps = getContentLinkProps({
      url: 'https://EXPLORE-education-statistics.service.gov.uk/find-statistics/Pupil-Attendance-In-Schools',
      text: 'Click this link!',
    });

    expect(anchorTagProps).toEqual({
      url: 'https://explore-education-statistics.service.gov.uk/find-statistics/pupil-attendance-in-schools',
      text: 'Click this link!',
      origin: 'public',
    });
  });

  test('admin links', () => {
    const { url, target, text, origin, rel } = getContentLinkProps({
      url: 'https://aDmIn.EXPLORE-education-statistics.service.gov.uk/find-statistics/Pupil-Attendance-In-Schools',
      text: 'Click this admin link!',
    });

    expect(url).toBe(
      'https://admin.explore-education-statistics.service.gov.uk/find-statistics/pupil-attendance-in-schools',
    );
    expect(target).toBe('_blank');
    expect(text).toBe('Click this admin link! (opens in a new tab)');
    expect(origin).toBe('admin');
    expect(rel).toContain('nofollow');
    expect(rel).toContain('noopener');
    expect(rel).toContain('noreferrer');
  });

  test('external links', () => {
    const { url, target, text, origin, rel } = getContentLinkProps({
      url: 'https://stackoverflow.com/Some-Upper-PATH',
      text: 'Click this external link!',
    });

    expect(url).toBe('https://stackoverflow.com/Some-Upper-PATH');
    expect(target).toBe('_blank');
    expect(text).toBe('Click this external link! (opens in a new tab)');
    expect(origin).toBe('external');
    expect(rel).toContain('nofollow');
    expect(rel).toContain('noopener');
    expect(rel).toContain('noreferrer');
    expect(rel).toContain('external');
  });
});

describe('addNewTabWarning', () => {
  test.each([
    ['Click here!', 'Click here! (opens in a new tab)'],
    ['', '(opens in a new tab)'],
    ['   ', '(opens in a new tab)'],
    [' untrimmed link  ', 'untrimmed link (opens in a new tab)'],
    [
      'link with manual warning (opens in a new tab)',
      'link with manual warning (opens in a new tab)',
    ],
    [
      '  untrimmed link with manual warning (opens in a new tab)       ',
      'untrimmed link with manual warning (opens in a new tab)',
    ],
  ])('adds advisory text to links', (input: string, expectedOutput: string) =>
    expect(addNewTabWarning(input)).toBe(expectedOutput),
  );
});
