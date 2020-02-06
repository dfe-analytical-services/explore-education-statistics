import {
  testData1,
  testData2,
  testData3,
} from '@common/modules/table-tool/components/__tests__/__data__/TimePeriodDataTable.data';
import TimePeriodDataTable, {
  createGroupHeaders,
} from '@common/modules/table-tool/components/TimePeriodDataTable';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import React from 'react';
import { render } from 'react-testing-library';

describe('TimePeriodDataTable', () => {
  test('renders table with two of every option', () => {
    const fullTable = mapFullTable(testData1.fullTable);

    const { container } = render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testData1.tableHeadersConfig,
          fullTable.subjectMeta,
        )}
      />,
    );

    expect(container.querySelectorAll('table thead tr')).toHaveLength(2);
    expect(container.querySelectorAll('table thead th')).toHaveLength(6);
    expect(
      container.querySelectorAll('table thead th[scope="colgroup"]'),
    ).toHaveLength(2);
    expect(
      container.querySelectorAll('table thead th[scope="col"]'),
    ).toHaveLength(4);

    expect(container.querySelectorAll('table tbody tr')).toHaveLength(8);
    expect(container.querySelectorAll('table tbody th')).toHaveLength(14);
    expect(
      container.querySelectorAll('table tbody th[scope="rowgroup"]'),
    ).toHaveLength(6);
    expect(
      container.querySelectorAll('table tbody th[scope="row"]'),
    ).toHaveLength(8);

    expect(container.querySelectorAll('table tbody td')).toHaveLength(32);

    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    expect(container.querySelector('table')!.innerHTML).toMatchSnapshot();
  });

  test('renders title without indicator when there is more than one', () => {
    const fullTable = mapFullTable(testData1.fullTable);

    const { findByText } = render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testData1.tableHeadersConfig,
          fullTable.subjectMeta,
        )}
      />,
    );

    expect(
      findByText(
        "Table showing 'Absence by characteristic' from 'Pupil absence' for 2013/14 to 2014/15 for Barnet and Barnsley",
      ),
    ).not.toBeNull();
  });

  test('renders table without indicators when there is only one', () => {
    const fullTable = mapFullTable(testData2.fullTable);

    const { container } = render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testData2.tableHeadersConfig,
          fullTable.subjectMeta,
        )}
      />,
    );

    expect(container.querySelectorAll('table thead tr')).toHaveLength(2);
    expect(container.querySelectorAll('table thead th')).toHaveLength(6);
    expect(
      container.querySelectorAll('table thead th[scope="colgroup"]'),
    ).toHaveLength(2);
    expect(
      container.querySelectorAll('table thead th[scope="col"]'),
    ).toHaveLength(4);

    expect(container.querySelectorAll('table tbody tr')).toHaveLength(4);
    expect(container.querySelectorAll('table tbody th')).toHaveLength(6);
    expect(
      container.querySelectorAll('table tbody th[scope="rowgroup"]'),
    ).toHaveLength(2);
    expect(
      container.querySelectorAll('table tbody th[scope="row"]'),
    ).toHaveLength(4);

    expect(container.querySelectorAll('table tbody td')).toHaveLength(16);

    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    expect(container.querySelector('table')!.innerHTML).toMatchSnapshot();
  });

  test('renders title with indicator when there is only one', () => {
    const fullTable = mapFullTable(testData2.fullTable);

    const { findByText } = render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testData2.tableHeadersConfig,
          fullTable.subjectMeta,
        )}
      />,
    );

    expect(
      findByText(
        "Table showing Authorised absence rate for 'Absence by characteristic' from 'Pupil absence' for 2013/14 to 2014/15 for Barnet and Barnsley",
      ),
    ).not.toBeNull();
  });

  test('renders table without time period when there is only one', () => {
    const fullTable = mapFullTable(testData3.fullTable);

    const { container } = render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testData3.tableHeadersConfig,
          fullTable.subjectMeta,
        )}
      />,
    );

    expect(container.querySelectorAll('table thead tr')).toHaveLength(1);
    expect(container.querySelectorAll('table thead th')).toHaveLength(2);
    expect(
      container.querySelectorAll('table thead th[scope="colgroup"]'),
    ).toHaveLength(0);
    expect(
      container.querySelectorAll('table thead th[scope="col"]'),
    ).toHaveLength(2);

    expect(container.querySelectorAll('table tbody tr')).toHaveLength(4);
    expect(container.querySelectorAll('table tbody th')).toHaveLength(6);
    expect(
      container.querySelectorAll('table tbody th[scope="rowgroup"]'),
    ).toHaveLength(2);
    expect(
      container.querySelectorAll('table tbody th[scope="row"]'),
    ).toHaveLength(4);

    expect(container.querySelectorAll('table tbody td')).toHaveLength(8);

    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    expect(container.querySelector('table')!.innerHTML).toMatchSnapshot();
  });

  test('renders title with time period when there is only one', () => {
    const fullTable = mapFullTable(testData3.fullTable);

    const { findByText } = render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testData3.tableHeadersConfig,
          fullTable.subjectMeta,
        )}
      />,
    );

    expect(
      findByText(
        "Table showing 'Absence by characteristic' from 'Pupil absence' for 2014/15 for Barnet and Barnsley",
      ),
    ).not.toBeNull();
  });

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
