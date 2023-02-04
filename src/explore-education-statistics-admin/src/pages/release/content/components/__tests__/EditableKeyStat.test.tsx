import { testEditableRelease } from '@admin/pages/release/__data__/testEditableRelease';
import EditableKeyStat from '@admin/pages/release/content/components/EditableKeyStat';
import EditableKeyStatDataBlock from '@admin/pages/release/content/components/EditableKeyStatDataBlock';
import EditableKeyStatText from '@admin/pages/release/content/components/EditableKeyStatText';
import { ReleaseContentProvider } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import _keyStatisticService from '@admin/services/keyStatisticService';
import {
  KeyStatisticDataBlock,
  KeyStatisticText,
} from '@common/services/publicationService';
import _tableBuilderService, {
  TableDataResponse,
} from '@common/services/tableBuilderService';
import {
  render as baseRender,
  RenderResult,
  screen,
  waitFor,
} from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { noop } from 'lodash';
import React, { ReactNode } from 'react';

jest.mock('@common/services/tableBuilderService');
const tableBuilderService = _tableBuilderService as jest.Mocked<
  typeof _tableBuilderService
>;

jest.mock('@admin/services/keyStatisticService');
const keyStatisticService = _keyStatisticService as jest.Mocked<
  typeof _keyStatisticService
>;

describe('EditableKeyStat', () => {
  // TODO: EES-2469 Write tests for EditableKeyStatText
  // Text isEditing
  // Text isEditing form
  // Text isEditing form Cancel
  // Text onSubmit
  // Text onRemove

  describe('KeyStatisticText', () => {
    const keyStatText: KeyStatisticText = {
      type: 'KeyStatisticText',
      id: 'keyStatDataBlock-1',
      trend: 'Text trend',
      guidanceTitle: 'Text guidance title',
      guidanceText: 'Text guidance text',
      order: 0,
      created: '2023-01-01',
      title: 'Text title',
      statistic: 'Over 9000',
    };

    test('renders correctly when read-only', async () => {
      render(<EditableKeyStat releaseId="release-1" keyStat={keyStatText} />);

      await waitFor(() => {
        expect(
          tableBuilderService.getDataBlockTableData,
        ).not.toHaveBeenCalled();

        expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
          'Text title',
        );

        expect(screen.getByTestId('keyStat-value')).toHaveTextContent(
          'Over 9000',
        );

        expect(screen.getByTestId('keyStat-trend')).toHaveTextContent(
          'Text trend',
        );
      });

      expect(
        screen.getByRole('button', {
          name: 'Text guidance title',
        }),
      ).toBeInTheDocument();

      expect(screen.getByTestId('keyStat-guidanceText')).toHaveTextContent(
        'Text guidance text',
      );

      expect(
        screen.queryByRole('button', { name: /Edit/ }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByRole('button', { name: /Remove/ }),
      ).not.toBeInTheDocument();
    });

    test('renders correctly without trend when read-only', async () => {
      render(
        <EditableKeyStatText
          keyStat={{ ...keyStatText, trend: undefined }}
          onSubmit={noop}
        />,
      );

      await waitFor(() => {
        expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
          'Text title',
        );

        expect(screen.getByTestId('keyStat-value')).toHaveTextContent(
          'Over 9000',
        );

        expect(screen.queryByTestId('keyStat-trend')).not.toBeInTheDocument();
      });

      expect(
        screen.getByRole('button', {
          name: 'Text guidance title',
        }),
      ).toBeInTheDocument();

      expect(screen.getByTestId('keyStat-guidanceText')).toHaveTextContent(
        'Text guidance text',
      );

      expect(
        screen.queryByRole('button', { name: 'Remove' }),
      ).not.toBeInTheDocument();
    });

    test('renders correctly without guidanceTitle when read-only', async () => {
      render(
        <EditableKeyStatText
          keyStat={{ ...keyStatText, guidanceTitle: undefined }}
          onSubmit={noop}
        />,
      );

      await waitFor(() => {
        expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
          'Text title',
        );

        expect(screen.getByTestId('keyStat-value')).toHaveTextContent(
          'Over 9000',
        );

        expect(screen.getByTestId('keyStat-trend')).toHaveTextContent(
          'Text trend',
        );
      });

      expect(
        screen.getByRole('button', {
          name: 'Help for Text title',
        }),
      ).toBeInTheDocument();

      expect(screen.getByTestId('keyStat-guidanceText')).toHaveTextContent(
        'Text guidance text',
      );

      expect(
        screen.queryByRole('button', { name: /Remove/ }),
      ).not.toBeInTheDocument();
    });

    test('renders correctly without guidanceText when read-only', async () => {
      render(
        <EditableKeyStatText
          keyStat={{ ...keyStatText, guidanceText: undefined }}
          onSubmit={noop}
        />,
      );

      await waitFor(() => {
        expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
          'Text title',
        );

        expect(screen.getByTestId('keyStat-value')).toHaveTextContent(
          'Over 9000',
        );

        expect(screen.getByTestId('keyStat-trend')).toHaveTextContent(
          'Text trend',
        );
      });

      expect(
        screen.queryByRole('button', {
          name: 'Text guidance title',
        }),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByTestId('keyStat-guidanceText'),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByRole('button', { name: /Remove/ }),
      ).not.toBeInTheDocument();
    });
  });

  describe('KeyStatisticDataBlock', () => {
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
            label: 'DataBlock indicator',
            name: 'datablock_title',
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

    const keyStatDataBlock: KeyStatisticDataBlock = {
      type: 'KeyStatisticDataBlock',
      id: 'keyStatDataBlock-1',
      trend: 'DataBlock trend',
      guidanceTitle: 'DataBlock guidance title',
      guidanceText: 'DataBlock guidance text',
      order: 0,
      created: '2023-01-01',
      dataBlockId: 'block-1',
    };

    test('renders correctly when read-only', async () => {
      tableBuilderService.getDataBlockTableData.mockResolvedValue(
        testTableDataResponse,
      );

      render(
        <EditableKeyStat releaseId="release-1" keyStat={keyStatDataBlock} />,
      );

      await waitFor(() => {
        expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(
          1,
        );

        expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
          'DataBlock indicator',
        );

        expect(screen.getByTestId('keyStat-value')).toHaveTextContent(
          '608,180',
        );

        expect(screen.getByTestId('keyStat-trend')).toHaveTextContent(
          'DataBlock trend',
        );
      });

      expect(
        screen.getByRole('button', {
          name: 'DataBlock guidance title',
        }),
      ).toBeInTheDocument();

      expect(screen.getByTestId('keyStat-guidanceText')).toHaveTextContent(
        'DataBlock guidance text',
      );

      expect(
        screen.queryByRole('button', { name: /Remove/ }),
      ).not.toBeInTheDocument();
    });

    test('renders correctly without trend when read-only', async () => {
      tableBuilderService.getDataBlockTableData.mockResolvedValue(
        testTableDataResponse,
      );

      render(
        <EditableKeyStatDataBlock
          releaseId="release-1"
          keyStat={{ ...keyStatDataBlock, trend: undefined }}
          onSubmit={noop}
        />,
      );

      await waitFor(() => {
        expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(
          1,
        );

        expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
          'DataBlock indicator',
        );

        expect(screen.getByTestId('keyStat-value')).toHaveTextContent(
          '608,180',
        );

        expect(screen.queryByTestId('keyStat-trend')).not.toBeInTheDocument();
      });

      expect(
        screen.getByRole('button', {
          name: 'DataBlock guidance title',
        }),
      ).toBeInTheDocument();

      expect(screen.getByTestId('keyStat-guidanceText')).toHaveTextContent(
        'DataBlock guidance text',
      );

      expect(
        screen.queryByRole('button', { name: /Remove/ }),
      ).not.toBeInTheDocument();
    });

    test('renders correctly without guidanceTitle when read-only', async () => {
      tableBuilderService.getDataBlockTableData.mockResolvedValue(
        testTableDataResponse,
      );

      render(
        <EditableKeyStatDataBlock
          releaseId="release-1"
          keyStat={{ ...keyStatDataBlock, guidanceTitle: undefined }}
          onSubmit={noop}
        />,
      );

      await waitFor(() => {
        expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(
          1,
        );

        expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
          'DataBlock indicator',
        );

        expect(screen.getByTestId('keyStat-value')).toHaveTextContent(
          '608,180',
        );

        expect(screen.getByTestId('keyStat-trend')).toHaveTextContent(
          'DataBlock trend',
        );
      });

      expect(
        screen.getByRole('button', {
          name: 'Help for DataBlock indicator',
        }),
      ).toBeInTheDocument();

      expect(screen.getByTestId('keyStat-guidanceText')).toHaveTextContent(
        'DataBlock guidance text',
      );

      expect(
        screen.queryByRole('button', { name: /Remove/ }),
      ).not.toBeInTheDocument();
    });

    test('renders without guidanceText correctly when read-only', async () => {
      tableBuilderService.getDataBlockTableData.mockResolvedValue(
        testTableDataResponse,
      );

      render(
        <EditableKeyStatDataBlock
          releaseId="release-1"
          keyStat={{ ...keyStatDataBlock, guidanceText: undefined }}
          onSubmit={noop}
        />,
      );

      await waitFor(() => {
        expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(
          1,
        );

        expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
          'DataBlock indicator',
        );

        expect(screen.getByTestId('keyStat-value')).toHaveTextContent(
          '608,180',
        );

        expect(screen.getByTestId('keyStat-trend')).toHaveTextContent(
          'DataBlock trend',
        );
      });

      expect(
        screen.queryByRole('button', {
          name: 'DataBlock guidance title',
        }),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByTestId('keyStat-guidanceText'),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByRole('button', { name: /Remove/ }),
      ).not.toBeInTheDocument();
    });

    test('renders correctly when editable', async () => {
      tableBuilderService.getDataBlockTableData.mockResolvedValue(
        testTableDataResponse,
      );

      render(
        <EditableKeyStat
          releaseId="release-1"
          keyStat={keyStatDataBlock}
          isEditing
        />,
      );

      await waitFor(() => {
        expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(
          1,
        );

        expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
          'DataBlock indicator',
        );

        expect(screen.getByTestId('keyStat-value')).toHaveTextContent(
          '608,180',
        );

        expect(screen.getByTestId('keyStat-trend')).toHaveTextContent(
          'DataBlock trend',
        );
      });

      expect(
        screen.getByRole('button', {
          name: 'DataBlock guidance title',
        }),
      ).toBeInTheDocument();

      expect(screen.getByTestId('keyStat-guidanceText')).toHaveTextContent(
        'DataBlock guidance text',
      );

      expect(
        screen.getByRole('button', {
          name: 'Edit key statistic: DataBlock indicator',
        }),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('button', { name: /Remove/ }),
      ).not.toBeInTheDocument();
    });

    test('can click `Remove` button when editable', async () => {
      tableBuilderService.getDataBlockTableData.mockResolvedValue(
        testTableDataResponse,
      );

      const onRemove = jest.fn();
      render(
        <EditableKeyStat
          releaseId="release-1"
          keyStat={keyStatDataBlock}
          isEditing
          onRemove={onRemove}
        />,
      );

      await waitFor(() => {
        expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(
          1,
        );

        expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
          'DataBlock indicator',
        );

        expect(screen.getByTestId('keyStat-value')).toHaveTextContent(
          '608,180',
        );

        expect(screen.getByTestId('keyStat-trend')).toHaveTextContent(
          'DataBlock trend',
        );
      });

      expect(
        screen.getByRole('button', {
          name: 'DataBlock guidance title',
        }),
      ).toBeInTheDocument();

      expect(screen.getByTestId('keyStat-guidanceText')).toHaveTextContent(
        'DataBlock guidance text',
      );

      expect(
        screen.getByRole('button', {
          name: 'Remove key statistic: DataBlock indicator',
        }),
      ).toBeInTheDocument();

      userEvent.click(
        screen.getByRole('button', {
          name: 'Remove key statistic: DataBlock indicator',
        }),
      );

      await waitFor(() => {
        expect(onRemove).toHaveBeenCalledTimes(1);
      });
    });

    test('renders edit form correctly when editable', async () => {
      tableBuilderService.getDataBlockTableData.mockResolvedValue(
        testTableDataResponse,
      );

      render(
        <EditableKeyStat
          releaseId="release-1"
          keyStat={keyStatDataBlock}
          isEditing
        />,
      );

      await waitFor(() => {
        expect(screen.getByText('Edit')).toBeInTheDocument();
      });

      userEvent.click(
        screen.getByRole('button', {
          name: 'Edit key statistic: DataBlock indicator',
        }),
      );

      await waitFor(() => {
        expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
          'DataBlock indicator',
        );
        expect(screen.getByTestId('keyStat-value')).toHaveTextContent(
          '608,180',
        );
      });

      expect(screen.getByLabelText('Trend')).toHaveValue('DataBlock trend');
      expect(screen.getByLabelText('Guidance title')).toHaveValue(
        'DataBlock guidance title',
      );
      expect(screen.getByLabelText('Guidance text')).toHaveTextContent(
        'DataBlock guidance text',
      );

      expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Cancel' }),
      ).toBeInTheDocument();
    });

    test('submits edit form with updated values when editable', async () => {
      tableBuilderService.getDataBlockTableData.mockResolvedValue(
        testTableDataResponse,
      );

      keyStatisticService.updateKeyStatisticDataBlock.mockResolvedValue(
        keyStatDataBlock,
      );

      render(
        <EditableKeyStat
          releaseId="release-1"
          keyStat={keyStatDataBlock}
          isEditing
        />,
      );

      await waitFor(() => {
        expect(screen.getByText('Edit')).toBeInTheDocument();
      });

      userEvent.click(
        screen.getByRole('button', {
          name: 'Edit key statistic: DataBlock indicator',
        }),
      );

      await waitFor(() => {
        expect(screen.getByText('Save')).toBeInTheDocument();
      });

      await userEvent.clear(screen.getByLabelText('Trend'));
      await userEvent.type(screen.getByLabelText('Trend'), 'New trend');

      await userEvent.clear(screen.getByLabelText('Guidance title'));
      await userEvent.type(
        screen.getByLabelText('Guidance title'),
        '  New guidance title  ', // Whitespace should be trimmed
      );

      await userEvent.clear(screen.getByLabelText('Guidance text'));
      await userEvent.type(
        screen.getByLabelText('Guidance text'),
        'New guidance text',
      );

      userEvent.click(screen.getByRole('button', { name: 'Save' }));

      await waitFor(() => {
        expect(keyStatisticService.updateKeyStatisticDataBlock).toBeCalledTimes(
          1,
        );
        expect(keyStatisticService.updateKeyStatisticDataBlock).toBeCalledWith(
          'release-1',
          'keyStatDataBlock-1',
          {
            trend: 'New trend',
            guidanceTitle: 'New guidance title',
            guidanceText: 'New guidance text',
          },
        );
      });
    });

    test('click `Cancel` button when editable', async () => {
      tableBuilderService.getDataBlockTableData.mockResolvedValue(
        testTableDataResponse,
      );

      render(
        <EditableKeyStat
          releaseId="release-1"
          keyStat={keyStatDataBlock}
          isEditing
        />,
      );

      await waitFor(() => {
        expect(screen.getByText('Edit')).toBeInTheDocument();
      });

      userEvent.click(
        screen.getByRole('button', {
          name: 'Edit key statistic: DataBlock indicator',
        }),
      );

      await waitFor(() => {
        expect(screen.getByText('Cancel')).toBeInTheDocument();
      });

      userEvent.click(screen.getByRole('button', { name: 'Cancel' }));

      await waitFor(() => {
        expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
          'DataBlock indicator',
        );
        expect(screen.getByTestId('keyStat-value')).toHaveTextContent(
          '608,180',
        );
        expect(screen.getByTestId('keyStat-trend')).toHaveTextContent(
          'DataBlock trend',
        );
      });

      expect(
        screen.getByRole('button', {
          name: 'DataBlock guidance title',
        }),
      ).toBeInTheDocument();
      expect(screen.getByTestId('keyStat-guidanceText')).toHaveTextContent(
        'DataBlock guidance text',
      );

      expect(
        screen.queryByRole('button', { name: /Edit/ }),
      ).toBeInTheDocument();
      expect(
        screen.queryByRole('button', { name: /Remove/ }),
      ).not.toBeInTheDocument();

      expect(keyStatisticService.updateKeyStatisticDataBlock).not.toBeCalled();
    });

    test('can click `Remove` button when failed to load', async () => {
      tableBuilderService.getDataBlockTableData.mockRejectedValue(undefined);

      const onRemove = jest.fn();

      render(
        <EditableKeyStat
          releaseId="release-1"
          keyStat={keyStatDataBlock}
          onRemove={onRemove}
        />,
      );

      await waitFor(() => {
        expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(
          1,
        );

        expect(
          screen.getByText('Could not load key statistic'),
        ).toBeInTheDocument();

        expect(screen.queryByTestId('keyStat-title')).not.toBeInTheDocument();
        expect(screen.queryByTestId('keyStat-value')).not.toBeInTheDocument();
        expect(screen.queryByTestId('keyStat-trend')).not.toBeInTheDocument();
      });

      expect(
        screen.queryByRole('button', { name: 'Edit' }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByTestId('keyStat-guidanceText'),
      ).not.toBeInTheDocument();

      userEvent.click(
        screen.getByRole('button', {
          name: 'Remove key statistic',
        }),
      );

      await waitFor(() => {
        expect(onRemove).toBeCalledTimes(1);
      });
    });
  });

  test('invalid `keyStat` renders null', async () => {
    const { container } = render(
      <EditableKeyStat
        releaseId="release-1"
        keyStat={
          {
            id: 'KeyStat-1',
            title: 'Key stat title',
            order: 0,
            created: '2023-01-01',
          } as never
        }
      />,
    );

    expect(container).toBeEmptyDOMElement();
  });

  function render(child: ReactNode): RenderResult {
    return baseRender(
      <ReleaseContentProvider
        value={{
          release: testEditableRelease,
          canUpdateRelease: true,
          availableDataBlocks: [],
        }}
      >
        {child}
      </ReleaseContentProvider>,
    );
  }
});
