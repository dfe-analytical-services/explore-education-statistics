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
  const testMeta: FullTableMeta = {
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
        order: 0,
      },
    },
    indicators: [
      new Indicator({
        label: 'Authorised absence rate',
        value: 'authorised-absence-rate',
        unit: '%',
        name: 'sess_authorised_percent',
      }),
    ],
    locations: [
      new LocationFilter({
        id: 'barnsley-id',
        value: 'barnsley',
        label: 'Barnsley',
        level: 'localAuthority',
      }),
      new LocationFilter({
        id: 'barnet-id',
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
    render(<DataTableCaption meta={testMeta} />);

    expect(screen.getByTestId('dataTableCaption')).toHaveTextContent(
      "Authorised absence rate for 'Absence by characteristic' for Female and Male in Barnet and Barnsley for 2015/16",
    );
    expect(
      screen.queryByRole('button', { name: /full table title/ }),
    ).not.toBeInTheDocument();
  });

  test('renders `title` from prop', () => {
    render(<DataTableCaption meta={testMeta} title="Test title" />);

    expect(screen.getByText('Test title')).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: /full table title/ }),
    ).not.toBeInTheDocument();
  });
});
