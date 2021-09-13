import React from 'react';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { render, screen, fireEvent } from '@testing-library/react';
import DataTableCaption from '../DataTableCaption';

describe('DataTableCaption', () => {
  const testfullTableMeta: FullTableMeta = {
    geoJsonAvailable: false,
    publicationName: 'Pupil absence in schools in England',
    subjectName: 'Absence by characteristic',
    footnotes: [],
    boundaryLevels: [],
    filters: {
      Characteristic: {
        name: 'characteristic',
        options: [
          new CategoryFilter({
            value: 'total',
            label: 'Total',
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
    ],
    locations: [
      new LocationFilter({
        value: 'E92000001',
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

  test('with given title', () => {
    render(<DataTableCaption title="Test title" {...testfullTableMeta} />);

    expect(screen.queryByText('Test title')).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: /full table title/ }),
    ).not.toBeInTheDocument();
  });

  test('with multiple filters labelled "Total"', () => {
    const props: FullTableMeta = {
      ...testfullTableMeta,
      filters: {
        ...testfullTableMeta.filters,
        'School Type': {
          name: 'school_type',
          options: [
            new CategoryFilter({
              value: 'total',
              label: 'Total',
              category: 'School Type',
            }),
          ],
        },
      },
    };
    render(<DataTableCaption {...props} />);

    expect(screen.getByTestId('dataTableCaption')).toMatchInlineSnapshot(`
      <strong
        data-testid="dataTableCaption"
      >
        Authorised absence rate for 'Absence by characteristic' in England for 2015/16
      </strong>
    `);
  });

  test('with multiple filters', () => {
    const props: FullTableMeta = {
      ...testfullTableMeta,
      filters: {
        ...testfullTableMeta.filters,
        Characteristic: {
          ...testfullTableMeta.filters.Characteristic,
          options: [
            ...testfullTableMeta.filters.Characteristic.options,
            new CategoryFilter({
              value: 'gender_female',
              label: 'Female',
              group: 'Gender',
              category: 'Characteristic',
            }),
          ],
        },
        'School Type': {
          name: 'school_type',
          options: [
            new CategoryFilter({
              value: 'total',
              label: 'Total',
              category: 'School Type',
            }),
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
    };
    render(<DataTableCaption {...props} />);

    expect(screen.getByTestId('dataTableCaption')).toMatchInlineSnapshot(`
      <strong
        data-testid="dataTableCaption"
      >
        Authorised absence rate for 'Absence by characteristic' for Female, State-funded primary and State-funded secondary in England for 2015/16
      </strong>
    `);
  });

  test('with more than one TimePeriodFilter', () => {
    const props: FullTableMeta = {
      ...testfullTableMeta,
      timePeriodRange: [
        ...testfullTableMeta.timePeriodRange,
        new TimePeriodFilter({
          code: 'AY',
          year: 2016,
          label: '2016/17',
          order: 1,
        }),
        new TimePeriodFilter({
          code: 'AY',
          year: 2017,
          label: '2017/18',
          order: 2,
        }),
        new TimePeriodFilter({
          code: 'AY',
          year: 2018,
          label: '2018/19',
          order: 3,
        }),
      ],
    };
    render(<DataTableCaption {...props} />);

    expect(screen.getByTestId('dataTableCaption')).toMatchInlineSnapshot(`
      <strong
        data-testid="dataTableCaption"
      >
        Authorised absence rate for 'Absence by characteristic' in England between 2015/16 and 2018/19
      </strong>
    `);
  });

  test('with multiple LocationFilters but less than 10', () => {
    const props: FullTableMeta = {
      ...testfullTableMeta,
      locations: [
        ...testfullTableMeta.locations,
        new LocationFilter({
          value: 'E09000002',
          label: 'Barking and Dagenham',
          level: 'localAuthority',
        }),
        new LocationFilter({
          value: 'E09000003',
          label: 'Barnet',
          level: 'localAuthority',
        }),
        new LocationFilter({
          value: 'E07000223',
          label: 'Adur',
          level: 'localAuthorityDistrict',
        }),
        new LocationFilter({
          value: 'E07000026',
          label: 'Allerdale',
          level: 'localAuthorityDistrict',
        }),
      ],
    };
    render(<DataTableCaption {...props} />);

    expect(screen.getByTestId('dataTableCaption')).toMatchInlineSnapshot(`
      <strong
        data-testid="dataTableCaption"
      >
        Authorised absence rate for 'Absence by characteristic' in Adur, Allerdale, Barking and Dagenham, Barnet and England for 2015/16
      </strong>
    `);
  });

  test('with more than 10 LocationFilters', () => {
    const props: FullTableMeta = {
      ...testfullTableMeta,
      locations: [
        ...testfullTableMeta.locations,
        new LocationFilter({
          value: 'E09000004',
          label: 'Eleven',
          level: 'localAuthority',
        }),
        new LocationFilter({
          value: 'E09000002',
          label: 'Ten',
          level: 'localAuthority',
        }),
        new LocationFilter({
          value: 'E09000003',
          label: 'Nine',
          level: 'localAuthority',
        }),
        new LocationFilter({
          value: 'E07000223',
          label: 'Eight',
          level: 'localAuthorityDistrict',
        }),
        new LocationFilter({
          value: 'E07000026',
          label: 'Seven',
          level: 'localAuthorityDistrict',
        }),
        new LocationFilter({
          value: 'E07000027',
          label: 'One',
          level: 'localAuthorityDistrict',
        }),
        new LocationFilter({
          value: 'E07000028',
          label: 'Two',
          level: 'localAuthorityDistrict',
        }),
        new LocationFilter({
          value: 'E07000029',
          label: 'Three',
          level: 'localAuthorityDistrict',
        }),
        new LocationFilter({
          value: 'E07000030',
          label: 'Four',
          level: 'localAuthorityDistrict',
        }),
        new LocationFilter({
          value: 'E07000031',
          label: 'Five',
          level: 'localAuthorityDistrict',
        }),
        new LocationFilter({
          value: 'E07000032',
          label: 'Six',
          level: 'localAuthorityDistrict',
        }),
      ],
    };
    render(<DataTableCaption {...props} />);

    expect(screen.getByTestId('dataTableCaption')).toMatchInlineSnapshot(`
      <strong
        data-testid="dataTableCaption"
      >
        Authorised absence rate for 'Absence by characteristic' in Eight, Eleven, England, Five, Four, Nine, One, Seven, Six, Ten and 2 other locations... for 2015/16
      </strong>
    `);

    fireEvent.click(
      screen.getByRole('button', { name: 'View full table title' }),
    );

    expect(screen.getByTestId('dataTableCaption')).toMatchInlineSnapshot(`
      <strong
        data-testid="dataTableCaption"
      >
        Authorised absence rate for 'Absence by characteristic' in Eight, Eleven, England, Five, Four, Nine, One, Seven, Six, Ten, Three and Two for 2015/16
      </strong>
    `);

    fireEvent.click(
      screen.getByRole('button', { name: 'Hide full table title' }),
    );

    expect(screen.getByTestId('dataTableCaption')).toMatchInlineSnapshot(`
      <strong
        data-testid="dataTableCaption"
      >
        Authorised absence rate for 'Absence by characteristic' in Eight, Eleven, England, Five, Four, Nine, One, Seven, Six, Ten and 2 other locations... for 2015/16
      </strong>
    `);
  });

  test('with multiple indicators', () => {
    const props: FullTableMeta = {
      ...testfullTableMeta,
      indicators: [
        ...testfullTableMeta.indicators,
        new Indicator({
          label: 'Number of authorised absence sessions',
          value: 'authAbsSess',
          unit: '',
          name: 'sess_authorised',
        }),
      ],
    };
    render(<DataTableCaption {...props} />);

    expect(screen.getByTestId('dataTableCaption')).toMatchInlineSnapshot(`
      <strong
        data-testid="dataTableCaption"
      >
        'Absence by characteristic' in England for 2015/16
      </strong>
    `);
  });
});
