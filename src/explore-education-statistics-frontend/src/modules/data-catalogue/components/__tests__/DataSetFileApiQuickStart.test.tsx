import render from '@common-test/render';
import DataSetFileApiQuickStart from '@frontend/modules/data-catalogue/components/DataSetFileApiQuickStart';
import { screen, within } from '@testing-library/react';
import React from 'react';

describe('DataSetFileQuickStart', () => {
  test('renders correctly', () => {
    render(
      <DataSetFileApiQuickStart
        id="api-data-set-id"
        name="Test API data set name"
        version="1.0"
      />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'API data set quick start',
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
      within(screen.getByTestId('API data set version')).getByText('1.0'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Get data set summary' }),
    ).toBeInTheDocument();

    expect(
      screen.getByLabelText('GET data set summary URL'),
    ).toHaveDisplayValue(/data-sets\/api-data-set-id/);
    expect(
      screen.getByLabelText('GET data set metadata URL'),
    ).toHaveDisplayValue(
      /data-sets\/api-data-set-id\/meta\?dataSetVersion=1.0/,
    );
    expect(screen.getByLabelText('GET data set query URL')).toHaveDisplayValue(
      /data-sets\/api-data-set-id\/query\?dataSetVersion=1.0/,
    );
    expect(screen.getByLabelText('POST data set query URL')).toHaveDisplayValue(
      /data-sets\/api-data-set-id\/query\?dataSetVersion=1.0/,
    );
  });
});
