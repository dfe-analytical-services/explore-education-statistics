import TimePeriod from '@common/services/types/TimePeriod';
import Papa from 'papaparse';
import React from 'react';
import { fireEvent, render } from 'react-testing-library';
import DownloadCsvButton from '../DownloadCsvButton';

describe('DownloadCsvButton', () => {
  test('clicking generates correct CSV', async () => {
    const { getByText } = render(
      <DownloadCsvButton
        publicationSlug="pupil-absence"
        meta={{
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
          locations: {},
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
        }}
        filters={{
          characteristics: [
            { label: 'Male', value: 'gender_male' },
            { label: 'Female', value: 'gender_female' },
          ],
          schoolType: [
            { label: 'State-funded primary', value: 'school_primary' },
            { label: 'State-funded secondary', value: 'school_secondary' },
          ],
        }}
        locations={{}}
        timePeriods={[
          TimePeriod.fromString('2014_AY'),
          TimePeriod.fromString('2015_AY'),
        ]}
        indicators={[
          {
            label: 'Authorised absence rate',
            value: 'authAbsRate',
            unit: '%',
          },
          {
            label: 'Number of authorised absence sessions',
            value: 'authAbsSess',
            unit: '',
          },
        ]}
        results={[
          {
            filters: ['gender_male', 'school_primary'],
            timeIdentifier: 'AY',
            measures: {
              authAbsRate: '1.2',
              authAbsSess: '2',
            },
            year: 2014,
          },
          {
            filters: ['gender_male', 'school_secondary'],
            timeIdentifier: 'AY',
            measures: {
              authAbsRate: '3.4',
              authAbsSess: '4',
            },
            year: 2014,
          },
          {
            filters: ['gender_female', 'school_primary'],
            timeIdentifier: 'AY',
            measures: {
              authAbsRate: '5.6',
              authAbsSess: '6',
            },
            year: 2014,
          },
          {
            filters: ['gender_female', 'school_secondary'],
            timeIdentifier: 'AY',
            measures: {
              authAbsRate: '7.8',
              authAbsSess: '8',
            },
            year: 2014,
          },
          {
            filters: ['gender_male', 'school_primary'],
            timeIdentifier: 'AY',
            measures: {
              authAbsRate: '9',
              authAbsSess: '10',
            },
            year: 2015,
          },
          {
            filters: ['gender_male', 'school_secondary'],
            timeIdentifier: 'AY',
            measures: {
              authAbsRate: '11.2',
              authAbsSess: '12',
            },
            year: 2015,
          },
          {
            filters: ['gender_female', 'school_primary'],
            timeIdentifier: 'AY',
            measures: {
              authAbsRate: '13.4',
              authAbsSess: '14',
            },
            year: 2015,
          },
          {
            filters: ['gender_female', 'school_secondary'],
            timeIdentifier: 'AY',
            measures: {
              authAbsRate: '15.6',
              authAbsSess: '16',
            },
            year: 2015,
          },
        ]}
      />,
    );

    jest.spyOn(Papa, 'unparse');

    fireEvent.click(getByText('Download data (.csv)'));

    const csv = (Papa.unparse as jest.Mock).mock.calls[0][0];

    expect(csv).toMatchSnapshot();
  });

  test('csv can contain suppressed or cells with no result ', () => {
    const { getByText } = render(
      <DownloadCsvButton
        publicationSlug="pupil-absence"
        meta={{
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
          locations: {},
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
        }}
        filters={{
          characteristics: [
            { label: 'Male', value: 'gender_male' },
            { label: 'Female', value: 'gender_female' },
          ],
          schoolType: [
            { label: 'State-funded primary', value: 'school_primary' },
            { label: 'State-funded secondary', value: 'school_secondary' },
          ],
        }}
        locations={{}}
        timePeriods={[
          TimePeriod.fromString('2014_AY'),
          TimePeriod.fromString('2015_AY'),
        ]}
        indicators={[
          {
            label: 'Authorised absence rate',
            value: 'authAbsRate',
            unit: '%',
          },
          {
            label: 'Number of authorised absence sessions',
            value: 'authAbsSess',
            unit: '',
          },
        ]}
        results={[
          {
            filters: ['gender_male', 'school_primary'],
            timeIdentifier: 'AY',
            measures: {
              authAbsRate: '1.2',
              authAbsSess: '2',
            },
            year: 2014,
          },
          {
            filters: ['gender_female', 'school_primary'],
            timeIdentifier: 'AY',
            measures: {
              authAbsRate: '5.6',
              authAbsSess: '6',
            },
            year: 2014,
          },
          {
            filters: ['gender_female', 'school_secondary'],
            timeIdentifier: 'AY',
            measures: {
              authAbsRate: '7.8',
              authAbsSess: '8',
            },
            year: 2014,
          },
          {
            filters: ['gender_male', 'school_primary'],
            timeIdentifier: 'AY',
            measures: {
              authAbsRate: '9',
              authAbsSess: '10',
            },
            year: 2015,
          },
          {
            filters: ['gender_male', 'school_secondary'],
            timeIdentifier: 'AY',
            measures: {
              authAbsRate: '11.2',
              authAbsSess: '12',
            },
            year: 2015,
          },
          {
            filters: ['gender_female', 'school_primary'],
            timeIdentifier: 'AY',
            measures: {
              authAbsRate: '13.4',
              authAbsSess: 'x',
            },
            year: 2015,
          },
          {
            filters: ['gender_female', 'school_secondary'],
            timeIdentifier: 'AY',
            measures: {
              authAbsRate: '15.6',
              authAbsSess: 'x',
            },
            year: 2015,
          },
        ]}
      />,
    );

    jest.spyOn(Papa, 'unparse');

    fireEvent.click(getByText('Download data (.csv)'));

    const csv = (Papa.unparse as jest.Mock).mock.calls[0][0];

    expect(csv).toMatchSnapshot();
  });
});
