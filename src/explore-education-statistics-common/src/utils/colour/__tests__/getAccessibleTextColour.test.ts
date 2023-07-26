import getAccessibleTextColour from '@common/utils/colour/getAccessibleTextColour';

describe('getAccessibleTextColour', () => {
  test('returns the text colour if it has sufficient contrast against the background colour', () => {
    expect(
      getAccessibleTextColour({
        backgroundColour: '#6BACE6',
        textColour: '#0B0C0C',
      }),
    ).toBe('#0B0C0C');
  });

  test('returns the white if the given text colour does not have sufficient contrast against the background colour and white does', () => {
    expect(
      getAccessibleTextColour({
        backgroundColour: '#801650',
        textColour: '#0B0C0C',
      }),
    ).toBe('#FFFFFF');
  });

  test('returns the black if the given text colour does not have sufficient contrast against the background colour and white also does not', () => {
    expect(
      getAccessibleTextColour({
        backgroundColour: '#F0F3C4',
        textColour: '#F8C6B9',
      }),
    ).toBe('#000000');
  });

  test('applies the correct contrast ratio when the WCAG level is AAA', () => {
    expect(
      getAccessibleTextColour({
        backgroundColour: '#6BACE6',
        textColour: '#262B2B',
        wcagLevel: 'AAA',
      }),
    ).toBe('#FFFFFF');
  });

  test('applies the correct contrast ratio when the text size is large', () => {
    expect(
      getAccessibleTextColour({
        backgroundColour: '#28A197',
        textColour: '#FBFDFD',
        textSize: 'large',
      }),
    ).toBe('#FBFDFD');
  });

  test('applies the correct contrast ratio when the WCAG level is AAA and the text size is large', () => {
    expect(
      getAccessibleTextColour({
        backgroundColour: '#F46A25',
        textColour: '#0B0C0C',
        textSize: 'large',
        wcagLevel: 'AAA',
      }),
    ).toBe('#0B0C0C');
  });
});
