import getSsrDataBlocks from '@frontend/modules/find-statistics/utils/getSsrDataBlocks';

describe('getSsrDataBlocks', () => {
  test('returns the data block ids of data block key stats, in order', () => {
    const result = getSsrDataBlocks({
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
