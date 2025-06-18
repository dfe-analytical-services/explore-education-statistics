import sanitiseFileName from '../sanitiseFileName';

describe('sanitiseFileName', () => {
  test('removes disallowed characters from the file name', () => {
    // eslint-disable-next-line no-useless-escape
    expect(sanitiseFileName(`file<>:"/\|?.*name`)).toBe('file.name');
    expect(sanitiseFileName(`data-further-education-and-skills.meta.csv`)).toBe(
      'data-further-education-and-skills.meta.csv',
    );
  });

  test('handles extensions', () => {
    // eslint-disable-next-line no-useless-escape
    expect(sanitiseFileName(`file<>:"/\|?.*name`, 'png')).toBe('file.name.png');
    expect(sanitiseFileName(`file.png`, 'png')).toBe('file.png');
    expect(sanitiseFileName(`file.png`, 'csv')).toBe('file.png.csv');
    expect(sanitiseFileName(`file.meta.png`, 'csv')).toBe('file.meta.png.csv');
  });

  test('trims the file name', () => {
    // eslint-disable-next-line no-useless-escape
    expect(sanitiseFileName(` file<>:"/\|?*.name `)).toBe('file.name');
  });
});
