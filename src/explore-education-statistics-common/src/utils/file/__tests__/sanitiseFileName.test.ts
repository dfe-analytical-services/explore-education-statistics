import sanitiseFileName from '../sanitiseFileName';

describe('sanitiseFileName', () => {
  test('removes disallowed characters from the file name', () => {
    // eslint-disable-next-line no-useless-escape
    expect(sanitiseFileName(`file<>:"/\|?.*name`)).toBe('file.name');
    expect(sanitiseFileName(`data-further-education-and-skills.meta.csv`)).toBe(
      'data-further-education-and-skills.meta.csv',
    );
  });

  test('trims the file name', () => {
    // eslint-disable-next-line no-useless-escape
    expect(sanitiseFileName(` file<>:"/\|?*.name `)).toBe('file.name');
  });
});
