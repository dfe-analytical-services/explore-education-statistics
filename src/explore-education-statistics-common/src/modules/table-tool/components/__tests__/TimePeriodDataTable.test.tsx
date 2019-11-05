import { createHeadersFromGroups } from '@common/modules/table-tool/components/TimePeriodDataTable';

describe('MultiHeaderTable', () => {
  test('createHeadersFromGroups 1', () => {
    const data = [
      [{ value: 'E92000001', label: 'England', level: '6' }],
      [
        {
          value: '1',
          label: 'Total',
          filterGroup: 'Total',
          isTotal: true,
        },
        {
          value: '53',
          label: 'Gender male',
          filterGroup: 'Gender',
          isTotal: false,
        },
        {
          value: '52',
          label: 'Gender female',
          filterGroup: 'Gender',
          isTotal: false,
        },
      ],
    ];

    const result = createHeadersFromGroups(data);

    expect(result).toStrictEqual([
      ['England'],
      ['Total', 'Gender', undefined],
      ['Total', 'Gender male', 'Gender female'],
    ]);
  });

  test('createHeadersFromGroups 2', () => {
    const data = [
      [{ value: 'E92000001', label: 'England', level: '6' }],
      [
        {
          value: '53',
          label: 'Gender male',
          filterGroup: 'Gender',
          isTotal: false,
        },
        {
          value: '52',
          label: 'Gender female',
          filterGroup: 'Gender',
          isTotal: false,
        },

        {
          value: '1',
          label: 'Total',
          filterGroup: 'Total',
          isTotal: true,
        },
      ],
    ];

    const result = createHeadersFromGroups(data);

    expect(result).toStrictEqual([
      ['England'],
      ['Gender', undefined, 'Total'],
      ['Gender male', 'Gender female', 'Total'],
    ]);
  });
});
