import sanitiseFileName from '../sanitiseFileName';

describe('sanitiseFileName', () => {
  test('removes disallowed characters from the file name', () => {
    expect(sanitiseFileName(`file<>:"/\\|?.*name`)).toBe('file.name');
    expect(sanitiseFileName(`data-further-education-and-skills.meta.csv`)).toBe(
      'data-further-education-and-skills.meta.csv',
    );
  });

  test('handles extensions', () => {
    expect(sanitiseFileName(`file<>:"/\\|?.*name`, 'png')).toBe(
      'file.name.png',
    );
    expect(sanitiseFileName(`file.png`, 'png')).toBe('file.png');
    expect(sanitiseFileName(`file.png`, 'csv')).toBe('file.png.csv');
    expect(sanitiseFileName(`file.meta.png`, 'csv')).toBe('file.meta.png.csv');
    expect(sanitiseFileName(`File.Meta.CSV`, 'csv')).toBe('File.Meta.csv');
  });

  test('removes backslashes from the file name', () => {
    expect(sanitiseFileName('folder\\file.png', 'png')).toBe('folderfile.png');
  });

  test('trims the file name', () => {
    expect(sanitiseFileName(` file<>:"/\\|?*.name `)).toBe('file.name');
  });
});
