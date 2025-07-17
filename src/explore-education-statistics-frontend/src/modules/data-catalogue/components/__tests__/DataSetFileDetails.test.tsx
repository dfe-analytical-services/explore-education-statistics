import render from '@common-test/render';
import { testDataSetFile } from '@frontend/modules/data-catalogue/__data__/testDataSets';
import DataSetFileDetails from '@frontend/modules/data-catalogue/components/DataSetFileDetails';
import { screen, within } from '@testing-library/react';
import React from 'react';

describe('DataSetFileDetails', () => {
  test('renders the data set details', async () => {
    render(<DataSetFileDetails dataSetFile={testDataSetFile} />);

    expect(
      within(screen.getByTestId('Theme')).getByText('Theme 1'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Publication')).getByText('Publication 1'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Release')).getByRole('link', {
        name: 'Release 1',
      }),
    ).toHaveAttribute('href', '/find-statistics/publication-slug/release-slug');
    expect(
      within(screen.getByTestId('Release type')).getByRole('button', {
        name: /Accredited official statistics/,
      }),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Number of rows')).getByText('65'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Geographic levels')).getByText(
        'Local authority, National',
      ),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Indicators')).getByText('Indicator 1'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Indicators')).getByText('Indicator 2'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Filters')).getByText('Filter 1'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Filters')).getByText('Filter 2'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Time period')).getByText('2023 to 2024'),
    ).toBeInTheDocument();
  });

  test('clicking the release type shows the modal', async () => {
    const { user } = render(
      <DataSetFileDetails dataSetFile={testDataSetFile} />,
    );

    await user.click(
      screen.getByRole('button', { name: /Accredited official statistics/ }),
    );

    const modal = within(screen.getByRole('dialog'));
    expect(
      modal.getByRole('heading', { name: 'Accredited official statistics' }),
    ).toBeInTheDocument();
    expect(
      modal.getByRole('button', { name: 'Close modal' }),
    ).toBeInTheDocument();
  });
});
