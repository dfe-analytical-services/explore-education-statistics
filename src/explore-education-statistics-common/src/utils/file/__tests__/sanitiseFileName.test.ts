import sanitiseFileName from '../sanitiseFileName';

describe('sanitiseFileName', () => {
  test('removes disallowed characters from the file name', () => {
    // eslint-disable-next-line no-useless-escape
    expect(sanitiseFileName(`file<>:"/\|?.*name`)).toBe('filename');
  });

  test('trims the file name', () => {
    // eslint-disable-next-line no-useless-escape
    expect(sanitiseFileName(` file<>:"/\|?*.name `)).toBe('filename');
  });
});
