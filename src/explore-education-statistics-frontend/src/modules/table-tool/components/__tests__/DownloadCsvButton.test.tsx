import { PublicationSubjectMeta } from '@common/services/tableBuilderService';
import TimePeriod from '@common/services/types/TimePeriod';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
} from '@frontend/modules/table-tool/components/types/filters';
import Papa from 'papaparse';
import React from 'react';
import { fireEvent, render } from 'react-testing-library';
import DownloadCsvButton from '../DownloadCsvButton';

describe('DownloadCsvButton', () => {
  const testMeta: PublicationSubjectMeta = {
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
    indicators: {
      absenceFields: {
        label: 'Absence fields',
        options: [
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
      },
    },
    locations: {
      country: {
        legend: 'Country',
        options: [
          {
            value: 'england',
            label: 'England',
          },
        ],
      },
    },
    timePeriod: {
      legend: 'Academic year',
      hint: '',
      options: [
        {
          code: 'AY',
          label: '2014/15',
          year: 2014,
        },
        {
          code: 'AY',
          label: '2015/16',
          year: 2015,
        },
      ],
    },
  };

  test('clicking generates complete csv', async () => {
    const { getByText } = render(
      <DownloadCsvButton
        publicationSlug="pupil-absence"
        meta={testMeta}
        filters={{
          characteristics: [
            new CategoryFilter({ label: 'Male', value: 'gender_male' }),
            new CategoryFilter({ label: 'Female', value: 'gender_female' }),
          ],
          schoolType: [
            new CategoryFilter({
              label: 'State-funded primary',
              value: 'school_primary',
            }),
            new CategoryFilter({
              label: 'State-funded secondary',
              value: 'school_secondary',
            }),
          ],
        }}
        locations={[
          new LocationFilter({ value: 'england', label: 'England' }, 'country'),
          new LocationFilter(
            { value: 'south_yorkshire', label: 'South Yorkshire' },
            'region',
          ),
        ]}
        timePeriods={[
          TimePeriod.fromString('2014_AY'),
          TimePeriod.fromString('2015_AY'),
        ]}
        indicators={[
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
        ]}
        results={[
          {
            filters: ['gender_male', 'school_primary'],
            timeIdentifier: 'AY',
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
            year: 2014,
          },
          {
            filters: ['gender_male', 'school_secondary'],
            timeIdentifier: 'AY',
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
            year: 2014,
          },
          {
            filters: ['gender_female', 'school_primary'],
            timeIdentifier: 'AY',
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
            year: 2014,
          },
          {
            filters: ['gender_female', 'school_secondary'],
            timeIdentifier: 'AY',
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
            year: 2014,
          },
          {
            filters: ['gender_male', 'school_primary'],
            timeIdentifier: 'AY',
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
            year: 2015,
          },
          {
            filters: ['gender_male', 'school_secondary'],
            timeIdentifier: 'AY',
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
            year: 2015,
          },
          {
            filters: ['gender_female', 'school_primary'],
            timeIdentifier: 'AY',
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
            year: 2015,
          },
          {
            filters: ['gender_female', 'school_secondary'],
            timeIdentifier: 'AY',
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
            year: 2015,
          },
          {
            filters: ['gender_male', 'school_primary'],
            timeIdentifier: 'AY',
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
            year: 2014,
          },
          {
            filters: ['gender_male', 'school_secondary'],
            timeIdentifier: 'AY',
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
            year: 2014,
          },
          {
            filters: ['gender_female', 'school_primary'],
            timeIdentifier: 'AY',
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
            year: 2014,
          },
          {
            filters: ['gender_female', 'school_secondary'],
            timeIdentifier: 'AY',
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
            year: 2014,
          },
          {
            filters: ['gender_male', 'school_primary'],
            timeIdentifier: 'AY',
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
            year: 2015,
          },
          {
            filters: ['gender_male', 'school_secondary'],
            timeIdentifier: 'AY',
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
            year: 2015,
          },
          {
            filters: ['gender_female', 'school_primary'],
            timeIdentifier: 'AY',
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
            year: 2015,
          },
          {
            filters: ['gender_female', 'school_secondary'],
            timeIdentifier: 'AY',
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
            year: 2015,
          },
        ]}
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
        meta={testMeta}
        filters={{
          characteristics: [
            new CategoryFilter({ label: 'Female', value: 'gender_female' }),
          ],
          schoolType: [
            new CategoryFilter({
              label: 'State-funded primary',
              value: 'school_primary',
            }),
          ],
        }}
        locations={[
          new LocationFilter({ value: 'england', label: 'England' }, 'country'),
        ]}
        timePeriods={[TimePeriod.fromString('2015_AY')]}
        indicators={[
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
        ]}
        results={[
          {
            filters: ['gender_female', 'school_primary'],
            timeIdentifier: 'AY',
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
            year: 2015,
          },
        ]}
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
        meta={testMeta}
        filters={{
          characteristics: [
            new CategoryFilter({ label: 'Male', value: 'gender_male' }),
            new CategoryFilter({ label: 'Female', value: 'gender_female' }),
          ],
          schoolType: [
            new CategoryFilter({
              label: 'State-funded primary',
              value: 'school_primary',
            }),
          ],
        }}
        locations={[
          new LocationFilter({ value: 'england', label: 'England' }, 'country'),
        ]}
        timePeriods={[TimePeriod.fromString('2015_AY')]}
        indicators={[
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
        ]}
        results={[
          {
            filters: ['gender_female', 'school_primary'],
            timeIdentifier: 'AY',
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
            year: 2015,
          },
        ]}
      />,
    );

    jest.spyOn(Papa, 'unparse');

    fireEvent.click(getByText('Download underlying data (.csv)'));

    const csv = (Papa.unparse as jest.Mock).mock.calls[0][0];

    expect(csv).toMatchSnapshot();
  });
});
