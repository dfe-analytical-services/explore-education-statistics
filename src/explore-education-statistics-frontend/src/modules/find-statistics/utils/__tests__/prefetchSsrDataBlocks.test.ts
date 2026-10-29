import tableBuilderQueries from '@common/queries/tableBuilderQueries';
import {
  DataBlockViewModel,
  KeyStatistic,
} from '@common/services/publicationService';
import _tableBuilderService, {
  TableDataResponse,
} from '@common/services/tableBuilderService';
import prefetchSsrDataBlocks from '@frontend/modules/find-statistics/utils/prefetchSsrDataBlocks';
import { DehydratedState } from '@tanstack/react-query';
import noop from 'lodash/noop';

jest.mock('@common/services/tableBuilderService');

const tableBuilderService = jest.mocked(_tableBuilderService);

describe('prefetchSsrDataBlocks', () => {
  const releaseVersionId = 'release-version-1';

  const keyStatistics: KeyStatistic[] = [
    {
      type: 'KeyStatisticDataBlock',
      id: 'key-stat-1',
      dataBlockId: 'data-block-1',
    },
    {
      type: 'KeyStatisticDataBlock',
      id: 'key-stat-2',
      dataBlockId: 'data-block-2',
    },
  ];

  const testHomeContent = ({
    content = [],
    keyStatistics: testKeyStatistics = [],
  }: {
    content?: DataBlockViewModel[];
    keyStatistics?: KeyStatistic[];
  }) => ({
    content: [{ id: 'section-1', heading: 'Section 1', content }],
    keyStatistics: testKeyStatistics,
    keyStatisticsSecondarySection: { id: 'secondary-section', content: [] },
  });

  const testDataBlock = (dataBlockId: string): DataBlockViewModel => ({
    id: `content-block-${dataBlockId}`,
    type: 'DataBlockVersionLink',
    dataBlockVersion: {
      dataBlockId,
      table: {},
    } as DataBlockViewModel['dataBlockVersion'],
  });

  const testTableData = (value: string) =>
    ({
      subjectMeta: {
        indicators: [
          {
            label: 'Number of pupils',
            name: 'pupils',
            unit: '',
            value: 'indicator-1',
          },
        ],
      },
      results: [{ measures: { 'indicator-1': value } }],
    }) as unknown as TableDataResponse;

  // The same key `KeyStatDataBlock` reads, so it renders from the cache.
  const keyStatKey = (dataBlockId: string) =>
    tableBuilderQueries.getKeyStat(releaseVersionId, dataBlockId).queryKey;

  // The same key `DataBlockTabs` reads, so it renders from the cache.
  const tableKey = (dataBlockId: string) =>
    tableBuilderQueries.getDataBlockTable(releaseVersionId, dataBlockId)
      .queryKey;

  const getQueryKeys = (state: DehydratedState) =>
    state.queries.map(query => query.queryKey);

  test('returns the fetched data block key stats', async () => {
    tableBuilderService.getDataBlockTableData
      .mockResolvedValueOnce(testTableData('1000'))
      .mockResolvedValueOnce(testTableData('2000'));

    const state = await prefetchSsrDataBlocks({
      homeContent: testHomeContent({ keyStatistics }),
      releaseVersionId,
    });

    expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledWith(
      releaseVersionId,
      'data-block-1',
    );
    expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledWith(
      releaseVersionId,
      'data-block-2',
    );

    expect(getQueryKeys(state)).toEqual([
      keyStatKey('data-block-1'),
      keyStatKey('data-block-2'),
    ]);
    expect(state.queries[0].state.data).toEqual({
      title: 'Number of pupils',
      value: '1,000',
    });
  });

  test('returns the fetched data block tables', async () => {
    const tableData = testTableData('1000');
    tableBuilderService.getDataBlockTableData.mockResolvedValue(tableData);

    const state = await prefetchSsrDataBlocks({
      homeContent: testHomeContent({
        content: [testDataBlock('data-block-1')],
      }),
      releaseVersionId,
    });

    expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledWith(
      releaseVersionId,
      'data-block-1',
    );

    expect(getQueryKeys(state)).toEqual([tableKey('data-block-1')]);
    expect(state.queries[0].state.data).toEqual(tableData);
  });

  test('returns key stats and tables together', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableData('1000'),
    );

    const state = await prefetchSsrDataBlocks({
      homeContent: testHomeContent({
        content: [testDataBlock('data-block-3')],
        keyStatistics,
      }),
      releaseVersionId,
    });

    expect(getQueryKeys(state)).toEqual([
      keyStatKey('data-block-1'),
      keyStatKey('data-block-2'),
      tableKey('data-block-3'),
    ]);
  });

  test('does not throw or return a key stat whose fetch fails', async () => {
    const error = new Error('Something went wrong');

    tableBuilderService.getDataBlockTableData
      .mockResolvedValueOnce(testTableData('1000'))
      .mockRejectedValueOnce(error);

    // React Query logs failed queries itself outside of production.
    const consoleError = jest.spyOn(console, 'error').mockImplementation(noop);

    try {
      const state = await prefetchSsrDataBlocks({
        homeContent: testHomeContent({ keyStatistics }),
        releaseVersionId,
      });

      expect(getQueryKeys(state)).toEqual([keyStatKey('data-block-1')]);
      expect(consoleError).toHaveBeenCalledWith(error);
    } finally {
      consoleError.mockRestore();
    }
  });

  test('stops waiting after the timeout, and does not return pending key stats', async () => {
    tableBuilderService.getDataBlockTableData
      .mockResolvedValueOnce(testTableData('1000'))
      .mockReturnValueOnce(new Promise(noop));

    const state = await prefetchSsrDataBlocks({
      homeContent: testHomeContent({ keyStatistics }),
      releaseVersionId,
      timeoutMs: 10,
    });

    expect(getQueryKeys(state)).toEqual([keyStatKey('data-block-1')]);
  });

  test('does not fetch anything when there are no data blocks', async () => {
    const state = await prefetchSsrDataBlocks({
      homeContent: testHomeContent({
        keyStatistics: [
          {
            type: 'KeyStatisticText',
            id: 'key-stat-1',
            title: 'Text key stat',
            statistic: '100',
          },
        ],
      }),
      releaseVersionId,
    });

    expect(tableBuilderService.getDataBlockTableData).not.toHaveBeenCalled();
    expect(state.queries).toEqual([]);
  });
});
