import render from '@common-test/render';
import DataSetFileApiQuickStart from '@frontend/modules/data-catalogue/components/DataSetFileApiQuickStart';
import { screen, within } from '@testing-library/react';
import React from 'react';

describe('DataSetFileApiQuickStart', () => {
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
    ).toHaveDisplayValue('http://localhost:5050/v1/data-sets/api-data-set-id');
    expect(
      screen.getByRole('link', { name: 'Guidance: Get data set summary' }),
    ).toHaveAttribute(
      'href',
      'https://dev.statistics.api.education.gov.uk/docs/reference-v1/endpoints/GetDataSet/',
    );

    expect(
      screen.getByLabelText('GET data set metadata URL'),
    ).toHaveDisplayValue(
      'http://localhost:5050/v1/data-sets/api-data-set-id/meta?dataSetVersion=1.0',
    );
    expect(
      screen.getByRole('link', { name: 'Guidance: Get data set metadata' }),
    ).toHaveAttribute(
      'href',
      'https://dev.statistics.api.education.gov.uk/docs/reference-v1/endpoints/GetDataSetMeta/',
    );

    expect(screen.getByLabelText('GET data set query URL')).toHaveDisplayValue(
      'http://localhost:5050/v1/data-sets/api-data-set-id/query?dataSetVersion=1.0',
    );
    expect(
      screen.getByRole('link', { name: 'Guidance: Query data set (GET)' }),
    ).toHaveAttribute(
      'href',
      'https://dev.statistics.api.education.gov.uk/docs/reference-v1/endpoints/QueryDataSetGet/',
    );

    expect(screen.getByLabelText('POST data set query URL')).toHaveDisplayValue(
      'http://localhost:5050/v1/data-sets/api-data-set-id/query?dataSetVersion=1.0',
    );
    expect(
      screen.getByRole('link', { name: 'Guidance: Query data set (POST)' }),
    ).toHaveAttribute(
      'href',
      'https://dev.statistics.api.education.gov.uk/docs/reference-v1/endpoints/QueryDataSetPost/',
    );

    expect(screen.getByLabelText('GET data set CSV URL')).toHaveDisplayValue(
      'http://localhost:5050/v1/data-sets/api-data-set-id/csv?dataSetVersion=1.0',
    );
    expect(
      screen.getByRole('link', { name: 'Guidance: Download data set as CSV' }),
    ).toHaveAttribute(
      'href',
      'https://dev.statistics.api.education.gov.uk/docs/reference-v1/endpoints/DownloadDataSetCsv/',
    );
  });
});
