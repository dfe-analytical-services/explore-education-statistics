import getReferenceLineLabelPosition from '@common/modules/charts/components/utils/getReferenceLineLabelPosition';

describe('getReferenceLineLabelPosition', () => {
  test('returns the default position for an X axis line when otherAxisPosition is not set', () => {
    const result = getReferenceLineLabelPosition({
      axis: 'x',
      axisType: 'major',
      otherAxisDomain: [0, 50000],
      viewBox: {
        x: 205,
        y: 20,
        width: 0,
        height: 230,
      },
    });

    expect(result).toEqual({
      xPosition: 205,
      yPosition: 135,
    });
  });

  test('returns the default position for an Y axis line when otherAxisPosition is not set', () => {
    const result = getReferenceLineLabelPosition({
      axis: 'y',
      axisType: 'major',
      otherAxisDomain: [0, 50000],
      viewBox: {
        x: 80,
        y: 218,
        width: 880,
        height: 0,
      },
    });

    expect(result).toEqual({
      xPosition: 520,
      yPosition: 218,
    });
  });

  test('returns the correct positions for major X axis lines with an otherAxisPosition', () => {
    const result = getReferenceLineLabelPosition({
      axis: 'x',
      axisType: 'major',
      otherAxisDomain: [0, 50000],
      otherAxisPosition: 40000,
      viewBox: {
        x: 205,
        y: 20,
        width: 0,
        height: 230,
      },
    });

    expect(result).toEqual({
      xPosition: 205,
      yPosition: 66,
    });
  });

  test('returns the correct positions for minor X axis lines with an otherAxisPosition', () => {
    const result = getReferenceLineLabelPosition({
      axis: 'x',
      axisType: 'minor',
      otherAxisDomain: [0, 3],
      otherAxisPosition: 20,
      viewBox: {
        x: 424,
        y: 0,
        width: 0,
        height: 250,
      },
    });

    expect(result).toEqual({
      xPosition: 424,
      yPosition: 200,
    });
  });

  test('returns the correct positions for major Y axis lines with an otherAxisPosition', () => {
    const result = getReferenceLineLabelPosition({
      axis: 'y',
      axisType: 'major',
      otherAxisDomain: [0, 50000],
      otherAxisPosition: 20000,
      viewBox: {
        x: 80,
        y: 218,
        width: 880,
        height: 0,
      },
    });

    expect(result).toEqual({
      xPosition: 432,
      yPosition: 218,
    });
  });

  test('returns the correct positions for minor Y axis lines with an otherAxisPosition', () => {
    const result = getReferenceLineLabelPosition({
      axis: 'y',
      axisType: 'minor',
      otherAxisDomain: [0, 3],
      otherAxisPosition: 20,
      viewBox: {
        x: 80,
        y: 158,
        width: 880,
        height: 0,
      },
    });

    expect(result).toEqual({
      xPosition: 256,
      yPosition: 158,
    });
  });
});
