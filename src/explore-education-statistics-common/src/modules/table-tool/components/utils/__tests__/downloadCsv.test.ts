import {
  downloadCsvFile,
  getCsvData,
} from '@common/modules/table-tool/components/utils/downloadCsv';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { waitFor } from '@testing-library/react';
import { writeFile, WorkBook } from 'xlsx';

jest.mock('xlsx', () => {
  const { utils } = jest.requireActual('xlsx');

  return {
    writeFile: jest.fn(),
    utils,
  };
});

describe('Download CSV utils', () => {
  const basicSubjectMeta: FullTableMeta = {
    geoJsonAvailable: false,
    publicationName: '',
    subjectName: '',
    footnotes: [],
    boundaryLevels: [],
    filters: {
      Characteristic: {
        name: 'characteristic',
        options: [
          new CategoryFilter({
            value: 'gender_female',
            label: 'Female',
            group: 'Gender',
            category: 'Characteristic',
          }),
        ],
      },
    },
    indicators: [
      new Indicator({
        label: 'Authorised absence rate',
        value: 'authAbsRate',
        unit: '%',
        name: 'sess_authorised_percent',
      }),
      new Indicator({
        label: 'Number of authorised absence sessions',
        value: 'authAbsSess',
        unit: '',
        name: 'sess_authorised',
      }),
    ],
    locations: [
      new LocationFilter({
        value: 'england',
        label: 'England',
        level: 'country',
      }),
    ],
    timePeriodRange: [
      new TimePeriodFilter({
        code: 'AY',
        year: 2015,
        label: '2015/16',
        order: 0,
      }),
    ],
  };

  describe('getCsvData', () => {
    test('contains full set of data', async () => {
      const data = getCsvData({
        subjectMeta: {
          ...basicSubjectMeta,
          filters: {
            ...basicSubjectMeta.filters,
            'School Type': {
              name: 'school_type',
              options: [
                new CategoryFilter({
                  value: 'school_primary',
                  label: 'State-funded primary',
                  category: 'School Type',
                }),
                new CategoryFilter({
                  value: 'school_secondary',
                  label: 'State-funded secondary',
                  category: 'School Type',
                }),
              ],
            },
          },
          locations: [
            ...basicSubjectMeta.locations,
            new LocationFilter({
              value: 'barnsley',
              label: 'Barnsley',
              level: 'localAuthority',
            }),
          ],
        },
        results: [
          {
            filters: ['gender_female', 'school_primary'],
            timePeriod: '2015_AY',
            geographicLevel: 'country',
            location: {
              country: {
                code: 'england',
                name: 'England',
              },
            },
            measures: {
              authAbsRate: '111',
              authAbsSess: '222',
            },
          },
          {
            filters: ['gender_female', 'school_secondary'],
            timePeriod: '2015_AY',
            geographicLevel: 'country',
            location: {
              country: {
                code: 'england',
                name: 'England',
              },
            },
            measures: {
              authAbsRate: '333',
              authAbsSess: '444',
            },
          },
          {
            filters: ['gender_female', 'school_primary'],
            timePeriod: '2015_AY',
            geographicLevel: 'localAuthority',
            location: {
              localAuthority: {
                code: 'barnsley',
                name: 'Barnsley',
              },
            },
            measures: {
              authAbsRate: '555',
              authAbsSess: '666',
            },
          },
          {
            filters: ['gender_female', 'school_secondary'],
            timePeriod: '2015_AY',
            geographicLevel: 'localAuthority',
            location: {
              localAuthority: {
                code: 'barnsley',
                name: 'Barnsley',
              },
            },
            measures: {
              authAbsRate: '777',
              authAbsSess: '888',
            },
          },
        ],
      });

      expect(data).toHaveLength(5);

      expect(data[0]).toHaveLength(8);

      expect(data[1]).toHaveLength(8);
      expect(data[1][6]).toBe('111');
      expect(data[1][7]).toBe('222');

      expect(data[2]).toHaveLength(8);
      expect(data[2][6]).toBe('333');
      expect(data[2][7]).toBe('444');

      expect(data[3]).toHaveLength(8);
      expect(data[3][6]).toBe('555');
      expect(data[3][7]).toBe('666');

      expect(data[4]).toHaveLength(8);
      expect(data[4][6]).toBe('777');
      expect(data[4][7]).toBe('888');

      expect(data).toMatchSnapshot();
    });

    test("contains n/a's if there are some matching results", () => {
      const data = getCsvData({
        subjectMeta: {
          ...basicSubjectMeta,
          filters: {
            ...basicSubjectMeta.filters,
            'School Type': {
              name: 'school_type',
              options: [
                new CategoryFilter({
                  value: 'school_primary',
                  label: 'State-funded primary',
                  category: 'School Type',
                }),
                new CategoryFilter({
                  value: 'school_secondary',
                  label: 'State-funded secondary',
                  category: 'School Type',
                }),
              ],
            },
          },
        },
        results: [
          {
            filters: ['gender_female', 'school_primary'],
            timePeriod: '2015_AY',
            geographicLevel: 'country',
            location: {
              country: {
                code: 'england',
                name: 'England',
              },
            },
            measures: {
              authAbsRate: '111',
            },
          },
          {
            filters: ['gender_female', 'school_secondary'],
            timePeriod: '2015_AY',
            geographicLevel: 'country',
            location: {
              country: {
                code: 'england',
                name: 'England',
              },
            },
            measures: {
              authAbsSess: '222',
            },
          },
        ],
      });

      expect(data).toHaveLength(3);

      expect(data[0]).toHaveLength(8);

      expect(data[1]).toHaveLength(8);
      expect(data[1][6]).toBe('111');
      expect(data[1][7]).toBe('n/a');

      expect(data[2]).toHaveLength(8);
      expect(data[2][6]).toBe('n/a');
      expect(data[2][7]).toBe('222');

      expect(data).toMatchSnapshot();
    });

    test("strips out rows with only n/a's", () => {
      const data = getCsvData({
        subjectMeta: {
          ...basicSubjectMeta,
          filters: {
            ...basicSubjectMeta.filters,
            'School Type': {
              name: 'school_type',
              options: [
                new CategoryFilter({
                  value: 'school_primary',
                  label: 'State-funded primary',
                  category: 'School Type',
                }),
                new CategoryFilter({
                  value: 'school_secondary',
                  label: 'State-funded secondary',
                  category: 'School Type',
                }),
              ],
            },
          },
        },
        results: [
          {
            filters: ['gender_female', 'school_primary'],
            timePeriod: '2015_AY',
            geographicLevel: 'country',
            location: {
              country: {
                code: 'england',
                name: 'England',
              },
            },
            measures: {
              authAbsRate: '111',
              authAbsSess: '222',
            },
          },
        ],
      });

      expect(data).toHaveLength(2);

      expect(data[0]).toHaveLength(8);

      expect(data[1]).toHaveLength(8);
      expect(data[1][6]).toBe('111');
      expect(data[1][7]).toBe('222');

      expect(data).toMatchSnapshot();
    });

    test('returns only header if there are no results', () => {
      const data = getCsvData({
        subjectMeta: basicSubjectMeta,
        results: [],
      });

      expect(data).toHaveLength(1);

      expect(data[0]).toHaveLength(7);

      expect(data).toMatchSnapshot();
    });

    test("contains n/a's if matching result but no matching indicator", () => {
      const data = getCsvData({
        subjectMeta: basicSubjectMeta,
        results: [
          {
            filters: ['gender_female'],
            timePeriod: '2015_AY',
            geographicLevel: 'country',
            location: {
              country: {
                code: 'england',
                name: 'England',
              },
            },
            measures: {
              authAbsSess: '111',
            },
          },
        ],
      });

      expect(data).toHaveLength(2);

      expect(data[0]).toHaveLength(7);

      expect(data[1]).toHaveLength(7);
      expect(data[1][5]).toBe('n/a');
      expect(data[1][6]).toBe('111');

      expect(data).toMatchSnapshot();
    });

    test('does not format values in any way', () => {
      const data = getCsvData({
        subjectMeta: basicSubjectMeta,
        results: [
          {
            filters: ['gender_female'],
            timePeriod: '2015_AY',
            geographicLevel: 'country',
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

      expect(data[0]).toHaveLength(7);

      expect(data[1]).toHaveLength(7);
      expect(data[1][5]).toBe('12300000');
      expect(data[1][6]).toBe('44255667.2356');

      expect(data).toMatchSnapshot();
    });

    test('can contain suppressed cells', () => {
      const data = getCsvData({
        subjectMeta: basicSubjectMeta,
        results: [
          {
            filters: ['gender_female'],
            timePeriod: '2015_AY',
            geographicLevel: 'country',
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

      expect(data[0]).toHaveLength(7);

      expect(data[1]).toHaveLength(7);
      expect(data[1][5]).toBe('13.4');
      expect(data[1][6]).toBe('x');

      expect(data).toMatchSnapshot();
    });
  });

  describe('downloadCsvFile', () => {
    test('creates and downloads the csv file', async () => {
      const data = getCsvData({
        subjectMeta: basicSubjectMeta,
        results: [],
      });

      downloadCsvFile(data, 'The file name');

      const mockedWriteFile = writeFile as jest.Mock;
      await waitFor(() => {
        expect(mockedWriteFile).toHaveBeenCalledTimes(1);

        const workbook = mockedWriteFile.mock.calls[0][0] as WorkBook;

        expect(workbook.Sheets.Sheet1.A1.v).toBe('location');
        expect(workbook.Sheets.Sheet1.B1.v).toBe('location_code');
        expect(workbook.Sheets.Sheet1.C1.v).toBe('geographic_level');
        expect(workbook.Sheets.Sheet1.D1.v).toBe('time_period');
        expect(workbook.Sheets.Sheet1.E1.v).toBe('characteristic');
        expect(workbook.Sheets.Sheet1.F1.v).toBe('sess_authorised_percent');

        expect(mockedWriteFile.mock.calls[0][1]).toBe('The file name.csv');
      });
    });
  });
});
