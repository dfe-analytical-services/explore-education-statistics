import { createGroupHeaders } from '@common/modules/table-tool/components/TimePeriodDataTable';

describe('TimePeriodDataTable', () => {
  describe('createGroupHeaders', () => {
    test('creates extra filter group headers', () => {
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
          {
            value: '71',
            label: 'Ethnicity Major Asian Total',
            filterGroup: 'Ethnic group major',
            isTotal: false,
          },
          {
            value: '72',
            label: 'Ethnicity Major Black Total',
            filterGroup: 'Ethnic group major',
            isTotal: false,
          },
        ],
      ];

      const result = createGroupHeaders(data);

      expect(result).toStrictEqual([
        { headers: [{ text: 'England' }] },
        {
          groups: [
            { text: 'Total' },
            { text: 'Gender', span: 2 },
            { text: 'Ethnic group major', span: 2 },
          ],
          headers: [
            { text: 'Total' },
            { text: 'Gender male' },
            { text: 'Gender female' },
            { text: 'Ethnicity Major Asian Total' },
            { text: 'Ethnicity Major Black Total' },
          ],
        },
      ]);
    });

    test('creates extra filter group headers in reverse', () => {
      const data = [
        [{ value: 'E92000001', label: 'England', level: '6' }],
        [
          {
            value: '71',
            label: 'Ethnicity Major Asian Total',
            filterGroup: 'Ethnic group major',
            isTotal: false,
          },
          {
            value: '72',
            label: 'Ethnicity Major Black Total',
            filterGroup: 'Ethnic group major',
            isTotal: false,
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

          {
            value: '1',
            label: 'Total',
            filterGroup: 'Total',
            isTotal: true,
          },
        ],
      ];

      const result = createGroupHeaders(data);

      expect(result).toStrictEqual([
        { headers: [{ text: 'England' }] },
        {
          groups: [
            { text: 'Ethnic group major', span: 2 },
            { text: 'Gender', span: 2 },
            { text: 'Total' },
          ],
          headers: [
            { text: 'Ethnicity Major Asian Total' },
            { text: 'Ethnicity Major Black Total' },
            { text: 'Gender male' },
            { text: 'Gender female' },
            { text: 'Total' },
          ],
        },
      ]);
    });

    test('does not create `Default` filter group headers for only `Default` filters', () => {
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
        [
          {
            value: '100',
            label: 'State-funded primary',
            filterGroup: 'Default',
            isTotal: false,
          },
        ],
      ];

      const result = createGroupHeaders(data);

      expect(result).toStrictEqual([
        { headers: [{ text: 'England' }] },
        {
          groups: [{ text: 'Total' }, { text: 'Gender', span: 2 }],
          headers: [
            { text: 'Total' },
            { text: 'Gender male' },
            { text: 'Gender female' },
          ],
        },
        { headers: [{ text: 'State-funded primary' }] },
      ]);
    });

    test('creates `Default` filter group header if some sibling filters are not `Default`', () => {
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
          {
            value: '100',
            label: 'Some default filter',
            filterGroup: 'Default',
            isTotal: false,
          },
        ],
      ];

      const result = createGroupHeaders(data);

      expect(result).toStrictEqual([
        { headers: [{ text: 'England' }] },
        {
          groups: [
            { text: 'Total' },
            { text: 'Gender', span: 2 },
            { text: 'Default' },
          ],
          headers: [
            { text: 'Total' },
            { text: 'Gender male' },
            { text: 'Gender female' },
            { text: 'Some default filter' },
          ],
        },
      ]);
    });
  });
});
