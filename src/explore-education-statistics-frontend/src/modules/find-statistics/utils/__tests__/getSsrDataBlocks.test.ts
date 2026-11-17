import {
  DataBlockViewModel,
  HtmlBlockViewModel,
} from '@common/services/publicationService';
import getSsrDataBlocks, {
  MAX_TABLES_TO_SSR,
  SsrDataBlocksHomeContent,
} from '@frontend/modules/find-statistics/utils/getSsrDataBlocks';
import range from 'lodash/range';

describe('getSsrDataBlocks', () => {
  const testHomeContent: SsrDataBlocksHomeContent = {
    content: [],
    keyStatistics: [],
    keyStatisticsSecondarySection: { id: 'secondary-section', content: [] },
  };

  // Only the parts `getSsrDataBlocks` reads.
  const testDataBlock = (
    dataBlockId: string,
    { hasTable = true }: { hasTable?: boolean } = {},
  ): DataBlockViewModel => ({
    id: `content-block-${dataBlockId}`,
    type: 'DataBlockVersionLink',
    dataBlockVersion: {
      dataBlockId,
      dataBlockVersionId: `${dataBlockId}-version`,
      table: hasTable ? {} : undefined,
    } as DataBlockViewModel['dataBlockVersion'],
  });

  const testHtmlBlock: HtmlBlockViewModel = {
    id: 'html-block',
    type: 'HtmlBlock',
    body: '<p>Test</p>',
  };

  describe('key stats', () => {
    test('returns the data block ids of data block key stats, in order', () => {
      const result = getSsrDataBlocks({
        ...testHomeContent,
        keyStatistics: [
          {
            type: 'KeyStatisticDataBlock',
            id: 'key-stat-1',
            dataBlockId: 'data-block-1',
          },
          {
            type: 'KeyStatisticText',
            id: 'key-stat-2',
            title: 'Text key stat',
            statistic: '100',
          },
          {
            type: 'KeyStatisticDataBlock',
            id: 'key-stat-3',
            dataBlockId: 'data-block-3',
          },
        ],
      });

      expect(result.keyStatIds).toEqual(['data-block-1', 'data-block-3']);
    });

    test('returns no key stat ids when there are only text key stats', () => {
      const result = getSsrDataBlocks({
        ...testHomeContent,
        keyStatistics: [
          {
            type: 'KeyStatisticText',
            id: 'key-stat-1',
            title: 'Text key stat',
            statistic: '100',
          },
        ],
      });

      expect(result.keyStatIds).toEqual([]);
    });
  });

  describe('tables', () => {
    test('returns the data block ids of the secondary key stats section, then content sections, in order', () => {
      const result = getSsrDataBlocks({
        ...testHomeContent,
        content: [
          {
            id: 'section-1',
            heading: 'Section 1',
            content: [testDataBlock('data-block-2'), testHtmlBlock],
          },
          {
            id: 'section-2',
            heading: 'Section 2',
            content: [testDataBlock('data-block-3')],
          },
        ],
        keyStatisticsSecondarySection: {
          id: 'secondary-section',
          content: [testDataBlock('data-block-1')],
        },
      });

      expect(result.tableIds).toEqual([
        'data-block-1',
        'data-block-2',
        'data-block-3',
      ]);
    });

    test('skips data blocks without a table', () => {
      const result = getSsrDataBlocks({
        ...testHomeContent,
        content: [
          {
            id: 'section-1',
            heading: 'Section 1',
            content: [
              testDataBlock('data-block-1', { hasTable: false }),
              testDataBlock('data-block-2'),
            ],
          },
        ],
      });

      expect(result.tableIds).toEqual(['data-block-2']);
    });

    test('returns no more than `MAX_TABLES_TO_SSR`', () => {
      const dataBlockIds = range(MAX_TABLES_TO_SSR + 1).map(
        index => `data-block-${index}`,
      );

      const result = getSsrDataBlocks({
        ...testHomeContent,
        content: [
          {
            id: 'section-1',
            heading: 'Section 1',
            content: dataBlockIds.map(id => testDataBlock(id)),
          },
        ],
      });

      expect(result.tableIds).toHaveLength(MAX_TABLES_TO_SSR);
      expect(result.tableIds).toEqual(dataBlockIds.slice(0, MAX_TABLES_TO_SSR));
    });

    test('returns no table ids when there are no data blocks', () => {
      const result = getSsrDataBlocks({
        ...testHomeContent,
        content: [
          { id: 'section-1', heading: 'Section 1', content: [testHtmlBlock] },
        ],
      });

      expect(result.tableIds).toEqual([]);
    });
  });
});
