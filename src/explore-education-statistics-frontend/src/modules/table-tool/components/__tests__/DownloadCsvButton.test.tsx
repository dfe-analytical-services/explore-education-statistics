import Papa from 'papaparse';
import React from 'react';
import { fireEvent, render } from 'react-testing-library';
import TimePeriod from '@common/services/types/TimePeriod';
import { FullTable } from '@frontend/services/permalinkService';
import DownloadCsvButton from '../DownloadCsvButton';

describe('DownloadCsvButton', () => {
  const emptyTable: FullTable = {
    subjectMeta: {
      publicationName: '',
      subjectId: '',
      subjectName: '',
      footnotes: [],
      filters: {
        characteristics: {
          legend: 'Characteristics',
          hint: '',
          options: {
            gender: {
              label: 'Gender',
              options: [
                { label: 'Male', value: 'gender_male' },
                { label: 'Female', value: 'gender_female' },
              ],
            },
          },
        },
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
      indicators: [
        {
          label: 'Authorised absence rate',
          value: 'auth_abs_rate',
          unit: '%',
        },
        {
          label: 'Number of authorised absence sessions',
          value: 'auth_abs_sess',
          unit: '',
        },
      ],
      locations: [
        {
          value: 'england',
          label: 'England',
          level: 'country',
        },
        { value: 'south_yorkshire', label: 'South Yorkshire', level: 'region' },
      ],
      timePeriodRange: [
        TimePeriod.fromString('2014_AY'),
        TimePeriod.fromString('2015_AY'),
      ],
    },
    results: [],
  };

  test('clicking generates complete csv', async () => {
    const { getByText } = render(
      <DownloadCsvButton
        publicationSlug="pupil-absence"
        fullTable={{
          ...emptyTable,
          results: [
            {
              filters: ['gender_male', 'school_primary'],
              timePeriod: '2014_AY',
              location: {
                country: {
                  code: 'england',
                  name: 'England',
                },
              },
              measures: {
                authAbsRate: '1.2',
                authAbsSess: '2',
              },
            },
            {
              filters: ['gender_male', 'school_secondary'],
              timePeriod: '2014_AY',
              location: {
                country: {
                  code: 'england',
                  name: 'England',
                },
              },
              measures: {
                authAbsRate: '3.4',
                authAbsSess: '4',
              },
            },
            {
              filters: ['gender_female', 'school_primary'],
              timePeriod: '2014_AY',
              location: {
                country: {
                  code: 'england',
                  name: 'England',
                },
              },
              measures: {
                authAbsRate: '5.6',
                authAbsSess: '6',
              },
            },
            {
              filters: ['gender_female', 'school_secondary'],
              timePeriod: '2014_AY',
              location: {
                country: {
                  code: 'england',
                  name: 'England',
                },
              },
              measures: {
                authAbsRate: '7.8',
                authAbsSess: '8',
              },
            },
            {
              filters: ['gender_male', 'school_primary'],
              timePeriod: '2015_AY',
              location: {
                country: {
                  code: 'england',
                  name: 'England',
                },
              },
              measures: {
                authAbsRate: '9',
                authAbsSess: '10',
              },
            },
            {
              filters: ['gender_male', 'school_secondary'],
              timePeriod: '2015_AY',
              location: {
                country: {
                  code: 'england',
                  name: 'England',
                },
              },
              measures: {
                authAbsRate: '11.2',
                authAbsSess: '12',
              },
            },
            {
              filters: ['gender_female', 'school_primary'],
              timePeriod: '2015_AY',
              location: {
                country: {
                  code: 'england',
                  name: 'England',
                },
              },
              measures: {
                authAbsRate: '13.4',
                authAbsSess: '14',
              },
            },
            {
              filters: ['gender_female', 'school_secondary'],
              timePeriod: '2015_AY',
              location: {
                country: {
                  code: 'england',
                  name: 'England',
                },
              },
              measures: {
                authAbsRate: '15.6',
                authAbsSess: '16',
              },
            },
            {
              filters: ['gender_male', 'school_primary'],
              timePeriod: '2014_AY',
              location: {
                region: {
                  code: 'south_yorkshire',
                  name: 'South Yorkshire',
                },
              },
              measures: {
                authAbsRate: '16.7',
                authAbsSess: '17',
              },
            },
            {
              filters: ['gender_male', 'school_secondary'],
              timePeriod: '2014_AY',
              location: {
                country: {
                  code: 'england',
                  name: 'England',
                },
              },
              measures: {
                authAbsRate: '17.8',
                authAbsSess: '18',
              },
            },
            {
              filters: ['gender_female', 'school_primary'],
              timePeriod: '2014_AY',
              location: {
                country: {
                  code: 'england',
                  name: 'England',
                },
              },
              measures: {
                authAbsRate: '18.9',
                authAbsSess: '19',
              },
            },
            {
              filters: ['gender_female', 'school_secondary'],
              timePeriod: '2014_AY',
              location: {
                country: {
                  code: 'england',
                  name: 'England',
                },
              },
              measures: {
                authAbsRate: '20.1',
                authAbsSess: '20',
              },
            },
            {
              filters: ['gender_male', 'school_primary'],
              timePeriod: '2014_AY',
              location: {
                country: {
                  code: 'england',
                  name: 'England',
                },
              },
              measures: {
                authAbsRate: '22.3',
                authAbsSess: '22',
              },
            },
            {
              filters: ['gender_male', 'school_secondary'],
              timePeriod: '2014_AY',
              location: {
                country: {
                  code: 'england',
                  name: 'England',
                },
              },
              measures: {
                authAbsRate: '24.5',
                authAbsSess: '25',
              },
            },
            {
              filters: ['gender_female', 'school_primary'],
              timePeriod: '2014_AY',
              location: {
                country: {
                  code: 'england',
                  name: 'England',
                },
              },
              measures: {
                authAbsRate: '26.7',
                authAbsSess: '27',
              },
            },
            {
              filters: ['gender_female', 'school_secondary'],
              timePeriod: '2014_AY',
              location: {
                country: {
                  code: 'england',
                  name: 'England',
                },
              },
              measures: {
                authAbsRate: '28.9',
                authAbsSess: '29',
              },
            },
          ],
        }}
      />,
    );

    jest.spyOn(Papa, 'unparse');

    fireEvent.click(getByText('Download underlying data (.csv)'));

    const csv = (Papa.unparse as jest.Mock).mock.calls[0][0];

    expect(csv).toMatchSnapshot();
  });

  test('generated csv does not format values', () => {
    const { getByText } = render(
      <DownloadCsvButton
        publicationSlug="pupil-absence"
        fullTable={{
          ...emptyTable,
          results: [
            {
              filters: ['gender_female', 'school_primary'],
              timePeriod: '2015_AY',
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
        }}
      />,
    );

    jest.spyOn(Papa, 'unparse');

    fireEvent.click(getByText('Download underlying data (.csv)'));

    const csv = (Papa.unparse as jest.Mock).mock.calls[0][0];

    expect(csv).toMatchSnapshot();
  });

  test('generated csv can contain suppressed or cells with no result ', () => {
    const { getByText } = render(
      <DownloadCsvButton
        publicationSlug="pupil-absence"
        fullTable={{
          ...emptyTable,
          results: [
            {
              filters: ['gender_female', 'school_primary'],
              timePeriod: '2015_AY',
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
        }}
      />,
    );

    jest.spyOn(Papa, 'unparse');

    fireEvent.click(getByText('Download underlying data (.csv)'));

    const csv = (Papa.unparse as jest.Mock).mock.calls[0][0];

    expect(csv).toMatchSnapshot();
  });
});
