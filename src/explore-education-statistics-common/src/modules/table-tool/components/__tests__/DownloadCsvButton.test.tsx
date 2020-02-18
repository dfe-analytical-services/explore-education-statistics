import {
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import React from 'react';
import { fireEvent, render } from 'react-testing-library';
import { writeFile } from 'xlsx';
import DownloadCsvButton, { getCsvData } from '../DownloadCsvButton';

jest.mock('xlsx', () => {
  const { utils } = jest.requireActual('xlsx');

  return {
    writeFile: jest.fn(),
    utils,
  };
});

describe('DownloadCsvButton', () => {
  const basicSubjectMeta = {
    geoJsonAvailable: false,
    publicationName: '',
    subjectName: '',
    footnotes: [],
    filters: {
      characteristics: {
        legend: 'Characteristics',
        hint: '',
        options: {
          gender: {
            label: 'Gender',
            options: [{ label: 'Female', value: 'gender_female' }],
          },
        },
      },
    },
    indicators: [
      new Indicator({
        label: 'Authorised absence rate',
        value: 'authAbsRate',
        unit: '%',
      }),
      new Indicator({
        label: 'Number of authorised absence sessions',
        value: 'authAbsSess',
        unit: '',
      }),
    ],
    locations: [
      new LocationFilter(
        {
          value: 'england',
          label: 'England',
        },
        'country',
      ),
    ],
    timePeriodRange: [
      new TimePeriodFilter({ code: 'AY', year: 2015, label: '2015/16' }),
    ],
  };

  test('calls `writeFile` when button is pressed', () => {
    const { getByText } = render(
      <DownloadCsvButton
        publicationSlug="pupil-absence"
        fullTable={{
          subjectMeta: {
            ...basicSubjectMeta,
            filters: {
              ...basicSubjectMeta.filters,
              schoolType: {
                legend: 'School type',
                hint: '',
                options: {
                  default: {
                    label: '',
                    options: [
                      {
                        label: 'State-funded-primary',
                        value: 'school_primary',
                      },
                      {
                        label: 'State-funded secondary',
                        value: 'school_secondary',
                      },
                    ],
                  },
                },
              },
            },
          },
          results: [
            {
              filters: ['gender_female', 'school_primary'],
              timePeriod: '2015_AY',
              geographicLevel: 'Country',
              location: {
                country: {
                  code: 'england',
                  name: 'England',
                },
              },
              measures: {
                authAbsRate: '123',
                authAbsSess: '456',
              },
            },
            {
              filters: ['gender_female', 'school_secondary'],
              timePeriod: '2015_AY',
              geographicLevel: 'Country',
              location: {
                country: {
                  code: 'england',
                  name: 'England',
                },
              },
              measures: {
                authAbsRate: '234',
                authAbsSess: '567',
              },
            },
          ],
        }}
      />,
    );

    fireEvent.click(
      getByText('Download the underlying data of this table (CSV)'),
    );

    const mockedWriteFile = writeFile as jest.Mock;

    expect(mockedWriteFile).toHaveBeenCalledTimes(1);
    expect(mockedWriteFile.mock.calls[0][0]).toMatchSnapshot();
    expect(mockedWriteFile.mock.calls[0][1]).toBe('data-pupil-absence.csv');
  });

  describe('getCsvData', () => {
    test('contains full set of data', async () => {
      const data = getCsvData({
        subjectMeta: {
          ...basicSubjectMeta,
          filters: {
            ...basicSubjectMeta.filters,
            schoolType: {
              legend: 'School type',
              hint: '',
              options: {
                default: {
                  label: '',
                  options: [
                    {
                      label: 'State-funded-primary',
                      value: 'school_primary',
                    },
                    {
                      label: 'State-funded secondary',
                      value: 'school_secondary',
                    },
                  ],
                },
              },
            },
          },
        },
        results: [
          {
            filters: ['gender_female', 'school_primary'],
            timePeriod: '2015_AY',
            geographicLevel: 'Country',
            location: {
              country: {
                code: 'england',
                name: 'England',
              },
            },
            measures: {
              authAbsRate: '123',
              authAbsSess: '456',
            },
          },
          {
            filters: ['gender_female', 'school_secondary'],
            timePeriod: '2015_AY',
            geographicLevel: 'Country',
            location: {
              country: {
                code: 'england',
                name: 'England',
              },
            },
            measures: {
              authAbsRate: '234',
              authAbsSess: '567',
            },
          },
        ],
      });

      expect(data).toHaveLength(3);

      expect(data[0]).toHaveLength(6);

      expect(data[1]).toHaveLength(6);
      expect(data[1][4]).toBe('123');
      expect(data[1][5]).toBe('456');

      expect(data[2]).toHaveLength(6);
      expect(data[2][4]).toBe('234');
      expect(data[2][5]).toBe('567');

      expect(data).toMatchSnapshot();
    });

    test("contains n/a's if there are some matching results", () => {
      const data = getCsvData({
        subjectMeta: {
          ...basicSubjectMeta,
          filters: {
            ...basicSubjectMeta.filters,
            schoolType: {
              legend: 'School type',
              hint: '',
              options: {
                default: {
                  label: '',
                  options: [
                    {
                      label: 'State-funded-primary',
                      value: 'school_primary',
                    },
                    {
                      label: 'State-funded secondary',
                      value: 'school_secondary',
                    },
                  ],
                },
              },
            },
          },
        },
        results: [
          {
            filters: ['gender_female', 'school_primary'],
            timePeriod: '2015_AY',
            geographicLevel: 'Country',
            location: {
              country: {
                code: 'england',
                name: 'England',
              },
            },
            measures: {
              authAbsRate: '123',
              authAbsSess: '456',
            },
          },
        ],
      });

      expect(data).toHaveLength(3);

      expect(data[0]).toHaveLength(6);

      expect(data[1]).toHaveLength(6);
      expect(data[1][4]).toBe('123');
      expect(data[1][5]).toBe('456');

      expect(data[2]).toHaveLength(6);
      expect(data[2][4]).toBe('n/a');
      expect(data[2][5]).toBe('n/a');

      expect(data).toMatchSnapshot();
    });

    test("contains n/a's if there are no results", () => {
      const data = getCsvData({
        subjectMeta: basicSubjectMeta,
        results: [],
      });

      expect(data).toHaveLength(2);

      expect(data[0]).toHaveLength(5);

      expect(data[1]).toHaveLength(5);
      expect(data[1][3]).toBe('n/a');
      expect(data[1][4]).toBe('n/a');

      expect(data).toMatchSnapshot();
    });

    test("contains n/a's if matching result but no matching indicator", () => {
      const data = getCsvData({
        subjectMeta: basicSubjectMeta,
        results: [
          {
            filters: ['gender_female'],
            timePeriod: '2015_AY',
            geographicLevel: 'Country',
            location: {
              country: {
                code: 'england',
                name: 'England',
              },
            },
            measures: {
              authAbsSess: '123',
            },
          },
        ],
      });

      expect(data).toHaveLength(2);

      expect(data[0]).toHaveLength(5);

      expect(data[1]).toHaveLength(5);
      expect(data[1][3]).toBe('n/a');
      expect(data[1][4]).toBe('123');

      expect(data).toMatchSnapshot();
    });

    test('does not format values in any way', () => {
      const data = getCsvData({
        subjectMeta: basicSubjectMeta,
        results: [
          {
            filters: ['gender_female'],
            timePeriod: '2015_AY',
            geographicLevel: 'Country',
            location: {
              country: {
                code: 'england',
                name: 'England',
              },
            },
            measures: {
              authAbsRate: '12300000',
              authAbsSess: '44255667.2356',
            },
          },
        ],
      });

      expect(data).toHaveLength(2);

      expect(data[0]).toHaveLength(5);

      expect(data[1]).toHaveLength(5);
      expect(data[1][3]).toBe('12300000');
      expect(data[1][4]).toBe('44255667.2356');

      expect(data).toMatchSnapshot();
    });

    test('can contain suppressed cells', () => {
      const data = getCsvData({
        subjectMeta: basicSubjectMeta,
        results: [
          {
            filters: ['gender_female'],
            timePeriod: '2015_AY',
            geographicLevel: 'Country',
            location: {
              country: {
                code: 'england',
                name: 'England',
              },
            },
            measures: {
              authAbsRate: '13.4',
              authAbsSess: 'x',
            },
          },
        ],
      });

      expect(data).toHaveLength(2);

      expect(data[0]).toHaveLength(5);

      expect(data[1]).toHaveLength(5);
      expect(data[1][3]).toBe('13.4');
      expect(data[1][4]).toBe('x');

      expect(data).toMatchSnapshot();
    });
  });
});
