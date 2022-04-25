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
        Authorised absence rate for 'Absence by characteristic' for Male and Female in Barnet and Barnsley for 2015/16
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

  test('can show/hide full title when there are more than 10 locations', () => {
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
          new LocationFilter({
            value: 'seven',
            label: 'Seven',
            level: 'localAuthorityDistrict',
          }),
          new LocationFilter({
            value: 'eight',
            label: 'Eight',
            level: 'localAuthorityDistrict',
          }),
          new LocationFilter({
            value: 'nine',
            label: 'Nine',
            level: 'localAuthorityDistrict',
          }),
          new LocationFilter({
            value: 'ten',
            label: 'Ten',
            level: 'localAuthorityDistrict',
          }),
          new LocationFilter({
            value: 'eleven',
            label: 'Eleven',
            level: 'localAuthorityDistrict',
          }),
        ]}
      />,
    );

    expect(screen.getByTestId('dataTableCaption')).toMatchInlineSnapshot(`
      <strong
        data-testid="dataTableCaption"
      >
        Authorised absence rate for 'Absence by characteristic' for Male and Female in Eight, Eleven, Five, Four, Nine, One, Seven, Six, Ten, Three and 1 other location... for 2015/16
      </strong>
    `);

    userEvent.click(
      screen.getByRole('button', { name: 'View full table title' }),
    );

    expect(screen.getByTestId('dataTableCaption')).toMatchInlineSnapshot(`
      <strong
        data-testid="dataTableCaption"
      >
        Authorised absence rate for 'Absence by characteristic' for Male and Female in Eight, Eleven, Five, Four, Nine, One, Seven, Six, Ten, Three and Two for 2015/16
      </strong>
    `);

    userEvent.click(
      screen.getByRole('button', { name: 'Hide full table title' }),
    );

    expect(screen.getByTestId('dataTableCaption')).toMatchInlineSnapshot(`
      <strong
        data-testid="dataTableCaption"
      >
        Authorised absence rate for 'Absence by characteristic' for Male and Female in Eight, Eleven, Five, Four, Nine, One, Seven, Six, Ten, Three and 1 other location... for 2015/16
      </strong>
    `);
  });
});
