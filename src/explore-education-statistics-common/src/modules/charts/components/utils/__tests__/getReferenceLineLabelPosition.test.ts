import getReferenceLineLabelPosition from '@common/modules/charts/components/utils/getReferenceLineLabelPosition';

describe('getReferenceLineLabelPosition', () => {
  test('returns the default position for an X axis line when otherAxisPosition is not set', () => {
    const result = getReferenceLineLabelPosition({
      axis: 'x',
      axisType: 'major',
      otherAxisDomainMin: 0,
      otherAxisDomainMax: 50000,
      viewBox: {
        x: 205,
        y: 20,
        width: 0,
        height: 230,
      },
    });

    expect(result).toEqual({
      x: 205,
      y: 135,
    });
  });

  test('returns the default position for a Y axis line when otherAxisPosition is not set', () => {
    const result = getReferenceLineLabelPosition({
      axis: 'y',
      axisType: 'major',
      otherAxisDomainMin: 0,
      otherAxisDomainMax: 50000,
      viewBox: {
        x: 80,
        y: 218,
        width: 880,
        height: 0,
      },
    });

    expect(result).toEqual({
      x: 520,
      y: 218,
    });
  });

  test('returns the default position for an X axis line when otherAxisPosition below the minimum', () => {
    const result = getReferenceLineLabelPosition({
      axis: 'x',
      axisType: 'minor',
      otherAxisDomainMin: 0,
      otherAxisDomainMax: 100,
      otherAxisPosition: -10,
      viewBox: {
        x: 424,
        y: 0,
        width: 0,
        height: 250,
      },
    });

    expect(result).toEqual({
      x: 424,
      y: 125,
    });
  });

  test('returns the default position for an X axis line when otherAxisPosition above the maximum', () => {
    const result = getReferenceLineLabelPosition({
      axis: 'x',
      axisType: 'minor',
      otherAxisDomainMin: 0,
      otherAxisDomainMax: 100,
      otherAxisPosition: 110,
      viewBox: {
        x: 424,
        y: 0,
        width: 0,
        height: 250,
      },
    });

    expect(result).toEqual({
      x: 424,
      y: 125,
    });
  });

  test('returns the default position for a Y axis line when otherAxisPosition below the minimum', () => {
    const result = getReferenceLineLabelPosition({
      axis: 'y',
      axisType: 'major',
      otherAxisDomainMin: 10000,
      otherAxisDomainMax: 50000,
      otherAxisPosition: 1000,
      viewBox: {
        x: 80,
        y: 218,
        width: 880,
        height: 0,
      },
    });

    expect(result).toEqual({
      x: 520,
      y: 218,
    });
  });

  test('returns the default position for a Y axis line when otherAxisPosition above the maximum', () => {
    const result = getReferenceLineLabelPosition({
      axis: 'y',
      axisType: 'major',
      otherAxisDomainMin: 10000,
      otherAxisDomainMax: 50000,
      otherAxisPosition: 51000,
      viewBox: {
        x: 80,
        y: 218,
        width: 880,
        height: 0,
      },
    });

    expect(result).toEqual({
      x: 520,
      y: 218,
    });
  });

  test('returns the correct positions for major X axis lines with an otherAxisPosition', () => {
    const result = getReferenceLineLabelPosition({
      axis: 'x',
      axisType: 'major',
      otherAxisDomainMin: 0,
      otherAxisDomainMax: 50000,
      otherAxisPosition: 40000,
      viewBox: {
        x: 205,
        y: 20,
        width: 0,
        height: 230,
      },
    });

    expect(result).toEqual({
      x: 205,
      y: 66,
    });
  });

  test('returns the default position for an X axis line when otherAxisDomainMin is undefined', () => {
    const result = getReferenceLineLabelPosition({
      axis: 'x',
      axisType: 'minor',
      otherAxisDomainMax: 100,
      otherAxisPosition: 10,
      viewBox: {
        x: 424,
        y: 0,
        width: 0,
        height: 250,
      },
    });

    expect(result).toEqual({
      x: 424,
      y: 125,
    });
  });

  test('returns the default position for an X axis line when otherAxisDomainMax is undefined', () => {
    const result = getReferenceLineLabelPosition({
      axis: 'x',
      axisType: 'minor',
      otherAxisDomainMin: 0,
      otherAxisPosition: 10,
      viewBox: {
        x: 424,
        y: 0,
        width: 0,
        height: 250,
      },
    });

    expect(result).toEqual({
      x: 424,
      y: 125,
    });
  });

  test('returns the default position for a Y axis line when otherAxisDomainMin is undefined', () => {
    const result = getReferenceLineLabelPosition({
      axis: 'y',
      axisType: 'major',
      otherAxisDomainMax: 50000,
      otherAxisPosition: 20000,
      viewBox: {
        x: 80,
        y: 218,
        width: 880,
        height: 0,
      },
    });

    expect(result).toEqual({
      x: 520,
      y: 218,
    });
  });

  test('returns the default position for a Y axis line when otherAxisDomainMax is undefined', () => {
    const result = getReferenceLineLabelPosition({
      axis: 'y',
      axisType: 'major',
      otherAxisDomainMin: 0,
      otherAxisPosition: 20000,
      viewBox: {
        x: 80,
        y: 218,
        width: 880,
        height: 0,
      },
    });

    expect(result).toEqual({
      x: 520,
      y: 218,
    });
  });

  test('returns the correct positions for minor X axis lines with an otherAxisPosition', () => {
    const result = getReferenceLineLabelPosition({
      axis: 'x',
      axisType: 'minor',
      otherAxisDomainMin: 0,
      otherAxisDomainMax: 100,
      otherAxisPosition: 20,
      viewBox: {
        x: 424,
        y: 0,
        width: 0,
        height: 250,
      },
    });

    expect(result).toEqual({
      x: 424,
      y: 200,
    });
  });

  test('returns the correct positions for major Y axis lines with an otherAxisPosition', () => {
    const result = getReferenceLineLabelPosition({
      axis: 'y',
      axisType: 'major',
      otherAxisDomainMin: 0,
      otherAxisDomainMax: 50000,
      otherAxisPosition: 20000,
      viewBox: {
        x: 80,
        y: 218,
        width: 880,
        height: 0,
      },
    });

    expect(result).toEqual({
      x: 432,
      y: 218,
    });
  });

  test('returns the correct positions for minor Y axis lines with an otherAxisPosition', () => {
    const result = getReferenceLineLabelPosition({
      axis: 'y',
      axisType: 'minor',
      otherAxisDomainMin: 0,
      otherAxisDomainMax: 100,
      otherAxisPosition: 20,
      viewBox: {
        x: 80,
        y: 158,
        width: 880,
        height: 0,
      },
    });

    expect(result).toEqual({
      x: 256,
      y: 158,
    });
  });
});
