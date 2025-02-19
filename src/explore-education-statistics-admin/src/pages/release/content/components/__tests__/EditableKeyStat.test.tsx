import generateReleaseContent from '@admin-test/generators/releaseContentGenerators';
import EditableKeyStat from '@admin/pages/release/content/components/EditableKeyStat';
import { ReleaseContentProvider } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import _dataBlockService from '@admin/services/dataBlockService';
import _keyStatisticService from '@admin/services/keyStatisticService';
import baseRender from '@common-test/render';
import {
  KeyStatisticDataBlock,
  KeyStatisticText,
} from '@common/services/publicationService';
import _tableBuilderService, {
  TableDataResponse,
} from '@common/services/tableBuilderService';
import { RenderResult, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ReactNode } from 'react';

jest.mock('@common/services/tableBuilderService');
const tableBuilderService = _tableBuilderService as jest.Mocked<
  typeof _tableBuilderService
>;

jest.mock('@admin/services/keyStatisticService');
const keyStatisticService = _keyStatisticService as jest.Mocked<
  typeof _keyStatisticService
>;

jest.mock('@admin/services/dataBlockService');
const dataBlockService = _dataBlockService as jest.Mocked<
  typeof _dataBlockService
>;

describe('EditableKeyStat', () => {
  describe('for `KeyStatisticText` type', () => {
    const testKeyStat: KeyStatisticText = {
      type: 'KeyStatisticText',
      id: 'keyStatDataBlock-1',
      title: 'Text title',
      statistic: 'Over 9000',
      trend: 'Text trend',
      guidanceTitle: 'Text guidance title',
      guidanceText: 'Text guidance text',
      order: 0,
      created: '2023-01-01',
    };

    test('renders correctly', async () => {
      render(
        <EditableKeyStat
          releaseVersionId="release-1"
          keyStat={testKeyStat}
          keyStats={[testKeyStat]}
        />,
      );

      await waitFor(() => {
        expect(
          tableBuilderService.getDataBlockTableData,
        ).not.toHaveBeenCalled();

        expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
          'Text title',
        );

        expect(screen.getByTestId('keyStat-statistic')).toHaveTextContent(
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
  });

  describe('for `KeyStatisticDataBlock` type', () => {
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
                options: [{ label: 'Filter 1', value: 'filter-1' }],
                order: 1,
              },
            },
            order: 0,
          },
        },
        locations: {
          country: [{ label: 'England', value: 'england' }],
        },
        timePeriodRange: [{ code: 'AY', label: '2020/21', year: 2020 }],
        indicators: [
          {
            label: 'Indicator',
            name: 'indicator',
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

    const testKeyStat: KeyStatisticDataBlock = {
      type: 'KeyStatisticDataBlock',
      id: 'keyStatDataBlock-1',
      trend: 'Trend',
      guidanceTitle: 'Guidance title',
      guidanceText: 'Guidance text',
      order: 0,
      created: '2023-01-01',
      dataBlockParentId: 'block-1',
    };

    test('renders correctly', async () => {
      tableBuilderService.getDataBlockTableData.mockResolvedValue(
        testTableDataResponse,
      );

      keyStatisticService.updateKeyStatisticDataBlock.mockResolvedValue(
        testKeyStat,
      );

      render(
        <EditableKeyStat
          releaseVersionId="release-1"
          keyStat={testKeyStat}
          keyStats={[testKeyStat]}
        />,
      );

      await waitFor(() => {
        expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(
          1,
        );

        expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
          'Indicator',
        );
        expect(screen.getByTestId('keyStat-statistic')).toHaveTextContent(
          '608,180',
        );
        expect(screen.getByTestId('keyStat-trend')).toHaveTextContent('Trend');
      });

      expect(
        screen.getByRole('button', {
          name: 'Guidance title',
        }),
      ).toBeInTheDocument();

      expect(screen.getByTestId('keyStat-guidanceText')).toHaveTextContent(
        'Guidance text',
      );
    });

    test('submitting updated values after editing calls correct service', async () => {
      tableBuilderService.getDataBlockTableData.mockResolvedValue(
        testTableDataResponse,
      );

      keyStatisticService.updateKeyStatisticDataBlock.mockResolvedValue(
        testKeyStat,
      );

      render(
        <EditableKeyStat
          releaseVersionId="release-1"
          keyStat={testKeyStat}
          keyStats={[testKeyStat]}
          isEditing
        />,
      );

      await waitFor(() => {
        expect(screen.getByText(/Edit/)).toBeInTheDocument();
      });

      await userEvent.click(
        screen.getByRole('button', {
          name: 'Edit key statistic: Indicator',
        }),
      );

      await waitFor(() => {
        expect(screen.getByLabelText('Trend')).toBeInTheDocument();
      });

      await userEvent.type(screen.getByLabelText('Trend'), ' - New');
      await userEvent.type(screen.getByLabelText('Guidance title'), ' - New');

      expect(
        keyStatisticService.updateKeyStatisticDataBlock,
      ).not.toHaveBeenCalled();

      await userEvent.click(screen.getByRole('button', { name: 'Save' }));

      await waitFor(() => {
        expect(
          keyStatisticService.updateKeyStatisticDataBlock,
        ).toHaveBeenCalledWith<
          Parameters<typeof keyStatisticService.updateKeyStatisticDataBlock>
        >('release-1', 'keyStatDataBlock-1', {
          trend: 'Trend - New',
          guidanceTitle: 'Guidance title - New',
          guidanceText: 'Guidance text',
        });
      });
    });

    test('removing key stat calls correct services', async () => {
      tableBuilderService.getDataBlockTableData.mockResolvedValue(
        testTableDataResponse,
      );

      keyStatisticService.updateKeyStatisticDataBlock.mockResolvedValue(
        testKeyStat,
      );

      render(
        <EditableKeyStat
          releaseVersionId="release-1"
          keyStat={testKeyStat}
          keyStats={[testKeyStat]}
          isEditing
        />,
      );

      await waitFor(() => {
        expect(screen.getByText(/Remove/)).toBeInTheDocument();
      });

      expect(keyStatisticService.deleteKeyStatistic).not.toHaveBeenCalled();
      expect(dataBlockService.getUnattachedDataBlocks).not.toHaveBeenCalled();

      await userEvent.click(
        screen.getByRole('button', {
          name: 'Remove key statistic: Indicator',
        }),
      );

      await waitFor(() => {
        expect(keyStatisticService.deleteKeyStatistic).toHaveBeenCalledWith<
          Parameters<typeof keyStatisticService.deleteKeyStatistic>
        >('release-1', 'keyStatDataBlock-1');

        expect(dataBlockService.getUnattachedDataBlocks).toHaveBeenCalledWith<
          Parameters<typeof dataBlockService.getUnattachedDataBlocks>
        >('release-1');
      });
    });

    test('invalid `keyStat` renders null', async () => {
      const { container } = render(
        <EditableKeyStat
          releaseVersionId="release-1"
          keyStat={
            {
              id: 'KeyStat-1',
              title: 'Key stat title',
              order: 0,
              created: '2023-01-01',
            } as never
          }
          keyStats={[]}
        />,
      );

      expect(container).toBeEmptyDOMElement();
    });
  });
});

function render(child: ReactNode): RenderResult {
  const testReleaseContent = generateReleaseContent({});
  return baseRender(
    <ReleaseContentProvider
      value={{
        ...testReleaseContent,
        canUpdateRelease: true,
      }}
    >
      {child}
    </ReleaseContentProvider>,
  );
}
