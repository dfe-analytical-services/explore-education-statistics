import DataFileReplacementPairedIndicators from '@admin/pages/release/data/components/DataFileReplacementPairedIndicators';
import { IndicatorsMappingsPlan } from '@admin/services/dataReplacementService';
import _tableBuilderService from '@admin/services/tableBuilderService';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router';

jest.mock('@admin/services/tableBuilderService');

const tableBuilderService = _tableBuilderService as jest.Mocked<
  typeof _tableBuilderService
>;

describe('DataFileReplacementPairedIndicators', () => {
  const createMapping = (id: string, label: string, candidateKey?: string) => ({
    source: { id, label, name: id.replace('-', '_') },
    type: candidateKey ? ('AutoSet' as const) : ('Unset' as const),
    candidateKey,
  });

  const testIndicatorMappings: IndicatorsMappingsPlan = {
    candidates: {},
    mappings: {
      'indicator-1': createMapping('indicator-1', 'Indicator 1', 'new-1'),
      'indicator-2': createMapping('indicator-2', 'Indicator 2', 'new-2'),
      'indicator-3': createMapping('indicator-3', 'Indicator 3', 'new-3'),
      'indicator-4': createMapping('indicator-4', 'Indicator 4'),
    },
  };

  const renderComponent = () =>
    render(
      <MemoryRouter>
        <DataFileReplacementPairedIndicators
          indicatorMappings={testIndicatorMappings}
          publicationId="publication-1"
          releaseVersionId="release-1"
          subjectId="subject-1"
        />
      </MemoryRouter>,
    );

  test('renders maintained and dropped pairs', async () => {
    tableBuilderService.getPairedIndicators.mockResolvedValue({
      pairedIndicators: [
        { parentIndicatorId: 'indicator-1', childIndicatorId: 'indicator-2' },
        { parentIndicatorId: 'indicator-3', childIndicatorId: 'indicator-4' },
      ],
    });

    renderComponent();

    const rows = within(
      await screen.findByTestId('paired-indicators-table'),
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
    expect(tableBuilderService.getPairedIndicators).toHaveBeenCalledWith(
      'release-1',
      'subject-1',
    );
  });

  test('renders that all pairs are maintained when both indicators of every pair are mapped', async () => {
    tableBuilderService.getPairedIndicators.mockResolvedValue({
      pairedIndicators: [
        { parentIndicatorId: 'indicator-1', childIndicatorId: 'indicator-2' },
      ],
    });

    renderComponent();

    expect(
      await screen.findByText(
        'All paired indicators will be maintained by this data replacement.',
      ),
    ).toBeInTheDocument();
    expect(screen.queryByText('Dropped')).not.toBeInTheDocument();
  });

  test('renders a message when there are no paired indicators', async () => {
    tableBuilderService.getPairedIndicators.mockResolvedValue({
      pairedIndicators: [],
    });

    renderComponent();

    expect(
      await screen.findByText(
        'No indicators have been paired for this data set.',
      ),
    ).toBeInTheDocument();
    expect(
      screen.queryByTestId('paired-indicators-table'),
    ).not.toBeInTheDocument();
  });

  test('links to managing the indicators of the original data set', async () => {
    tableBuilderService.getPairedIndicators.mockResolvedValue({
      pairedIndicators: [],
    });

    renderComponent();

    expect(
      await screen.findByRole('link', { name: 'Manage paired indicators' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/data?manageIndicators=subject-1#reordering',
    );
  });
});
