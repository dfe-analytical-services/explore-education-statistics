import userEvent from '@testing-library/user-event';
import React from 'react';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { render, screen } from '@testing-library/react';
import DataTableCaption from '../DataTableCaption';

describe('DataTableCaption', () => {
  const testFullTableMeta: FullTableMeta = {
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
            value: 'gender_male',
            label: 'Male',
            group: 'Gender',
            category: 'Characteristic',
          }),
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
    ],
    locations: [
      new LocationFilter({
        value: 'barnsley',
        label: 'Barnsley',
        level: 'localAuthority',
      }),
      new LocationFilter({
        value: 'barnet',
        label: 'Barnet',
        level: 'localAuthority',
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

  test('renders correct default title', () => {
    render(<DataTableCaption {...testFullTableMeta} />);

    expect(screen.getByTestId('dataTableCaption')).toMatchInlineSnapshot(`
      <strong
        data-testid="dataTableCaption"
      >
        Authorised absence rate for 'Absence by characteristic' for Female and Male in Barnet and Barnsley for 2015/16
      </strong>
    `);
  });

  test('renders `title` from prop', () => {
    render(<DataTableCaption {...testFullTableMeta} title="Test title" />);

    expect(screen.getByText('Test title')).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: /full table title/ }),
    ).not.toBeInTheDocument();
  });

  test('can show/hide full title when there are more than 5 locations', () => {
    render(
      <DataTableCaption
        {...testFullTableMeta}
        locations={[
          new LocationFilter({
            value: 'one',
            label: 'One',
            level: 'localAuthority',
          }),
          new LocationFilter({
            value: 'two',
            label: 'Two',
            level: 'localAuthority',
          }),
          new LocationFilter({
            value: 'three',
            label: 'Three',
            level: 'localAuthority',
          }),
          new LocationFilter({
            value: 'Four',
            label: 'Four',
            level: 'localAuthorityDistrict',
          }),
          new LocationFilter({
            value: 'five',
            label: 'Five',
            level: 'localAuthorityDistrict',
          }),
          new LocationFilter({
            value: 'six',
            label: 'Six',
            level: 'localAuthorityDistrict',
          }),
        ]}
      />,
    );

    expect(screen.getByTestId('dataTableCaption')).toHaveTextContent(
      "Authorised absence rate for 'Absence by characteristic' for Female and Male in Five, Four, One, Six, Three and 1 other location for 2015/16",
    );

    userEvent.click(
      screen.getByRole('button', { name: 'View full table title' }),
    );

    expect(screen.getByTestId('dataTableCaption')).toHaveTextContent(
      "Authorised absence rate for 'Absence by characteristic' for Female and Male in Five, Four, One, Six, Three and Two for 2015/16",
    );

    userEvent.click(
      screen.getByRole('button', { name: 'Hide full table title' }),
    );

    expect(screen.getByTestId('dataTableCaption')).toHaveTextContent(
      "Authorised absence rate for 'Absence by characteristic' for Female and Male in Five, Four, One, Six, Three and 1 other location for 2015/16",
    );
  });

  test('can show/hide full title when there are more than 5 filters', () => {
    render(
      <DataTableCaption
        {...testFullTableMeta}
        filters={{
          ...testFullTableMeta.filters,
          Characteristic: {
            ...testFullTableMeta.filters.Characteristic,
            options: [
              new CategoryFilter({
                value: 'gender_female',
                label: 'Female',
                group: 'Gender',
                category: 'Characteristic',
              }),
              new CategoryFilter({
                value: 'gender_male',
                label: 'Male',
                group: 'Gender',
                category: 'Characteristic',
              }),
              new CategoryFilter({
                value: 'ethnicity_major_asian_total',
                label: 'Ethnicity Major Asian Total',
                group: 'Ethnic group major',
                category: 'Characteristic',
              }),
              new CategoryFilter({
                value: 'ethnicity_major_black_total',
                label: 'Ethnicity Major Black Total',
                group: 'Ethnic group major',
                category: 'Characteristic',
              }),
            ],
          },
          'School Type': {
            name: 'school_type',
            options: [
              new CategoryFilter({
                value: 'school_special',
                label: 'Special',
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
        }}
      />,
    );

    expect(screen.getByTestId('dataTableCaption')).toHaveTextContent(
      "Authorised absence rate for 'Absence by characteristic' for Ethnicity Major Asian Total, Ethnicity Major Black Total, Female, Male, Special and 2 other filters in Barnet and Barnsley for 2015/16",
    );

    userEvent.click(
      screen.getByRole('button', { name: 'View full table title' }),
    );

    expect(screen.getByTestId('dataTableCaption')).toHaveTextContent(
      "Authorised absence rate for 'Absence by characteristic' for Ethnicity Major Asian Total, Ethnicity Major Black Total, Female, Male, Special, State-funded primary and State-funded secondary in Barnet and Barnsley for 2015/16",
    );

    userEvent.click(
      screen.getByRole('button', { name: 'Hide full table title' }),
    );

    expect(screen.getByTestId('dataTableCaption')).toHaveTextContent(
      "Authorised absence rate for 'Absence by characteristic' for Ethnicity Major Asian Total, Ethnicity Major Black Total, Female, Male, Special and 2 other filters in Barnet and Barnsley for 2015/16",
    );
  });
});
