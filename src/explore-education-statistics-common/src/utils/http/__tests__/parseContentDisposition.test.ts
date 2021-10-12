import parseContentDisposition, {
  ParsedContentDisposition,
} from '@common/utils/http/parseContentDisposition';

describe('parseContentDisposition', () => {
  test('parses `inline` type', () => {
    const parsed = parseContentDisposition('inline');

    expect(parsed).toEqual<ParsedContentDisposition>({
      type: 'inline',
    });
  });

  test('parses `attachment` with quoted `filename`', () => {
    const parsed = parseContentDisposition('attachment; filename="test.csv"');

    expect(parsed).toEqual<ParsedContentDisposition>({
      type: 'attachment',
      filename: 'test.csv',
    });
  });

  test('parses `attachment` type with quoted `filename`', () => {
    const parsed = parseContentDisposition('attachment; filename="test.csv"');

    expect(parsed).toEqual<ParsedContentDisposition>({
      type: 'attachment',
      filename: 'test.csv',
    });
  });

  test('parses `attachment` type with quoted `filename` with internal quotes', () => {
    const parsed = parseContentDisposition(
      'attachment; filename="\\"test\\".csv"',
    );

    expect(parsed).toEqual<ParsedContentDisposition>({
      type: 'attachment',
      filename: '"test".csv',
    });
  });

  test('parses `attachment` type with quoted `filename` with internal spaces', () => {
    const parsed = parseContentDisposition(
      'attachment; filename="Test file.csv"',
    );

    expect(parsed).toEqual<ParsedContentDisposition>({
      type: 'attachment',
      filename: 'Test file.csv',
    });
  });

  test('parses parameters with any amount of whitespace separating them', () => {
    const parsed = parseContentDisposition(
      'attachment;  \n  filename="test.csv"',
    );

    expect(parsed).toEqual<ParsedContentDisposition>({
      type: 'attachment',
      filename: 'test.csv',
    });
  });

  test('throws if empty', () => {
    expect(() => parseContentDisposition('')).toThrow(
      'Cannot parse empty header',
    );
  });

  test('throws if no type', () => {
    expect(() => parseContentDisposition('filename="test.csv"')).toThrow(
      'Must specify type in first parameter',
    );
  });

  test('throws if type is not in the first parameter', () => {
    expect(() =>
      parseContentDisposition('filename="test.csv"; attachment'),
    ).toThrow('Must specify type in first parameter');
  });

  test('throws if no `filename` for `attachment` type', () => {
    expect(() => parseContentDisposition('attachment')).toThrow(
      'Must specify filename in second parameter',
    );
  });

  test('throws if invalid `filename` parameter', () => {
    expect(() => parseContentDisposition('attachment; filename')).toThrow(
      'Invalid filename parameter',
    );
  });

  test('throws if `filename` is not double quoted', () => {
    expect(() =>
      parseContentDisposition('attachment; filename=test.csv'),
    ).toThrow('Cannot parse unquoted filename parameter');
  });

  test('throws if `filename` empty for `attachment` type', () => {
    expect(() => parseContentDisposition('attachment; filename=""')).toThrow(
      'Filename parameter cannot be empty',
    );

    expect(() => parseContentDisposition('attachment; filename=')).toThrow(
      'Filename parameter cannot be empty',
    );
  });
});
