import EditableKeyStat, {
  KeyStatsFormValues,
} from '@admin/components/editable/EditableKeyStat';
import _tableBuilderService, {
  TableDataResponse,
} from '@common/services/tableBuilderService';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

jest.mock('@common/services/tableBuilderService');

const tableBuilderService = _tableBuilderService as jest.Mocked<
  typeof _tableBuilderService
>;

describe('EditableKeyStat', () => {
  const testTableDataResponse: TableDataResponse = {
    subjectMeta: {
      publicationName: 'Test publication',
      subjectName: 'Test subject',
      geoJsonAvailable: false,
      filters: {
        Filter1: {
          legend: 'Filter 1',
          name: 'filter1',
          options: {
            FilterGroup1: {
              id: 'filter-group-1',
              label: 'Filter group 1',
              options: [
                {
                  label: 'Filter 1',
                  value: 'filter-1',
                },
              ],
              order: 1,
            },
          },
          order: 0,
        },
      },
      locations: {
        country: [
          {
            label: 'England',
            value: 'england',
          },
        ],
      },
      timePeriodRange: [{ code: 'AY', label: '2020/21', year: 2020 }],
      indicators: [
        {
          label: 'Number of applications received',
          name: 'applications_received',
          unit: '',
          value: 'indicator-1',
        },
      ],
      boundaryLevels: [],
      footnotes: [],
    },
    results: [
      {
        filters: ['filter-1'],
        geographicLevel: 'country',
        locationId: 'england',
        timePeriod: '2020_AY',
        measures: {
          'indicator-1': '608180',
        },
      },
    ],
  };

  test('renders correctly with read-only summary', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableDataResponse,
    );

    render(
      <EditableKeyStat
        releaseId="release-1"
        dataBlockId="block-1"
        name="Key Stat 1"
        onSubmit={noop}
        summary={{
          dataSummary: ['Down from 620,330 in 2017'],
          dataDefinitionTitle: ['What is the number of applications received?'],
          dataDefinition: [
            'Total number of applications received for places at primary and secondary schools.',
          ],
          dataKeys: [],
        }}
      />,
    );

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(
        1,
      );

      expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
        'Number of applications received',
      );

      expect(screen.getByTestId('keyStat-value')).toHaveTextContent('608,180');

      expect(screen.getByTestId('keyStat-summary')).toHaveTextContent(
        'Down from 620,330 in 2017',
      );

      expect(
        screen.getByRole('button', {
          name: 'What is the number of applications received?',
        }),
      ).toBeInTheDocument();

      expect(screen.getByTestId('keyStat-definition')).toHaveTextContent(
        'Total number of applications received for places at primary and secondary schools.',
      );
    });
  });

  test('renders correctly with a default data definition title of "Help"', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableDataResponse,
    );

    render(
      <EditableKeyStat
        releaseId="release-1"
        dataBlockId="block-1"
        name="Key Stat 1"
        onSubmit={noop}
        summary={{
          dataSummary: ['Down from 620,330 in 2017'],
          dataDefinitionTitle: ['Help'],
          dataDefinition: [
            'Total number of applications received for places at primary and secondary schools.',
          ],
          dataKeys: [],
        }}
      />,
    );

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(
        1,
      );

      expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
        'Number of applications received',
      );

      expect(screen.getByTestId('keyStat-value')).toHaveTextContent('608,180');

      expect(screen.getByTestId('keyStat-summary')).toHaveTextContent(
        'Down from 620,330 in 2017',
      );

      expect(
        screen.getByRole('button', {
          name: 'Help for Number of applications received',
        }),
      ).toBeInTheDocument();

      expect(screen.getByTestId('keyStat-definition')).toHaveTextContent(
        'Total number of applications received for places at primary and secondary schools.',
      );
    });
  });

  test('renders correctly with a blank data definition title ', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableDataResponse,
    );

    render(
      <EditableKeyStat
        releaseId="release-1"
        dataBlockId="block-1"
        name="Key Stat 1"
        onSubmit={noop}
        summary={{
          dataSummary: ['Down from 620,330 in 2017'],
          dataDefinitionTitle: [''],
          dataDefinition: [
            'Total number of applications received for places at primary and secondary schools.',
          ],
          dataKeys: [],
        }}
      />,
    );

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(
        1,
      );

      expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
        'Number of applications received',
      );

      expect(screen.getByTestId('keyStat-value')).toHaveTextContent('608,180');

      expect(screen.getByTestId('keyStat-summary')).toHaveTextContent(
        'Down from 620,330 in 2017',
      );

      expect(
        screen.getByRole('button', {
          name: 'Help for Number of applications received',
        }),
      ).toBeInTheDocument();

      expect(screen.getByTestId('keyStat-definition')).toHaveTextContent(
        'Total number of applications received for places at primary and secondary schools.',
      );
    });
  });

  test('renders correctly without read-only summary', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableDataResponse,
    );

    render(
      <EditableKeyStat
        releaseId="release-1"
        dataBlockId="block-1"
        name="Key Stat 1"
        onSubmit={noop}
      />,
    );

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(
        1,
      );

      expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
        'Number of applications received',
      );

      expect(screen.getByTestId('keyStat-value')).toHaveTextContent('608,180');

      expect(screen.queryByTestId('keyStat-summary')).not.toBeInTheDocument();
      expect(
        screen.queryByTestId('keyStat-definition'),
      ).not.toBeInTheDocument();
    });
  });

  test('clicking Edit button renders editable summary form with no initial summary', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableDataResponse,
    );

    render(
      <EditableKeyStat
        isEditing
        releaseId="release-1"
        dataBlockId="block-1"
        name="Key Stat 1"
        onSubmit={noop}
      />,
    );

    await waitFor(() => {
      expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
        'Number of applications received',
      );
    });

    userEvent.click(screen.getByRole('button', { name: 'Edit' }));

    expect(
      screen.getByText('Key Stat 1', { selector: 'h3' }),
    ).toBeInTheDocument();
    expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
      'Number of applications received',
    );
    expect(screen.getByTestId('keyStat-value')).toHaveTextContent('608,180');
    expect(screen.getByLabelText('Trend')).toHaveValue('');
    expect(screen.getByLabelText('Guidance title')).toHaveValue('Help');
    expect(screen.getByLabelText('Guidance text')).toHaveTextContent('');
  });

  test('clicking Edit button renders editable summary form with initial summary', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableDataResponse,
    );

    render(
      <EditableKeyStat
        isEditing
        releaseId="release-1"
        dataBlockId="block-1"
        name="Key Stat 1"
        onSubmit={noop}
        summary={{
          dataSummary: ['Down from 620,330 in 2017'],
          dataDefinitionTitle: ['What is the number of applications received?'],
          dataDefinition: [
            'Total number of applications received for places at primary and secondary schools.',
          ],
          dataKeys: [],
        }}
      />,
    );

    await waitFor(() => {
      expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
        'Number of applications received',
      );
    });

    userEvent.click(screen.getByRole('button', { name: 'Edit' }));

    expect(
      screen.getByText('Key Stat 1', { selector: 'h3' }),
    ).toBeInTheDocument();
    expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
      'Number of applications received',
    );
    expect(screen.getByTestId('keyStat-value')).toHaveTextContent('608,180');
    expect(screen.getByLabelText('Trend')).toHaveValue(
      'Down from 620,330 in 2017',
    );
    expect(screen.getByLabelText('Guidance title')).toHaveValue(
      'What is the number of applications received?',
    );
    expect(screen.getByLabelText('Guidance text')).toHaveTextContent(
      'Total number of applications received for places at primary and secondary schools.',
    );
  });

  test('clicking Remove button calls the `onRemove` callback prop', async () => {
    const handleRemove = jest.fn();

    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableDataResponse,
    );

    render(
      <EditableKeyStat
        isEditing
        releaseId="release-1"
        dataBlockId="block-1"
        name="Key Stat 1"
        onRemove={handleRemove}
        onSubmit={noop}
        summary={{
          dataSummary: ['Down from 620,330 in 2017'],
          dataDefinitionTitle: ['What is the number of applications received?'],
          dataDefinition: [
            'Total number of applications received for places at primary and secondary schools.',
          ],
          dataKeys: [],
        }}
      />,
    );

    await waitFor(() => {
      expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
        'Number of applications received',
      );
    });

    expect(handleRemove).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Remove' }));

    expect(handleRemove).toHaveBeenCalled();
  });

  test('clicking Cancel button shows read-only summary again', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableDataResponse,
    );

    render(
      <EditableKeyStat
        isEditing
        releaseId="release-1"
        dataBlockId="block-1"
        name="Key Stat 1"
        onSubmit={noop}
        summary={{
          dataSummary: ['Down from 620,330 in 2017'],
          dataDefinitionTitle: ['What is the number of applications received?'],
          dataDefinition: [
            'Total number of applications received for places at primary and secondary schools.',
          ],
          dataKeys: [],
        }}
      />,
    );

    await waitFor(() => {
      expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
        'Number of applications received',
      );
    });

    // Start editing
    userEvent.click(screen.getByRole('button', { name: 'Edit' }));

    expect(screen.getByLabelText('Trend')).toHaveValue(
      'Down from 620,330 in 2017',
    );

    // Cancel editing
    userEvent.click(screen.getByRole('button', { name: 'Cancel' }));

    expect(screen.queryByLabelText('Trend')).not.toBeInTheDocument();
    expect(screen.queryByLabelText('Guidance title')).not.toBeInTheDocument();
    expect(screen.queryByLabelText('Guidance text')).not.toBeInTheDocument();

    expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
      'Number of applications received',
    );

    expect(screen.getByTestId('keyStat-value')).toHaveTextContent('608,180');

    expect(screen.getByTestId('keyStat-summary')).toHaveTextContent(
      'Down from 620,330 in 2017',
    );

    expect(
      screen.getByRole('button', {
        name: 'What is the number of applications received?',
      }),
    ).toBeInTheDocument();

    expect(screen.getByTestId('keyStat-definition')).toHaveTextContent(
      'Total number of applications received for places at primary and secondary schools.',
    );
  });

  test('can submit with updated summary field values', async () => {
    const handleSubmit = jest.fn();

    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableDataResponse,
    );

    render(
      <EditableKeyStat
        isEditing
        releaseId="release-1"
        dataBlockId="block-1"
        name="Key Stat 1"
        onSubmit={handleSubmit}
      />,
    );

    await waitFor(() => {
      expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
        'Number of applications received',
      );
    });

    userEvent.click(screen.getByRole('button', { name: 'Edit' }));

    userEvent.clear(screen.getByLabelText('Trend'));
    await userEvent.type(screen.getByLabelText('Trend'), 'New trend text');

    userEvent.clear(screen.getByLabelText('Guidance title'));
    await userEvent.type(
      screen.getByLabelText('Guidance title'),
      '  New guidance title  ',
    );

    // Note that we can't change 'Guidance text' field
    // as CKEditor doesn't work in Jest

    expect(handleSubmit).not.toBeCalled();

    userEvent.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledTimes(1);
      expect(handleSubmit).toHaveBeenCalledWith({
        dataDefinition: '',
        dataDefinitionTitle: 'New guidance title',
        dataSummary: 'New trend text',
      } as KeyStatsFormValues);
    });
  });

  test('renders correctly if there was an error fetching the table data', async () => {
    tableBuilderService.getDataBlockTableData.mockRejectedValue(
      new Error('Something went wrong'),
    );

    render(
      <EditableKeyStat
        releaseId="release-1"
        dataBlockId="block-1"
        name="Key Stat 1"
        onRemove={noop}
        onSubmit={noop}
        summary={{
          dataSummary: ['Down from 620,330 in 2017'],
          dataDefinitionTitle: ['What is the number of applications received?'],
          dataDefinition: [
            'Total number of applications received for places at primary and secondary schools.',
          ],
          dataKeys: [],
        }}
      />,
    );

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(
        1,
      );
      expect(screen.getByText('Could not load key stat')).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Remove' }),
      ).toBeInTheDocument();

      expect(screen.queryByTestId('keyStat-title')).not.toBeInTheDocument();
      expect(screen.queryByTestId('keyStat-value')).not.toBeInTheDocument();
      expect(screen.queryByTestId('keyStat-summary')).not.toBeInTheDocument();
      expect(
        screen.queryByRole('button', { name: 'Edit' }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByTestId('keyStat-definition'),
      ).not.toBeInTheDocument();
    });
  });

  test('renders correctly if there is no matching result in the response', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue({
      ...testTableDataResponse,
      subjectMeta: {
        ...testTableDataResponse.subjectMeta,
        indicators: [
          {
            label: 'Number of applications received',
            name: 'applications_received',
            unit: '',
            value: 'indicator-1',
          },
        ],
      },
      results: [],
    });

    render(
      <EditableKeyStat
        releaseId="release-1"
        dataBlockId="block-1"
        name="Key Stat 1"
        onRemove={noop}
        onSubmit={noop}
        summary={{
          dataSummary: ['Down from 620,330 in 2017'],
          dataDefinitionTitle: ['What is the number of applications received?'],
          dataDefinition: [
            'Total number of applications received for places at primary and secondary schools.',
          ],
          dataKeys: [],
        }}
      />,
    );

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(
        1,
      );
      expect(screen.getByText('Could not load key stat')).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Remove' }),
      ).toBeInTheDocument();

      expect(screen.queryByTestId('keyStat-title')).not.toBeInTheDocument();
      expect(screen.queryByTestId('keyStat-value')).not.toBeInTheDocument();
      expect(screen.queryByTestId('keyStat-summary')).not.toBeInTheDocument();
      expect(
        screen.queryByRole('button', { name: 'Edit' }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByTestId('keyStat-definition'),
      ).not.toBeInTheDocument();
    });
  });

  test('clicking Remove button when there has been an error calls the `onRemove` callback prop', async () => {
    const handleRemove = jest.fn();

    tableBuilderService.getDataBlockTableData.mockRejectedValue(
      new Error('Something went wrong'),
    );

    render(
      <EditableKeyStat
        releaseId="release-1"
        dataBlockId="block-1"
        name="Key Stat 1"
        onRemove={handleRemove}
        onSubmit={noop}
        summary={{
          dataSummary: ['Down from 620,330 in 2017'],
          dataDefinitionTitle: ['What is the number of applications received?'],
          dataDefinition: [
            'Total number of applications received for places at primary and secondary schools.',
          ],
          dataKeys: [],
        }}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: 'Remove' }),
      ).toBeInTheDocument();
    });

    expect(handleRemove).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Remove' }));

    expect(handleRemove).toHaveBeenCalled();
  });
});
