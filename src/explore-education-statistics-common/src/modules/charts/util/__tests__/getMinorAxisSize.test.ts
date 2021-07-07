import getMinorAxisSize from '@common/modules/charts/util/getMinorAxisSize';

describe('getMinorAxisSize', () => {
  const testDataSetCategories = [
    {
      dataSets: {
        key: {
          dataSet: { indicator: '', filters: [] },
          value: 200,
        },
      },
      filter: { value: '', label: '', id: '' },
    },
    {
      dataSets: {
        key: {
          dataSet: { indicator: '', filters: [] },
          value: 20000,
        },
      },
      filter: { value: '', label: '', id: '' },
    },
    {
      dataSets: {
        key: {
          dataSet: { indicator: '', filters: [] },
          value: 6,
        },
      },
      filter: { value: '', label: '', id: '' },
    },
  ];
  test('it returns the size based on the length of the higest value', () => {
    const size = getMinorAxisSize({
      dataSetCategories: testDataSetCategories,
      minorAxisSize: undefined,
      minorAxisDecimals: undefined,
      minorAxisUnit: '',
    });
    expect(size).toBe(60);
  });

  test('it returns the axis size if set by the user', () => {
    const size = getMinorAxisSize({
      dataSetCategories: testDataSetCategories,
      minorAxisSize: 55,
      minorAxisDecimals: undefined,
      minorAxisUnit: '',
    });
    expect(size).toBe(55);
  });
  test('it takes into account decimal places', () => {
    const size = getMinorAxisSize({
      dataSetCategories: testDataSetCategories,
      minorAxisSize: undefined,
      minorAxisDecimals: 3,
      minorAxisUnit: '',
    });
    expect(size).toBe(96);
  });

  test('it takes into account units', () => {
    const size = getMinorAxisSize({
      dataSetCategories: testDataSetCategories,
      minorAxisSize: undefined,
      minorAxisDecimals: undefined,
      minorAxisUnit: '%',
    });
    expect(size).toBe(72);
  });
});
