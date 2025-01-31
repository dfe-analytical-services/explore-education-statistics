import slugFromTitle from '../slugFromTitle';

describe('slugFromTitle', () => {
  test('converts calendar year', () => {
    expect(slugFromTitle('calendar year 2019')).toEqual('calendar-year-2019');
  });

  test('converts non-calendar year', () => {
    expect(slugFromTitle('tax year 2019/20')).toEqual('tax-year-2019-20');
  });

  test('no changes', () => {
    expect(slugFromTitle('title')).toEqual('title');
  });

  test('lowercase characters', () => {
    expect(slugFromTitle('TITLE')).toEqual('title');
  });

  test('converts a sentence with spaces', () => {
    expect(slugFromTitle('A sentence with spaces')).toEqual(
      'a-sentence-with-spaces',
    );
  });

  test('converts a sentence with dashes and spaces', () => {
    expect(
      slugFromTitle('A - sentence -  with - - dashes  -  and -- spaces'),
    ).toEqual('a-sentence-with-dashes-and-spaces');
  });

  test('converts a sentence with non alphanumeric characters', () => {
    expect(
      slugFromTitle("A sentence with !@£('\\) non alpha numeric characters"),
    ).toEqual('a-sentence-with-non-alpha-numeric-characters');
  });

  test('converts a sentence with non alphanumeric characters at the end', () => {
    expect(
      slugFromTitle(
        "A sentence with non alpha numeric characters at the end !@£('\\)",
      ),
    ).toEqual('a-sentence-with-non-alpha-numeric-characters-at-the-end');
  });

  test('converts a sentence with big spaces', () => {
    expect(slugFromTitle('A sentence with     big      spaces      ')).toEqual(
      'a-sentence-with-big-spaces',
    );
  });

  test('converts a sentence with numbers', () => {
    expect(slugFromTitle('A sentence with numbers 1 2 3 and 4')).toEqual(
      'a-sentence-with-numbers-1-2-3-and-4',
    );
  });
});
