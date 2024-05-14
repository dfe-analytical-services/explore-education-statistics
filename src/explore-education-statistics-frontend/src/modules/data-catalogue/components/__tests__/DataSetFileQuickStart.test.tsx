import render from '@common-test/render';
import DataSetFileQuickStart from '@frontend/modules/data-catalogue/components/DataSetFileQuickStart';
import { screen, within } from '@testing-library/react';
import React from 'react';

describe('DataSetFileQuickStart', () => {
  test('renders correctly', () => {
    render(
      <DataSetFileQuickStart
        id="api-data-set-id"
        name="Test API data set name"
        version="1.0"
      />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'API data set endpoints quick start',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'API data set details' }),
    ).toBeInTheDocument();

    expect(
      within(screen.getByTestId('API data set name')).getByText(
        'Test API data set name',
      ),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('API data set ID')).getByText(
        'api-data-set-id',
      ),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Latest API version')).getByText('1.0'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Data set summary' }),
    ).toBeInTheDocument();

    expect(screen.getByLabelText('GET data set summary')).toHaveDisplayValue(
      /data-sets\/api-data-set-id/,
    );
    expect(screen.getByLabelText('GET data set meta data')).toHaveDisplayValue(
      /data-sets\/api-data-set-id\/meta/,
    );
    expect(screen.getByLabelText('GET data set')).toHaveDisplayValue(
      /data-sets\/api-data-set-id\/query/,
    );
    expect(screen.getByLabelText('POST data set')).toHaveDisplayValue(
      /data-sets\/api-data-set-id\/query/,
    );
  });
});
