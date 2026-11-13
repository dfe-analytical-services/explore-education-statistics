import DataFileReplacementPairedIndicators from '@admin/pages/release/data/components/DataFileReplacementPairedIndicators';
import { ReplacementPairedIndicator } from '@admin/pages/release/data/utils/replacementPairedIndicators';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router';

describe('DataFileReplacementPairedIndicators', () => {
  const testMaintainedPair: ReplacementPairedIndicator = {
    parentIndicatorId: 'indicator-1',
    parentLabel: 'Indicator 1',
    childIndicatorId: 'indicator-2',
    childLabel: 'Indicator 2',
    maintained: true,
  };

  const testDroppedPair: ReplacementPairedIndicator = {
    parentIndicatorId: 'indicator-3',
    parentLabel: 'Indicator 3',
    childIndicatorId: 'indicator-4',
    childLabel: 'Indicator 4',
    maintained: false,
  };

  const renderComponent = (pairedIndicators: ReplacementPairedIndicator[]) =>
    render(
      <MemoryRouter>
        <DataFileReplacementPairedIndicators
          manageIndicatorsPath="/manage-indicators"
          pairedIndicators={pairedIndicators}
        />
      </MemoryRouter>,
    );

  test('renders maintained and dropped pairs', () => {
    renderComponent([testMaintainedPair, testDroppedPair]);

    const rows = within(
      screen.getByTestId('paired-indicators-table'),
    ).getAllByRole('row');

    expect(rows).toHaveLength(3);

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent('Indicator 1');
    expect(row1Cells[1]).toHaveTextContent('Indicator 2');
    expect(row1Cells[2]).toHaveTextContent('Maintained');

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('Indicator 3');
    expect(row2Cells[1]).toHaveTextContent('Indicator 4');
    expect(row2Cells[2]).toHaveTextContent('Dropped');

    expect(
      screen.getByText(/One or more paired indicators will be dropped/),
    ).toBeInTheDocument();
  });

  test('renders that all pairs are maintained when none are dropped', () => {
    renderComponent([testMaintainedPair]);

    expect(
      screen.getByText(
        'All paired indicators will be maintained by this data replacement.',
      ),
    ).toBeInTheDocument();
    expect(screen.queryByText('Dropped')).not.toBeInTheDocument();
  });

  test('renders a message when there are no paired indicators', () => {
    renderComponent([]);

    expect(
      screen.getByText('No indicators have been paired for this data set.'),
    ).toBeInTheDocument();
    expect(
      screen.queryByTestId('paired-indicators-table'),
    ).not.toBeInTheDocument();
  });

  test('links to managing paired indicators', () => {
    renderComponent([]);

    expect(
      screen.getByRole('link', { name: 'Manage paired indicators' }),
    ).toHaveAttribute('href', '/manage-indicators');
  });
});
