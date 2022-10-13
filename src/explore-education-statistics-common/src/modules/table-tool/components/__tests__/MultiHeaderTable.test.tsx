import Header from '@common/modules/table-tool/components/utils/Header';
import { render } from '@testing-library/react';
import React from 'react';
import MultiHeaderTable from '../MultiHeaderTable';

describe('MultiHeaderTable', () => {
  test('renders 2x2 table correctly', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[
          new Header('A', 'A')
            .addChild(new Header('C', 'C'))
            .addChild(new Header('D', 'D')),
          new Header('B', 'B')
            .addChild(new Header('C', 'C'))
            .addChild(new Header('D', 'D')),
        ]}
        rowHeaders={[
          new Header('1', '1')
            .addChild(new Header('3', '3'))
            .addChild(new Header('4', '4')),
          new Header('2', '2')
            .addChild(new Header('3', '3'))
            .addChild(new Header('4', '4')),
        ]}
        rows={[
          ['AC13', 'AD13', 'BC13', 'BD13'],
          ['AC14', 'AD14', 'BC14', 'BD14'],
          ['AC23', 'AD23', 'BC23', 'BC23'],
          ['AC24', 'AD24', 'BC23', 'BC24'],
        ]}
      />,
    );

    expect(container.querySelectorAll('thead tr')).toHaveLength(2);
    expect(
      container.querySelectorAll('thead tr:nth-child(1) th[scope="colgroup"]'),
    ).toHaveLength(2);
    expect(
      container.querySelectorAll('thead tr:nth-child(2) th[scope="col"]'),
    ).toHaveLength(4);

    expect(container.querySelectorAll('tbody tr')).toHaveLength(4);
    expect(container.querySelectorAll('tbody tr:nth-child(1) td')).toHaveLength(
      4,
    );
    expect(container.querySelectorAll('tbody tr:nth-child(2) td')).toHaveLength(
      4,
    );
    expect(container.querySelectorAll('tbody tr:nth-child(3) td')).toHaveLength(
      4,
    );
    expect(container.querySelectorAll('tbody tr:nth-child(4) td')).toHaveLength(
      4,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders 2x2x2 table correctly', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[
          new Header('A', 'A')
            .addChild(
              new Header('C', 'C')
                .addChild(new Header('E', 'E'))
                .addChild(new Header('F', 'F')),
            )
            .addChild(
              new Header('D', 'D')
                .addChild(new Header('E', 'E'))
                .addChild(new Header('F', 'F')),
            ),
          new Header('B', 'B')
            .addChild(
              new Header('C', 'C')
                .addChild(new Header('E', 'E'))
                .addChild(new Header('F', 'F')),
            )
            .addChild(
              new Header('D', 'D')
                .addChild(new Header('E', 'E'))
                .addChild(new Header('F', 'F')),
            ),
        ]}
        rowHeaders={[
          new Header('1', '1')
            .addChild(
              new Header('3', '3')
                .addChild(new Header('5', '5'))
                .addChild(new Header('6', '6')),
            )
            .addChild(
              new Header('4', '4')
                .addChild(new Header('5', '5'))
                .addChild(new Header('6', '6')),
            ),
          new Header('2', '2')
            .addChild(
              new Header('3', '3')
                .addChild(new Header('5', '5'))
                .addChild(new Header('6', '6')),
            )
            .addChild(
              new Header('4', '4')
                .addChild(new Header('5', '5'))
                .addChild(new Header('6', '6')),
            ),
        ]}
        rows={[
          [
            'ACE135',
            'ACF135',
            'ADE135',
            'ADF135',
            'BCE135',
            'BCF135',
            'BDE135',
            'BDF135',
          ],
          [
            'ACE136',
            'ACF136',
            'ADE136',
            'ADF136',
            'BCE136',
            'BCF136',
            'BDE136',
            'BDF136',
          ],
          [
            'ACE145',
            'ACF145',
            'ADE145',
            'ADF145',
            'BCE145',
            'BCF145',
            'BDE145',
            'BDF145',
          ],
          [
            'ACE146',
            'ACF146',
            'ADE146',
            'ADF146',
            'BCE146',
            'BCF146',
            'BDE146',
            'BDF146',
          ],
          [
            'ACE235',
            'ACF235',
            'ADE235',
            'ADF235',
            'BCE235',
            'BCF235',
            'BDE235',
            'BDF235',
          ],
          [
            'ACE236',
            'ACF236',
            'ADE236',
            'ADF236',
            'BCE236',
            'BCF236',
            'BDE236',
            'BDF236',
          ],
          [
            'ACE245',
            'ACF245',
            'ADE245',
            'ADF245',
            'BCE245',
            'BCF245',
            'BDE245',
            'BDF245',
          ],
          [
            'ACE246',
            'ACF246',
            'ADE246',
            'ADF246',
            'BCE246',
            'BCF246',
            'BDE246',
            'BDF246',
          ],
        ]}
      />,
    );

    expect(container.querySelectorAll('thead tr')).toHaveLength(3);
    expect(
      container.querySelectorAll('thead tr:nth-child(1) th[scope="colgroup"]'),
    ).toHaveLength(2);
    expect(
      container.querySelectorAll('thead tr:nth-child(2) th[scope="colgroup"]'),
    ).toHaveLength(4);
    expect(
      container.querySelectorAll('thead tr:nth-child(3) th[scope="col"]'),
    ).toHaveLength(8);

    expect(container.querySelectorAll('tbody tr')).toHaveLength(8);
    expect(container.querySelectorAll('tbody tr:nth-child(1) td')).toHaveLength(
      8,
    );
    expect(container.querySelectorAll('tbody tr:nth-child(2) td')).toHaveLength(
      8,
    );
    expect(container.querySelectorAll('tbody tr:nth-child(3) td')).toHaveLength(
      8,
    );
    expect(container.querySelectorAll('tbody tr:nth-child(4) td')).toHaveLength(
      8,
    );
    expect(container.querySelectorAll('tbody tr:nth-child(5) td')).toHaveLength(
      8,
    );
    expect(container.querySelectorAll('tbody tr:nth-child(6) td')).toHaveLength(
      8,
    );
    expect(container.querySelectorAll('tbody tr:nth-child(7) td')).toHaveLength(
      8,
    );
    expect(container.querySelectorAll('tbody tr:nth-child(8) td')).toHaveLength(
      8,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with one `rowgroup` header subgroup', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rowHeaders={[
          new Header('B', 'B').addChild(
            new Header('A', 'A')
              .addChild(new Header('C', 'C'))
              .addChild(new Header('D', 'D')),
          ),
        ]}
        rows={[
          ['BAC1', 'BAC2'],
          ['BAD1', 'BAD2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(2);
    expect(container.querySelectorAll('tbody td')).toHaveLength(4);

    // Row 1
    const row1Headers = container.querySelectorAll('tbody tr:nth-child(1) th');
    expect(row1Headers).toHaveLength(3);

    expect(row1Headers[0]).toHaveTextContent('B');
    expect(row1Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row1Headers[0]).toHaveAttribute('rowspan', '2');

    expect(row1Headers[1]).toHaveTextContent('A');
    expect(row1Headers[1]).toHaveAttribute('scope', 'rowgroup');
    expect(row1Headers[1]).toHaveAttribute('rowspan', '2');

    expect(row1Headers[2]).toHaveTextContent('C');
    expect(row1Headers[2]).toHaveAttribute('scope', 'row');
    expect(row1Headers[2]).toHaveAttribute('rowspan', '1');

    // Row 2
    const row2Headers = container.querySelectorAll('tbody tr:nth-child(2) th');
    expect(row2Headers).toHaveLength(1);

    expect(row2Headers[0]).toHaveTextContent('D');
    expect(row2Headers[0]).toHaveAttribute('scope', 'row');
    expect(row2Headers[0]).toHaveAttribute('rowspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with two `rowgroup` header subgroups', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rowHeaders={[
          new Header('B', 'B').addChild(
            new Header('A', 'A').addChild(new Header('F', 'F')),
          ),
          new Header('C', 'C')
            .addChild(new Header('D', 'D').addChild(new Header('F', 'F')))
            .addChild(new Header('E', 'E').addChild(new Header('F', 'F'))),
        ]}
        rows={[
          ['BAF1', 'BAF2'],
          ['CDF1', 'CDF2'],
          ['CEF1', 'CEF2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(3);
    expect(container.querySelectorAll('tbody td')).toHaveLength(6);

    // Row 1
    const row1Headers = container.querySelectorAll('tbody tr:nth-child(1) th');
    expect(row1Headers).toHaveLength(3);

    expect(row1Headers[0]).toHaveTextContent('B');
    expect(row1Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row1Headers[0]).toHaveAttribute('rowspan', '1');

    expect(row1Headers[1]).toHaveTextContent('A');
    expect(row1Headers[1]).toHaveAttribute('scope', 'rowgroup');
    expect(row1Headers[1]).toHaveAttribute('rowspan', '1');

    expect(row1Headers[2]).toHaveTextContent('F');
    expect(row1Headers[2]).toHaveAttribute('scope', 'row');
    expect(row1Headers[2]).toHaveAttribute('rowspan', '1');

    // Row 2
    const row2Headers = container.querySelectorAll('tbody tr:nth-child(2) th');
    expect(row2Headers).toHaveLength(3);

    expect(row2Headers[0]).toHaveTextContent('C');
    expect(row2Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row2Headers[0]).toHaveAttribute('rowspan', '2');

    expect(row2Headers[1]).toHaveTextContent('D');
    expect(row2Headers[1]).toHaveAttribute('scope', 'rowgroup');
    expect(row2Headers[1]).toHaveAttribute('rowspan', '1');

    expect(row2Headers[2]).toHaveTextContent('F');
    expect(row2Headers[2]).toHaveAttribute('scope', 'row');
    expect(row2Headers[2]).toHaveAttribute('rowspan', '1');

    // Row 3
    const row3Headers = container.querySelectorAll('tbody tr:nth-child(3) th');
    expect(row3Headers).toHaveLength(2);

    expect(row3Headers[0]).toHaveTextContent('E');
    expect(row3Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row3Headers[0]).toHaveAttribute('rowspan', '1');

    expect(row3Headers[1]).toHaveTextContent('F');
    expect(row3Headers[1]).toHaveAttribute('scope', 'row');
    expect(row3Headers[1]).toHaveAttribute('rowspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with three `rowgroup` header subgroups', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rowHeaders={[
          new Header('B', 'B').addChild(
            new Header('A', 'A').addChild(new Header('H', 'H')),
          ),
          new Header('C', 'C')
            .addChild(new Header('D', 'D').addChild(new Header('H', 'H')))
            .addChild(new Header('F', 'F').addChild(new Header('H', 'H'))),
          new Header('E', 'E').addChild(
            new Header('G', 'G').addChild(new Header('H', 'H')),
          ),
        ]}
        rows={[
          ['BAH1', 'BAH2'],
          ['CDH1', 'CDH2'],
          ['CFH1', 'CFH2'],
          ['EGH1', 'EGH2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(4);
    expect(container.querySelectorAll('tbody td')).toHaveLength(8);

    // Row 1
    const row1Headers = container.querySelectorAll('tbody tr:nth-child(1) th');
    expect(row1Headers).toHaveLength(3);

    expect(row1Headers[0]).toHaveTextContent('B');
    expect(row1Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row1Headers[0]).toHaveAttribute('rowspan', '1');

    expect(row1Headers[1]).toHaveTextContent('A');
    expect(row1Headers[1]).toHaveAttribute('scope', 'rowgroup');
    expect(row1Headers[1]).toHaveAttribute('rowspan', '1');

    expect(row1Headers[2]).toHaveTextContent('H');
    expect(row1Headers[2]).toHaveAttribute('scope', 'row');
    expect(row1Headers[2]).toHaveAttribute('rowspan', '1');

    // Row 2
    const row2Headers = container.querySelectorAll('tbody tr:nth-child(2) th');
    expect(row2Headers).toHaveLength(3);

    expect(row2Headers[0]).toHaveTextContent('C');
    expect(row2Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row2Headers[0]).toHaveAttribute('rowspan', '2');

    expect(row2Headers[1]).toHaveTextContent('D');
    expect(row2Headers[1]).toHaveAttribute('scope', 'rowgroup');
    expect(row2Headers[1]).toHaveAttribute('rowspan', '1');

    expect(row2Headers[2]).toHaveTextContent('H');
    expect(row2Headers[2]).toHaveAttribute('scope', 'row');
    expect(row2Headers[2]).toHaveAttribute('rowspan', '1');

    // Row 3
    const row3Headers = container.querySelectorAll('tbody tr:nth-child(3) th');
    expect(row3Headers).toHaveLength(2);

    expect(row3Headers[0]).toHaveTextContent('F');
    expect(row3Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row3Headers[0]).toHaveAttribute('rowspan', '1');

    expect(row3Headers[1]).toHaveTextContent('H');
    expect(row3Headers[1]).toHaveAttribute('scope', 'row');
    expect(row3Headers[1]).toHaveAttribute('rowspan', '1');

    // Row 4
    const row4Headers = container.querySelectorAll('tbody tr:nth-child(4) th');
    expect(row4Headers).toHaveLength(3);

    expect(row4Headers[0]).toHaveTextContent('E');
    expect(row4Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row4Headers[0]).toHaveAttribute('rowspan', '1');

    expect(row4Headers[1]).toHaveTextContent('G');
    expect(row4Headers[1]).toHaveAttribute('scope', 'rowgroup');
    expect(row4Headers[1]).toHaveAttribute('rowspan', '1');

    expect(row4Headers[2]).toHaveTextContent('H');
    expect(row4Headers[2]).toHaveAttribute('scope', 'row');
    expect(row4Headers[2]).toHaveAttribute('rowspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with `rowgroup` header merged with identical subgroup', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rowHeaders={[
          new Header('B', 'B').addChild(
            new Header('A', 'A').addChild(new Header('F', 'F')),
          ),
          new Header('C', 'C').addChild(
            new Header('C', 'C').addChild(new Header('F', 'F')),
          ),
          new Header('D', 'D').addChild(
            new Header('E', 'E').addChild(new Header('F', 'F')),
          ),
        ]}
        rows={[
          ['BAF1', 'BAF2'],
          ['CCF1', 'CCF2'],
          ['DEF1', 'DEF2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(3);
    expect(container.querySelectorAll('tbody td')).toHaveLength(6);

    // Row 2
    const row2Headers = container.querySelectorAll('tbody tr:nth-child(2) th');
    expect(row2Headers).toHaveLength(2);

    expect(row2Headers[0]).toHaveTextContent('C');
    expect(row2Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row2Headers[0]).toHaveAttribute('rowspan', '1');
    expect(row2Headers[0]).toHaveAttribute('colspan', '2');

    expect(row2Headers[1]).toHaveTextContent('F');
    expect(row2Headers[1]).toHaveAttribute('scope', 'row');
    expect(row2Headers[1]).toHaveAttribute('rowspan', '1');
    expect(row2Headers[1]).toHaveAttribute('colspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with multi-span `rowgroup` merged with its identical groups', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rowHeaders={[
          new Header('B', 'B').addChild(
            new Header('A', 'A').addChild(new Header('F', 'F')),
          ),
          new Header('C', 'C').addChild(
            new Header('C', 'C')
              .addChild(new Header('F', 'F'))
              .addChild(new Header('F', 'F')),
          ),
        ]}
        rows={[
          ['BAF1', 'BAF2'],
          ['CCF1', 'CCF2'],
          ['CCF1', 'CCF2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(3);
    expect(container.querySelectorAll('tbody td')).toHaveLength(6);

    // Row 2
    const row2Headers = container.querySelectorAll('tbody tr:nth-child(2) th');
    expect(row2Headers).toHaveLength(2);

    expect(row2Headers[0]).toHaveTextContent('C');
    expect(row2Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row2Headers[0]).toHaveAttribute('rowspan', '2');
    expect(row2Headers[0]).toHaveAttribute('colspan', '2');

    expect(row2Headers[1]).toHaveTextContent('F');
    expect(row2Headers[1]).toHaveAttribute('scope', 'row');
    expect(row2Headers[1]).toHaveAttribute('rowspan', '2');
    expect(row2Headers[1]).toHaveAttribute('colspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with multi-span `rowgroup` header merged with 2 identical groups ', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rowHeaders={[
          new Header('A', 'A').addChild(
            new Header('B', 'B').addChild(new Header('C', 'C')),
          ),
          new Header('D', 'D').addChild(
            new Header('D', 'D')
              .addChild(new Header('D', 'D'))
              .addChild(new Header('E', 'E')),
          ),
          new Header('F', 'F').addChild(
            new Header('G', 'G').addChild(new Header('H', 'H')),
          ),
        ]}
        rows={[
          ['ABC1', 'ABC2'],
          ['DDD1', 'DDD2'],
          ['DDE1', 'DDE2'],
          ['FGH1', 'FGH2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(4);
    expect(container.querySelectorAll('tbody td')).toHaveLength(8);

    // Row 2
    const row2Headers = container.querySelectorAll('tbody tr:nth-child(2) th');
    expect(row2Headers).toHaveLength(2);

    expect(row2Headers[0]).toHaveTextContent('D');
    expect(row2Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row2Headers[0]).toHaveAttribute('colspan', '2');
    expect(row2Headers[0]).toHaveAttribute('rowspan', '2');

    expect(row2Headers[1]).toHaveTextContent('D');
    expect(row2Headers[1]).toHaveAttribute('scope', 'row');
    expect(row2Headers[1]).toHaveAttribute('colspan', '1');
    expect(row2Headers[1]).toHaveAttribute('rowspan', '1');

    // Row 3
    const row3Headers = container.querySelectorAll('tbody tr:nth-child(3) th');
    expect(row3Headers).toHaveLength(1);

    expect(row3Headers[0]).toHaveTextContent('E');
    expect(row3Headers[0]).toHaveAttribute('scope', 'row');
    expect(row3Headers[0]).toHaveAttribute('colspan', '1');
    expect(row3Headers[0]).toHaveAttribute('rowspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('does not render `rowgroup` headers with multi-span subgroup with invalid rowspans and colspans', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rowHeaders={[
          new Header('B', 'B').addChild(
            new Header('A', 'A').addChild(new Header('E', 'E')),
          ),
          new Header('C', 'C')
            .addChild(
              new Header('C', 'C')
                .addChild(new Header('E', 'E'))
                .addChild(new Header('E', 'E')),
            )
            .addChild(new Header('D', 'D').addChild(new Header('E', 'E'))),
        ]}
        rows={[
          ['BAE1', 'BAE2'],
          ['CCE1', 'CCE2'],
          ['CDE1', 'CDE2'],
          ['CEE1', 'CEE2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(4);
    expect(container.querySelectorAll('tbody td')).toHaveLength(8);

    // Row 2
    const row2Headers = container.querySelectorAll('tbody tr:nth-child(2) th');
    expect(row2Headers).toHaveLength(3);

    expect(row2Headers[0]).toHaveTextContent('C');
    expect(row2Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row2Headers[0]).toHaveAttribute('rowspan', '3');
    expect(row2Headers[0]).toHaveAttribute('colspan', '1');

    expect(row2Headers[1]).toHaveTextContent('C');
    expect(row2Headers[1]).toHaveAttribute('scope', 'rowgroup');
    expect(row2Headers[1]).toHaveAttribute('rowspan', '2');
    expect(row2Headers[1]).toHaveAttribute('colspan', '1');

    expect(row2Headers[2]).toHaveTextContent('E');
    expect(row2Headers[2]).toHaveAttribute('scope', 'row');
    expect(row2Headers[2]).toHaveAttribute('rowspan', '2');
    expect(row2Headers[2]).toHaveAttribute('colspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with one `row` header subgroup', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rowHeaders={[
          new Header('A', 'A').addChild(
            new Header('B', 'B')
              .addChild(new Header('C', 'C'))
              .addChild(new Header('D', 'D')),
          ),
        ]}
        rows={[
          ['ABC1', 'ABC2'],
          ['ABD1', 'ABD2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(2);
    expect(container.querySelectorAll('tbody td')).toHaveLength(4);

    // Row 1
    const row1Headers = container.querySelectorAll('tbody tr:nth-child(1) th');
    expect(row1Headers).toHaveLength(3);

    expect(row1Headers[0]).toHaveTextContent('A');
    expect(row1Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row1Headers[0]).toHaveAttribute('rowspan', '2');

    expect(row1Headers[1]).toHaveTextContent('B');
    expect(row1Headers[1]).toHaveAttribute('scope', 'rowgroup');
    expect(row1Headers[1]).toHaveAttribute('rowspan', '2');

    expect(row1Headers[2]).toHaveTextContent('C');
    expect(row1Headers[2]).toHaveAttribute('scope', 'row');
    expect(row1Headers[2]).toHaveAttribute('rowspan', '1');

    // Row 2
    const row2Headers = container.querySelectorAll('tbody tr:nth-child(2) th');
    expect(row2Headers).toHaveLength(1);

    expect(row2Headers[0]).toHaveTextContent('D');
    expect(row2Headers[0]).toHaveAttribute('scope', 'row');
    expect(row2Headers[0]).toHaveAttribute('rowspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with two `row` header subgroups', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rowHeaders={[
          new Header('A', 'A')
            .addChild(new Header('B', 'B').addChild(new Header('D', 'D')))
            .addChild(
              new Header('C', 'C')
                .addChild(new Header('E', 'E'))
                .addChild(new Header('F', 'F')),
            ),
        ]}
        rows={[
          ['ABD1', 'ABD2'],
          ['ACE1', 'ACE2'],
          ['ACF1', 'ACF2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(3);
    expect(container.querySelectorAll('tbody td')).toHaveLength(6);

    // Row 1
    const row1Headers = container.querySelectorAll('tbody tr:nth-child(1) th');
    expect(row1Headers).toHaveLength(3);

    expect(row1Headers[0]).toHaveTextContent('A');
    expect(row1Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row1Headers[0]).toHaveAttribute('rowspan', '3');

    expect(row1Headers[1]).toHaveTextContent('B');
    expect(row1Headers[1]).toHaveAttribute('scope', 'rowgroup');
    expect(row1Headers[1]).toHaveAttribute('rowspan', '1');

    expect(row1Headers[2]).toHaveTextContent('D');
    expect(row1Headers[2]).toHaveAttribute('scope', 'row');
    expect(row1Headers[2]).toHaveAttribute('rowspan', '1');

    // Row 2
    const row2Headers = container.querySelectorAll('tbody tr:nth-child(2) th');
    expect(row2Headers).toHaveLength(2);

    expect(row2Headers[0]).toHaveTextContent('C');
    expect(row2Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row2Headers[0]).toHaveAttribute('rowspan', '2');

    expect(row2Headers[1]).toHaveTextContent('E');
    expect(row2Headers[1]).toHaveAttribute('scope', 'row');
    expect(row2Headers[1]).toHaveAttribute('rowspan', '1');

    // Row 3
    const row3Headers = container.querySelectorAll('tbody tr:nth-child(3) th');
    expect(row3Headers).toHaveLength(1);

    expect(row3Headers[0]).toHaveTextContent('F');
    expect(row3Headers[0]).toHaveAttribute('scope', 'row');
    expect(row3Headers[0]).toHaveAttribute('rowspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with three `row` header subgroups', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rowHeaders={[
          new Header('A', 'A')
            .addChild(new Header('B', 'B').addChild(new Header('E', 'E')))
            .addChild(
              new Header('C', 'C')
                .addChild(new Header('F', 'F'))
                .addChild(new Header('G', 'G')),
            )
            .addChild(new Header('D', 'D').addChild(new Header('H', 'H'))),
        ]}
        rows={[
          ['ABE1', 'ABE2'],
          ['ACF1', 'ACF2'],
          ['ACG1', 'ACG2'],
          ['ADH1', 'ADH2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(4);
    expect(container.querySelectorAll('tbody td')).toHaveLength(8);

    // Row 1
    const row1Headers = container.querySelectorAll('tbody tr:nth-child(1) th');
    expect(row1Headers).toHaveLength(3);

    expect(row1Headers[0]).toHaveTextContent('A');
    expect(row1Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row1Headers[0]).toHaveAttribute('rowspan', '4');

    expect(row1Headers[1]).toHaveTextContent('B');
    expect(row1Headers[1]).toHaveAttribute('scope', 'rowgroup');
    expect(row1Headers[1]).toHaveAttribute('rowspan', '1');

    expect(row1Headers[2]).toHaveTextContent('E');
    expect(row1Headers[2]).toHaveAttribute('scope', 'row');
    expect(row1Headers[2]).toHaveAttribute('rowspan', '1');

    // Row 2
    const row2Headers = container.querySelectorAll('tbody tr:nth-child(2) th');
    expect(row2Headers).toHaveLength(2);

    expect(row2Headers[0]).toHaveTextContent('C');
    expect(row2Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row2Headers[0]).toHaveAttribute('rowspan', '2');

    expect(row2Headers[1]).toHaveTextContent('F');
    expect(row2Headers[1]).toHaveAttribute('scope', 'row');
    expect(row2Headers[1]).toHaveAttribute('rowspan', '1');

    // Row 3
    const row3Headers = container.querySelectorAll('tbody tr:nth-child(3) th');
    expect(row3Headers).toHaveLength(1);

    expect(row3Headers[0]).toHaveTextContent('G');
    expect(row3Headers[0]).toHaveAttribute('scope', 'row');
    expect(row3Headers[0]).toHaveAttribute('rowspan', '1');

    // Row 4
    const row4Headers = container.querySelectorAll('tbody tr:nth-child(4) th');
    expect(row4Headers).toHaveLength(2);

    expect(row4Headers[0]).toHaveTextContent('D');
    expect(row4Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row4Headers[0]).toHaveAttribute('rowspan', '1');

    expect(row4Headers[1]).toHaveTextContent('H');
    expect(row4Headers[1]).toHaveAttribute('scope', 'row');
    expect(row4Headers[1]).toHaveAttribute('rowspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with `rowgroup` header merged with identical parent', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rowHeaders={[
          new Header('A', 'A').addChild(
            new Header('B', 'B').addChild(new Header('C', 'C')),
          ),
          new Header('D', 'D').addChild(
            new Header('D', 'D').addChild(new Header('E', 'E')),
          ),
          new Header('F', 'F').addChild(
            new Header('G', 'G').addChild(new Header('H', 'H')),
          ),
        ]}
        rows={[
          ['ABC1', 'ABC2'],
          ['DDE1', 'DDE2'],
          ['FGH1', 'FGH2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(3);
    expect(container.querySelectorAll('tbody td')).toHaveLength(6);

    // Row 2
    const row2Headers = container.querySelectorAll('tbody tr:nth-child(2) th');
    expect(row2Headers).toHaveLength(2);

    expect(row2Headers[0]).toHaveTextContent('D');
    expect(row2Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row2Headers[0]).toHaveAttribute('rowspan', '1');
    expect(row2Headers[0]).toHaveAttribute('colspan', '2');

    expect(row2Headers[1]).toHaveTextContent('E');
    expect(row2Headers[1]).toHaveAttribute('scope', 'row');
    expect(row2Headers[1]).toHaveAttribute('rowspan', '1');
    expect(row2Headers[1]).toHaveAttribute('colspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with `rowgroup` header merged with multiple identical parents', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rowHeaders={[
          new Header('A', 'A').addChild(
            new Header('B', 'B').addChild(new Header('C', 'C')),
          ),
          new Header('D', 'D').addChild(
            new Header('D', 'D').addChild(new Header('D', 'D')),
          ),
          new Header('F', 'F').addChild(
            new Header('G', 'G').addChild(new Header('H', 'H')),
          ),
        ]}
        rows={[
          ['ABC1', 'ABC2'],
          ['DDD1', 'DDD2'],
          ['FGH1', 'FGH2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(3);
    expect(container.querySelectorAll('tbody td')).toHaveLength(6);

    // Row 2
    const row2Headers = container.querySelectorAll('tbody tr:nth-child(2) th');
    expect(row2Headers).toHaveLength(1);

    expect(row2Headers[0]).toHaveTextContent('D');
    expect(row2Headers[0]).toHaveAttribute('scope', 'row');
    expect(row2Headers[0]).toHaveAttribute('rowspan', '1');
    expect(row2Headers[0]).toHaveAttribute('colspan', '3');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with `rowgroup` header merged with identical parent on first row', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rowHeaders={[
          new Header('A', 'A').addChild(
            new Header('A', 'A').addChild(new Header('B', 'B')),
          ),
          new Header('C', 'C').addChild(
            new Header('D', 'D').addChild(new Header('E', 'E')),
          ),
          new Header('F', 'F').addChild(
            new Header('G', 'G').addChild(new Header('H', 'H')),
          ),
        ]}
        rows={[
          ['AAB1', 'AAB2'],
          ['CDE1', 'CDE2'],
          ['FGH1', 'FGH2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(3);
    expect(container.querySelectorAll('tbody td')).toHaveLength(6);

    // Row 1
    const row1Headers = container.querySelectorAll('tbody tr:nth-child(1) th');
    expect(row1Headers).toHaveLength(2);

    expect(row1Headers[0]).toHaveTextContent('A');
    expect(row1Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row1Headers[0]).toHaveAttribute('rowspan', '1');
    expect(row1Headers[0]).toHaveAttribute('colspan', '2');

    expect(row1Headers[1]).toHaveTextContent('B');
    expect(row1Headers[1]).toHaveAttribute('scope', 'row');
    expect(row1Headers[1]).toHaveAttribute('rowspan', '1');
    expect(row1Headers[1]).toHaveAttribute('colspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with `row` header merged with identical parent', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rowHeaders={[
          new Header('A', 'A')
            .addChild(new Header('B', 'B').addChild(new Header('E', 'E')))
            .addChild(new Header('C', 'C').addChild(new Header('C', 'C')))
            .addChild(new Header('D', 'D').addChild(new Header('F', 'F'))),
        ]}
        rows={[
          ['ABE1', 'ABE2'],
          ['ACC1', 'ACC2'],
          ['ADF1', 'ADF2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(3);
    expect(container.querySelectorAll('tbody td')).toHaveLength(6);

    // Row 2
    const row2Headers = container.querySelectorAll('tbody tr:nth-child(2) th');
    expect(row2Headers).toHaveLength(1);

    expect(row2Headers[0]).toHaveTextContent('C');
    expect(row2Headers[0]).toHaveAttribute('scope', 'row');
    expect(row2Headers[0]).toHaveAttribute('rowspan', '1');
    expect(row2Headers[0]).toHaveAttribute('colspan', '2');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with `row` header merged with identical parent on first row', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rowHeaders={[
          new Header('A', 'A')
            .addChild(new Header('B', 'B').addChild(new Header('B', 'B')))
            .addChild(new Header('C', 'C').addChild(new Header('D', 'D')))
            .addChild(new Header('E', 'E').addChild(new Header('F', 'F'))),
        ]}
        rows={[
          ['ABB1', 'ABB2'],
          ['ACD1', 'ACD2'],
          ['AEF1', 'AEF2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(3);
    expect(container.querySelectorAll('tbody td')).toHaveLength(6);

    // Row 1
    const row1Headers = container.querySelectorAll('tbody tr:nth-child(1) th');
    expect(row1Headers).toHaveLength(2);

    expect(row1Headers[0]).toHaveTextContent('A');
    expect(row1Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row1Headers[0]).toHaveAttribute('rowspan', '3');
    expect(row1Headers[0]).toHaveAttribute('colspan', '1');

    expect(row1Headers[1]).toHaveTextContent('B');
    expect(row1Headers[1]).toHaveAttribute('scope', 'row');
    expect(row1Headers[1]).toHaveAttribute('rowspan', '1');
    expect(row1Headers[1]).toHaveAttribute('colspan', '2');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with deeply nested rows and multiple identical headers', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rowHeaders={[
          new Header('A', 'A').addChild(
            new Header('A', 'A').addChild(
              new Header('A', 'A').addChild(new Header('A', 'A')),
            ),
          ),
          new Header('B', 'B').addChild(
            new Header('B', 'B')
              .addChild(new Header('B', 'B').addChild(new Header('B', 'B')))
              .addChild(new Header('C', 'C').addChild(new Header('D', 'D'))),
          ),
          new Header('E', 'E').addChild(
            new Header('F', 'F')
              .addChild(
                new Header('F', 'F')
                  .addChild(new Header('F', 'F'))
                  .addChild(new Header('G', 'G')),
              )
              .addChild(
                new Header('H', 'H')
                  .addChild(new Header('I', 'I'))
                  .addChild(new Header('J', 'J')),
              ),
          ),
        ]}
        rows={[
          ['AAAA1', 'AAAA2'],
          ['BBBB1', 'BBBB2'],
          ['BBBC1', 'BBBC2'],
          ['EFFF1', 'EFFF2'],
          ['EFFG1', 'EFFG2'],
          ['EFHI1', 'EFHI2'],
          ['EFHJ1', 'EFHJ2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(7);
    expect(container.querySelectorAll('tbody td')).toHaveLength(14);

    // Row 1
    const row1Headers = container.querySelectorAll('tbody tr:nth-child(1) th');
    expect(row1Headers).toHaveLength(1);

    expect(row1Headers[0]).toHaveTextContent('A');
    expect(row1Headers[0]).toHaveAttribute('scope', 'row');
    expect(row1Headers[0]).toHaveAttribute('rowspan', '1');
    expect(row1Headers[0]).toHaveAttribute('colspan', '4');

    // Row 2
    const row2Headers = container.querySelectorAll('tbody tr:nth-child(2) th');
    expect(row2Headers).toHaveLength(2);

    expect(row2Headers[0]).toHaveTextContent('B');
    expect(row2Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row2Headers[0]).toHaveAttribute('rowspan', '2');
    expect(row2Headers[0]).toHaveAttribute('colspan', '2');

    expect(row2Headers[1]).toHaveTextContent('B');
    expect(row2Headers[1]).toHaveAttribute('scope', 'row');
    expect(row2Headers[1]).toHaveAttribute('rowspan', '1');
    expect(row2Headers[1]).toHaveAttribute('colspan', '2');

    // Row 3
    const row3Headers = container.querySelectorAll('tbody tr:nth-child(3) th');
    expect(row3Headers).toHaveLength(2);

    expect(row3Headers[0]).toHaveTextContent('C');
    expect(row3Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row3Headers[0]).toHaveAttribute('rowspan', '1');
    expect(row3Headers[0]).toHaveAttribute('colspan', '1');

    expect(row3Headers[1]).toHaveTextContent('D');
    expect(row3Headers[1]).toHaveAttribute('scope', 'row');
    expect(row3Headers[1]).toHaveAttribute('rowspan', '1');
    expect(row3Headers[1]).toHaveAttribute('colspan', '1');

    // Row 4
    const row4Headers = container.querySelectorAll('tbody tr:nth-child(4) th');
    expect(row4Headers).toHaveLength(4);

    expect(row4Headers[0]).toHaveTextContent('E');
    expect(row4Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row4Headers[0]).toHaveAttribute('rowspan', '4');
    expect(row4Headers[0]).toHaveAttribute('colspan', '1');

    expect(row4Headers[1]).toHaveTextContent('F');
    expect(row4Headers[1]).toHaveAttribute('scope', 'rowgroup');
    expect(row4Headers[1]).toHaveAttribute('rowspan', '4');
    expect(row4Headers[1]).toHaveAttribute('colspan', '1');

    expect(row4Headers[2]).toHaveTextContent('F');
    expect(row4Headers[2]).toHaveAttribute('scope', 'rowgroup');
    expect(row4Headers[2]).toHaveAttribute('rowspan', '2');
    expect(row4Headers[2]).toHaveAttribute('colspan', '1');

    expect(row4Headers[3]).toHaveTextContent('F');
    expect(row4Headers[3]).toHaveAttribute('scope', 'row');
    expect(row4Headers[3]).toHaveAttribute('rowspan', '1');
    expect(row4Headers[3]).toHaveAttribute('colspan', '1');

    // Row 5
    const row5Headers = container.querySelectorAll('tbody tr:nth-child(5) th');
    expect(row5Headers).toHaveLength(1);

    expect(row5Headers[0]).toHaveTextContent('G');
    expect(row5Headers[0]).toHaveAttribute('scope', 'row');
    expect(row5Headers[0]).toHaveAttribute('rowspan', '1');
    expect(row5Headers[0]).toHaveAttribute('colspan', '1');

    // Row 6
    const row6Headers = container.querySelectorAll('tbody tr:nth-child(6) th');
    expect(row6Headers).toHaveLength(2);

    expect(row6Headers[0]).toHaveTextContent('H');
    expect(row6Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row6Headers[0]).toHaveAttribute('rowspan', '2');
    expect(row6Headers[0]).toHaveAttribute('colspan', '1');

    expect(row6Headers[1]).toHaveTextContent('I');
    expect(row6Headers[1]).toHaveAttribute('scope', 'row');
    expect(row6Headers[1]).toHaveAttribute('rowspan', '1');
    expect(row6Headers[1]).toHaveAttribute('colspan', '1');

    // Row 7
    const row7Headers = container.querySelectorAll('tbody tr:nth-child(7) th');
    expect(row7Headers).toHaveLength(1);

    expect(row7Headers[0]).toHaveTextContent('J');
    expect(row7Headers[0]).toHaveAttribute('scope', 'row');
    expect(row7Headers[0]).toHaveAttribute('rowspan', '1');
    expect(row7Headers[0]).toHaveAttribute('colspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with one `colgroup` header subgroup', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[
          new Header('B', 'B').addChild(
            new Header('A', 'A')
              .addChild(new Header('C', 'C'))
              .addChild(new Header('D', 'D')),
          ),
        ]}
        rowHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rows={[
          ['BAC1', 'BAC1'],
          ['BAD2', 'BAD2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(2);
    expect(container.querySelectorAll('tbody td')).toHaveLength(4);

    expect(container.querySelectorAll('thead tr')).toHaveLength(3);

    // Row 1
    const row1Headers = container.querySelectorAll('thead tr:nth-child(1) th');
    expect(row1Headers).toHaveLength(1);

    expect(row1Headers[0]).toHaveTextContent('B');
    expect(row1Headers[0]).toHaveAttribute('scope', 'colgroup');
    expect(row1Headers[0]).toHaveAttribute('colspan', '2');

    // Row 2
    const row2Headers = container.querySelectorAll('thead tr:nth-child(2) th');
    expect(row2Headers).toHaveLength(1);

    expect(row2Headers[0]).toHaveTextContent('A');
    expect(row2Headers[0]).toHaveAttribute('scope', 'colgroup');
    expect(row2Headers[0]).toHaveAttribute('colspan', '2');

    // Row 3
    const row3Headers = container.querySelectorAll('thead tr:nth-child(3) th');
    expect(row3Headers).toHaveLength(2);

    expect(row3Headers[0]).toHaveTextContent('C');
    expect(row3Headers[0]).toHaveAttribute('scope', 'col');
    expect(row3Headers[0]).toHaveAttribute('colspan', '1');

    expect(row3Headers[1]).toHaveTextContent('D');
    expect(row3Headers[1]).toHaveAttribute('scope', 'col');
    expect(row3Headers[1]).toHaveAttribute('colspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with two `colgroup` header subgroups', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[
          new Header('B', 'B').addChild(
            new Header('A', 'A').addChild(new Header('F', 'F')),
          ),
          new Header('C', 'C')
            .addChild(new Header('D', 'D').addChild(new Header('F', 'F')))
            .addChild(new Header('E', 'E').addChild(new Header('F', 'F'))),
        ]}
        rowHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rows={[
          ['BAF1', 'CDF1', 'CEF1'],
          ['BAF2', 'CDF2', 'CEF2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(2);
    expect(container.querySelectorAll('tbody td')).toHaveLength(6);

    expect(container.querySelectorAll('thead tr')).toHaveLength(3);

    // Row 1
    const row1Headers = container.querySelectorAll('thead tr:nth-child(1) th');
    expect(row1Headers).toHaveLength(2);

    expect(row1Headers[0]).toHaveTextContent('B');
    expect(row1Headers[0]).toHaveAttribute('scope', 'colgroup');
    expect(row1Headers[0]).toHaveAttribute('colspan', '1');

    expect(row1Headers[1]).toHaveTextContent('C');
    expect(row1Headers[1]).toHaveAttribute('scope', 'colgroup');
    expect(row1Headers[1]).toHaveAttribute('colspan', '2');

    // Row 2
    const row2Headers = container.querySelectorAll('thead tr:nth-child(2) th');
    expect(row2Headers).toHaveLength(3);

    expect(row2Headers[0]).toHaveTextContent('A');
    expect(row2Headers[0]).toHaveAttribute('scope', 'colgroup');
    expect(row2Headers[0]).toHaveAttribute('colspan', '1');

    expect(row2Headers[1]).toHaveTextContent('D');
    expect(row2Headers[1]).toHaveAttribute('scope', 'colgroup');
    expect(row2Headers[1]).toHaveAttribute('colspan', '1');

    expect(row2Headers[2]).toHaveTextContent('E');
    expect(row2Headers[2]).toHaveAttribute('scope', 'colgroup');
    expect(row2Headers[2]).toHaveAttribute('colspan', '1');

    // Row 3
    const row3Headers = container.querySelectorAll('thead tr:nth-child(3) th');
    expect(row3Headers).toHaveLength(3);

    expect(row3Headers[0]).toHaveTextContent('F');
    expect(row3Headers[0]).toHaveAttribute('scope', 'col');
    expect(row3Headers[0]).toHaveAttribute('colspan', '1');

    expect(row3Headers[1]).toHaveTextContent('F');
    expect(row3Headers[1]).toHaveAttribute('scope', 'col');
    expect(row3Headers[1]).toHaveAttribute('colspan', '1');

    expect(row3Headers[2]).toHaveTextContent('F');
    expect(row3Headers[2]).toHaveAttribute('scope', 'col');
    expect(row3Headers[2]).toHaveAttribute('colspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with three `colgroup` header subgroups', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[
          new Header('B', 'B').addChild(
            new Header('A', 'A').addChild(new Header('H', 'H')),
          ),
          new Header('C', 'C')
            .addChild(new Header('D', 'D').addChild(new Header('H', 'H')))
            .addChild(new Header('F', 'F').addChild(new Header('H', 'H'))),
          new Header('E', 'E').addChild(
            new Header('G', 'G').addChild(new Header('H', 'H')),
          ),
        ]}
        rowHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rows={[
          ['BAH1', 'CDH1', 'CFH1', 'EGH1'],
          ['BAH2', 'CDH2', 'CFH2', 'EGH2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(2);
    expect(container.querySelectorAll('tbody td')).toHaveLength(8);

    expect(container.querySelectorAll('thead tr')).toHaveLength(3);

    // Row 1
    const row1Headers = container.querySelectorAll('thead tr:nth-child(1) th');
    expect(row1Headers).toHaveLength(3);

    expect(row1Headers[0]).toHaveTextContent('B');
    expect(row1Headers[0]).toHaveAttribute('scope', 'colgroup');
    expect(row1Headers[0]).toHaveAttribute('colspan', '1');

    expect(row1Headers[1]).toHaveTextContent('C');
    expect(row1Headers[1]).toHaveAttribute('scope', 'colgroup');
    expect(row1Headers[1]).toHaveAttribute('colspan', '2');

    expect(row1Headers[2]).toHaveTextContent('E');
    expect(row1Headers[2]).toHaveAttribute('scope', 'colgroup');
    expect(row1Headers[2]).toHaveAttribute('colspan', '1');

    // Row 2
    const row2Headers = container.querySelectorAll('thead tr:nth-child(2) th');
    expect(row2Headers).toHaveLength(4);

    expect(row2Headers[0]).toHaveTextContent('A');
    expect(row2Headers[0]).toHaveAttribute('scope', 'colgroup');
    expect(row2Headers[0]).toHaveAttribute('colspan', '1');

    expect(row2Headers[1]).toHaveTextContent('D');
    expect(row2Headers[1]).toHaveAttribute('scope', 'colgroup');
    expect(row2Headers[1]).toHaveAttribute('colspan', '1');

    expect(row2Headers[2]).toHaveTextContent('F');
    expect(row2Headers[2]).toHaveAttribute('scope', 'colgroup');
    expect(row2Headers[2]).toHaveAttribute('colspan', '1');

    expect(row2Headers[3]).toHaveTextContent('G');
    expect(row2Headers[3]).toHaveAttribute('scope', 'colgroup');
    expect(row2Headers[3]).toHaveAttribute('colspan', '1');

    // Row 3
    const row3Headers = container.querySelectorAll('thead tr:nth-child(3) th');
    expect(row3Headers).toHaveLength(4);

    expect(row3Headers[0]).toHaveTextContent('H');
    expect(row3Headers[0]).toHaveAttribute('scope', 'col');
    expect(row3Headers[0]).toHaveAttribute('colspan', '1');

    expect(row3Headers[1]).toHaveTextContent('H');
    expect(row3Headers[1]).toHaveAttribute('scope', 'col');
    expect(row3Headers[1]).toHaveAttribute('colspan', '1');

    expect(row3Headers[2]).toHaveTextContent('H');
    expect(row3Headers[2]).toHaveAttribute('scope', 'col');
    expect(row3Headers[2]).toHaveAttribute('colspan', '1');

    expect(row3Headers[3]).toHaveTextContent('H');
    expect(row3Headers[3]).toHaveAttribute('scope', 'col');
    expect(row3Headers[3]).toHaveAttribute('colspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with `colgroup` header merged with identical parent', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[
          new Header('B', 'B').addChild(
            new Header('A', 'A').addChild(new Header('F', 'F')),
          ),
          new Header('C', 'C').addChild(
            new Header('C', 'C').addChild(new Header('F', 'F')),
          ),
          new Header('D', 'D').addChild(
            new Header('E', 'E').addChild(new Header('F', 'F')),
          ),
        ]}
        rowHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rows={[
          ['BAF1', 'CCF1', 'DEF1'],
          ['BAF2', 'CCF2', 'DEF2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(2);
    expect(container.querySelectorAll('tbody td')).toHaveLength(6);

    expect(container.querySelectorAll('thead tr')).toHaveLength(3);

    // Row 1
    const row1Headers = container.querySelectorAll('thead tr:nth-child(1) th');
    expect(row1Headers).toHaveLength(3);

    expect(row1Headers[0]).toHaveTextContent('B');
    expect(row1Headers[0]).toHaveAttribute('scope', 'colgroup');
    expect(row1Headers[0]).toHaveAttribute('rowspan', '1');
    expect(row1Headers[0]).toHaveAttribute('colspan', '1');

    expect(row1Headers[1]).toHaveTextContent('C');
    expect(row1Headers[1]).toHaveAttribute('scope', 'colgroup');
    expect(row1Headers[1]).toHaveAttribute('rowspan', '2');
    expect(row1Headers[1]).toHaveAttribute('colspan', '1');

    expect(row1Headers[2]).toHaveTextContent('D');
    expect(row1Headers[2]).toHaveAttribute('scope', 'colgroup');
    expect(row1Headers[2]).toHaveAttribute('rowspan', '1');
    expect(row1Headers[2]).toHaveAttribute('colspan', '1');

    // Row 2
    const row2Headers = container.querySelectorAll('thead tr:nth-child(2) th');
    expect(row2Headers).toHaveLength(2);

    expect(row2Headers[0]).toHaveTextContent('A');
    expect(row2Headers[0]).toHaveAttribute('scope', 'colgroup');
    expect(row2Headers[0]).toHaveAttribute('rowspan', '1');
    expect(row2Headers[0]).toHaveAttribute('colspan', '1');

    expect(row2Headers[1]).toHaveTextContent('E');
    expect(row2Headers[1]).toHaveAttribute('scope', 'colgroup');
    expect(row2Headers[1]).toHaveAttribute('rowspan', '1');
    expect(row2Headers[1]).toHaveAttribute('colspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with `colgroup` header merged with multiple identical parents', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[
          new Header('B', 'B').addChild(
            new Header('A', 'A').addChild(new Header('F', 'F')),
          ),
          new Header('C', 'C').addChild(
            new Header('C', 'C').addChild(new Header('C', 'C')),
          ),
          new Header('D', 'D').addChild(
            new Header('E', 'E').addChild(new Header('F', 'F')),
          ),
        ]}
        rowHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rows={[
          ['BAF1', 'CCC1', 'DEF1'],
          ['BAF2', 'CCC2', 'DEF2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(2);
    expect(container.querySelectorAll('tbody td')).toHaveLength(6);

    expect(container.querySelectorAll('thead tr')).toHaveLength(3);

    // Row 1
    const row1Headers = container.querySelectorAll('thead tr:nth-child(1) th');
    expect(row1Headers).toHaveLength(3);

    expect(row1Headers[0]).toHaveTextContent('B');
    expect(row1Headers[0]).toHaveAttribute('scope', 'colgroup');
    expect(row1Headers[0]).toHaveAttribute('rowspan', '1');
    expect(row1Headers[0]).toHaveAttribute('colspan', '1');

    expect(row1Headers[1]).toHaveTextContent('C');
    expect(row1Headers[1]).toHaveAttribute('scope', 'col');
    expect(row1Headers[1]).toHaveAttribute('rowspan', '3');
    expect(row1Headers[1]).toHaveAttribute('colspan', '1');

    expect(row1Headers[2]).toHaveTextContent('D');
    expect(row1Headers[2]).toHaveAttribute('scope', 'colgroup');
    expect(row1Headers[2]).toHaveAttribute('rowspan', '1');
    expect(row1Headers[2]).toHaveAttribute('colspan', '1');

    // Row 2
    const row2Headers = container.querySelectorAll('thead tr:nth-child(2) th');
    expect(row2Headers).toHaveLength(2);

    expect(row2Headers[0]).toHaveTextContent('A');
    expect(row2Headers[0]).toHaveAttribute('scope', 'colgroup');
    expect(row2Headers[0]).toHaveAttribute('rowspan', '1');
    expect(row2Headers[0]).toHaveAttribute('colspan', '1');

    expect(row2Headers[1]).toHaveTextContent('E');
    expect(row2Headers[1]).toHaveAttribute('scope', 'colgroup');
    expect(row2Headers[1]).toHaveAttribute('rowspan', '1');
    expect(row2Headers[1]).toHaveAttribute('colspan', '1');

    // Row 3
    const row3Headers = container.querySelectorAll('thead tr:nth-child(3) th');
    expect(row3Headers).toHaveLength(2);

    expect(row3Headers[0]).toHaveTextContent('F');
    expect(row3Headers[0]).toHaveAttribute('scope', 'col');
    expect(row3Headers[0]).toHaveAttribute('rowspan', '1');
    expect(row3Headers[0]).toHaveAttribute('colspan', '1');

    expect(row3Headers[1]).toHaveTextContent('F');
    expect(row3Headers[1]).toHaveAttribute('scope', 'col');
    expect(row3Headers[1]).toHaveAttribute('rowspan', '1');
    expect(row3Headers[1]).toHaveAttribute('colspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with `colgroup` header merged with identical parent on first column', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[
          new Header('A', 'A').addChild(
            new Header('A', 'A').addChild(new Header('F', 'F')),
          ),
          new Header('B', 'B').addChild(
            new Header('C', 'C').addChild(new Header('F', 'F')),
          ),
          new Header('D', 'D').addChild(
            new Header('E', 'E').addChild(new Header('F', 'F')),
          ),
        ]}
        rowHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rows={[
          ['BAF1', 'CCF1', 'DEF1'],
          ['BAF2', 'CCF2', 'DEF2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(2);
    expect(container.querySelectorAll('tbody td')).toHaveLength(6);

    expect(container.querySelectorAll('thead tr')).toHaveLength(3);

    // Row 1
    const row1Headers = container.querySelectorAll('thead tr:nth-child(1) th');
    expect(row1Headers).toHaveLength(3);

    expect(row1Headers[0]).toHaveTextContent('A');
    expect(row1Headers[0]).toHaveAttribute('scope', 'colgroup');
    expect(row1Headers[0]).toHaveAttribute('rowspan', '2');
    expect(row1Headers[0]).toHaveAttribute('colspan', '1');

    expect(row1Headers[1]).toHaveTextContent('B');
    expect(row1Headers[1]).toHaveAttribute('scope', 'colgroup');
    expect(row1Headers[1]).toHaveAttribute('rowspan', '1');
    expect(row1Headers[1]).toHaveAttribute('colspan', '1');

    expect(row1Headers[2]).toHaveTextContent('D');
    expect(row1Headers[2]).toHaveAttribute('scope', 'colgroup');
    expect(row1Headers[2]).toHaveAttribute('rowspan', '1');
    expect(row1Headers[2]).toHaveAttribute('colspan', '1');

    // Row 2
    const row2Headers = container.querySelectorAll('thead tr:nth-child(2) th');
    expect(row2Headers).toHaveLength(2);

    expect(row2Headers[0]).toHaveTextContent('C');
    expect(row2Headers[0]).toHaveAttribute('scope', 'colgroup');
    expect(row2Headers[0]).toHaveAttribute('rowspan', '1');
    expect(row2Headers[0]).toHaveAttribute('colspan', '1');

    expect(row2Headers[1]).toHaveTextContent('E');
    expect(row2Headers[1]).toHaveAttribute('scope', 'colgroup');
    expect(row2Headers[1]).toHaveAttribute('rowspan', '1');
    expect(row2Headers[1]).toHaveAttribute('colspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with `col` header merged with identical parent', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[
          new Header('A', 'A').addChild(
            new Header('B', 'B').addChild(new Header('C', 'C')),
          ),
          new Header('D', 'D').addChild(
            new Header('E', 'E').addChild(new Header('E', 'E')),
          ),
          new Header('F', 'F').addChild(
            new Header('G', 'G').addChild(new Header('H', 'H')),
          ),
        ]}
        rowHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rows={[
          ['ABC1', 'DEE1', 'FGH1'],
          ['ABC2', 'DEE2', 'FGH2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(2);
    expect(container.querySelectorAll('tbody td')).toHaveLength(6);

    expect(container.querySelectorAll('thead tr')).toHaveLength(3);

    // Row 2
    const row1Headers = container.querySelectorAll('thead tr:nth-child(2) th');
    expect(row1Headers).toHaveLength(3);

    expect(row1Headers[0]).toHaveTextContent('B');
    expect(row1Headers[0]).toHaveAttribute('scope', 'colgroup');
    expect(row1Headers[0]).toHaveAttribute('rowspan', '1');
    expect(row1Headers[0]).toHaveAttribute('colspan', '1');

    expect(row1Headers[1]).toHaveTextContent('E');
    expect(row1Headers[1]).toHaveAttribute('scope', 'col');
    expect(row1Headers[1]).toHaveAttribute('rowspan', '2');
    expect(row1Headers[1]).toHaveAttribute('colspan', '1');

    expect(row1Headers[2]).toHaveTextContent('G');
    expect(row1Headers[2]).toHaveAttribute('scope', 'colgroup');
    expect(row1Headers[2]).toHaveAttribute('rowspan', '1');
    expect(row1Headers[2]).toHaveAttribute('colspan', '1');

    // Row 3
    const row3Headers = container.querySelectorAll('thead tr:nth-child(3) th');
    expect(row3Headers).toHaveLength(2);

    expect(row3Headers[0]).toHaveTextContent('C');
    expect(row3Headers[0]).toHaveAttribute('scope', 'col');
    expect(row3Headers[0]).toHaveAttribute('rowspan', '1');
    expect(row3Headers[0]).toHaveAttribute('colspan', '1');

    expect(row3Headers[1]).toHaveTextContent('H');
    expect(row3Headers[1]).toHaveAttribute('scope', 'col');
    expect(row3Headers[1]).toHaveAttribute('rowspan', '1');
    expect(row3Headers[1]).toHaveAttribute('colspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with `col` header merged with identical parent on first column', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[
          new Header('A', 'A').addChild(
            new Header('B', 'B').addChild(new Header('B', 'B')),
          ),
          new Header('C', 'C').addChild(
            new Header('D', 'D').addChild(new Header('E', 'E')),
          ),
          new Header('F', 'F').addChild(
            new Header('G', 'G').addChild(new Header('H', 'H')),
          ),
        ]}
        rowHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rows={[
          ['ABB1', 'CDE1', 'FGH1'],
          ['ABB2', 'CDE2', 'FGH2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(2);
    expect(container.querySelectorAll('tbody td')).toHaveLength(6);

    expect(container.querySelectorAll('thead tr')).toHaveLength(3);

    // Row 2
    const row1Headers = container.querySelectorAll('thead tr:nth-child(2) th');
    expect(row1Headers).toHaveLength(3);

    expect(row1Headers[0]).toHaveTextContent('B');
    expect(row1Headers[0]).toHaveAttribute('scope', 'col');
    expect(row1Headers[0]).toHaveAttribute('rowspan', '2');
    expect(row1Headers[0]).toHaveAttribute('colspan', '1');

    expect(row1Headers[1]).toHaveTextContent('D');
    expect(row1Headers[1]).toHaveAttribute('scope', 'colgroup');
    expect(row1Headers[1]).toHaveAttribute('rowspan', '1');
    expect(row1Headers[1]).toHaveAttribute('colspan', '1');

    expect(row1Headers[2]).toHaveTextContent('G');
    expect(row1Headers[2]).toHaveAttribute('scope', 'colgroup');
    expect(row1Headers[2]).toHaveAttribute('rowspan', '1');
    expect(row1Headers[2]).toHaveAttribute('colspan', '1');

    // Row 3
    const row3Headers = container.querySelectorAll('thead tr:nth-child(3) th');
    expect(row3Headers).toHaveLength(2);

    expect(row3Headers[0]).toHaveTextContent('E');
    expect(row3Headers[0]).toHaveAttribute('scope', 'col');
    expect(row3Headers[0]).toHaveAttribute('rowspan', '1');
    expect(row3Headers[0]).toHaveAttribute('colspan', '1');

    expect(row3Headers[1]).toHaveTextContent('H');
    expect(row3Headers[1]).toHaveAttribute('scope', 'col');
    expect(row3Headers[1]).toHaveAttribute('rowspan', '1');
    expect(row3Headers[1]).toHaveAttribute('colspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with multi-span `colgroup` merged with its identical groups', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[
          new Header('B', 'B').addChild(
            new Header('A', 'A').addChild(new Header('F', 'F')),
          ),
          new Header('C', 'C').addChild(
            new Header('C', 'C')
              .addChild(new Header('F', 'F'))
              .addChild(new Header('F', 'F')),
          ),
        ]}
        rowHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rows={[
          ['BAF1', 'CCF1', 'CCF1'],
          ['BAF2', 'CCF2', 'CCF2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(2);
    expect(container.querySelectorAll('tbody td')).toHaveLength(6);

    expect(container.querySelectorAll('thead tr')).toHaveLength(3);

    // Row 1
    const row1Headers = container.querySelectorAll('thead tr:nth-child(1) th');
    expect(row1Headers).toHaveLength(2);

    expect(row1Headers[0]).toHaveTextContent('B');
    expect(row1Headers[0]).toHaveAttribute('scope', 'colgroup');
    expect(row1Headers[0]).toHaveAttribute('rowspan', '1');
    expect(row1Headers[0]).toHaveAttribute('colspan', '1');

    expect(row1Headers[1]).toHaveTextContent('C');
    expect(row1Headers[1]).toHaveAttribute('scope', 'colgroup');
    expect(row1Headers[1]).toHaveAttribute('rowspan', '2');
    expect(row1Headers[1]).toHaveAttribute('colspan', '2');

    // Row 2
    const row2Headers = container.querySelectorAll('thead tr:nth-child(2) th');
    expect(row2Headers).toHaveLength(1);

    expect(row2Headers[0]).toHaveTextContent('A');
    expect(row2Headers[0]).toHaveAttribute('scope', 'colgroup');
    expect(row2Headers[0]).toHaveAttribute('rowspan', '1');
    expect(row2Headers[0]).toHaveAttribute('colspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('does not render `colgroup` headers with multi-span subgroup with invalid rowspans and colspans', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[
          new Header('B', 'B').addChild(
            new Header('A', 'A').addChild(new Header('E', 'E')),
          ),
          new Header('C', 'C')
            .addChild(
              new Header('C', 'C')
                .addChild(new Header('E', 'E'))
                .addChild(new Header('E', 'E')),
            )
            .addChild(new Header('D', 'D').addChild(new Header('E', 'E'))),
        ]}
        rowHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rows={[
          ['BAE1', 'CCE1', 'CCE1', 'CDE1'],
          ['BAE2', 'CCE2', 'CCE2', 'CDE2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(2);
    expect(container.querySelectorAll('tbody td')).toHaveLength(8);

    expect(container.querySelectorAll('thead tr')).toHaveLength(3);

    // Row 1
    const row1Headers = container.querySelectorAll('thead tr:nth-child(1) th');
    expect(row1Headers).toHaveLength(2);

    expect(row1Headers[0]).toHaveTextContent('B');
    expect(row1Headers[0]).toHaveAttribute('scope', 'colgroup');
    expect(row1Headers[0]).toHaveAttribute('rowspan', '1');
    expect(row1Headers[0]).toHaveAttribute('colspan', '1');

    expect(row1Headers[1]).toHaveTextContent('C');
    expect(row1Headers[1]).toHaveAttribute('scope', 'colgroup');
    expect(row1Headers[1]).toHaveAttribute('rowspan', '1');
    expect(row1Headers[1]).toHaveAttribute('colspan', '3');

    // Row 2
    const row2Headers = container.querySelectorAll('thead tr:nth-child(2) th');
    expect(row2Headers).toHaveLength(3);

    expect(row2Headers[0]).toHaveTextContent('A');
    expect(row2Headers[0]).toHaveAttribute('scope', 'colgroup');
    expect(row2Headers[0]).toHaveAttribute('rowspan', '1');
    expect(row2Headers[0]).toHaveAttribute('colspan', '1');

    expect(row2Headers[1]).toHaveTextContent('C');
    expect(row2Headers[1]).toHaveAttribute('scope', 'colgroup');
    expect(row2Headers[1]).toHaveAttribute('rowspan', '1');
    expect(row2Headers[1]).toHaveAttribute('colspan', '2');

    expect(row2Headers[2]).toHaveTextContent('D');
    expect(row2Headers[2]).toHaveAttribute('scope', 'colgroup');
    expect(row2Headers[2]).toHaveAttribute('rowspan', '1');
    expect(row2Headers[2]).toHaveAttribute('colspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with one `col` header subgroup', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[
          new Header('A', 'A').addChild(
            new Header('B', 'B')
              .addChild(new Header('C', 'C'))
              .addChild(new Header('D', 'D')),
          ),
        ]}
        rowHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rows={[
          ['ABC1', 'ABC2'],
          ['ABD1', 'ABD2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(2);
    expect(container.querySelectorAll('tbody td')).toHaveLength(4);

    // Row 1
    const row1Headers = container.querySelectorAll('thead tr:nth-child(1) th');
    expect(row1Headers).toHaveLength(1);

    expect(row1Headers[0]).toHaveTextContent('A');
    expect(row1Headers[0]).toHaveAttribute('scope', 'colgroup');
    expect(row1Headers[0]).toHaveAttribute('colspan', '2');

    // Row 2
    const row2Headers = container.querySelectorAll('thead tr:nth-child(2) th');
    expect(row2Headers).toHaveLength(1);

    expect(row2Headers[0]).toHaveTextContent('B');
    expect(row2Headers[0]).toHaveAttribute('scope', 'colgroup');
    expect(row2Headers[0]).toHaveAttribute('colspan', '2');

    // Row 3
    const row3Headers = container.querySelectorAll('thead tr:nth-child(3) th');
    expect(row3Headers).toHaveLength(2);

    expect(row3Headers[0]).toHaveTextContent('C');
    expect(row3Headers[0]).toHaveAttribute('scope', 'col');
    expect(row3Headers[0]).toHaveAttribute('colspan', '1');

    expect(row3Headers[1]).toHaveTextContent('D');
    expect(row3Headers[1]).toHaveAttribute('scope', 'col');
    expect(row3Headers[1]).toHaveAttribute('colspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with two `col` header subgroups', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[
          new Header('A', 'A')
            .addChild(new Header('B', 'B').addChild(new Header('D', 'D')))
            .addChild(
              new Header('C', 'C')
                .addChild(new Header('E', 'E'))
                .addChild(new Header('F', 'F')),
            ),
        ]}
        rowHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rows={[
          ['ABD1', 'ABE1', 'ABF1'],
          ['ABD2', 'ABE2', 'ABF2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(2);
    expect(container.querySelectorAll('tbody td')).toHaveLength(6);

    expect(container.querySelectorAll('thead tr')).toHaveLength(3);

    // Row 1
    const row1Headers = container.querySelectorAll('thead tr:nth-child(1) th');
    expect(row1Headers).toHaveLength(1);

    expect(row1Headers[0]).toHaveTextContent('A');
    expect(row1Headers[0]).toHaveAttribute('scope', 'colgroup');
    expect(row1Headers[0]).toHaveAttribute('colspan', '3');

    // Row 2
    const row2Headers = container.querySelectorAll('thead tr:nth-child(2) th');
    expect(row2Headers).toHaveLength(2);

    expect(row2Headers[0]).toHaveTextContent('B');
    expect(row2Headers[0]).toHaveAttribute('scope', 'colgroup');
    expect(row2Headers[0]).toHaveAttribute('colspan', '1');

    expect(row2Headers[1]).toHaveTextContent('C');
    expect(row2Headers[1]).toHaveAttribute('scope', 'colgroup');
    expect(row2Headers[1]).toHaveAttribute('colspan', '2');

    // Row 3
    const row3Headers = container.querySelectorAll('thead tr:nth-child(3) th');
    expect(row3Headers).toHaveLength(3);

    expect(row3Headers[0]).toHaveTextContent('D');
    expect(row3Headers[0]).toHaveAttribute('scope', 'col');
    expect(row3Headers[0]).toHaveAttribute('colspan', '1');

    expect(row3Headers[1]).toHaveTextContent('E');
    expect(row3Headers[1]).toHaveAttribute('scope', 'col');
    expect(row3Headers[1]).toHaveAttribute('colspan', '1');

    expect(row3Headers[2]).toHaveTextContent('F');
    expect(row3Headers[2]).toHaveAttribute('scope', 'col');
    expect(row3Headers[2]).toHaveAttribute('colspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with three `col` header subgroups', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[
          new Header('A', 'A')
            .addChild(new Header('B', 'B').addChild(new Header('E', 'E')))
            .addChild(
              new Header('C', 'C')
                .addChild(new Header('F', 'F'))
                .addChild(new Header('G', 'G')),
            )
            .addChild(new Header('D', 'D').addChild(new Header('H', 'H'))),
        ]}
        rowHeaders={[new Header('1', '1'), new Header('2', '2')]}
        rows={[
          ['ABE1', 'ACF1', 'ACG1', 'ADH1'],
          ['ABE2', 'ACF2', 'ACG2', 'ADH2'],
        ]}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(2);
    expect(container.querySelectorAll('tbody td')).toHaveLength(8);

    expect(container.querySelectorAll('thead tr')).toHaveLength(3);

    // Row 1
    const row1Headers = container.querySelectorAll('thead tr:nth-child(1) th');
    expect(row1Headers).toHaveLength(1);

    expect(row1Headers[0]).toHaveTextContent('A');
    expect(row1Headers[0]).toHaveAttribute('scope', 'colgroup');
    expect(row1Headers[0]).toHaveAttribute('colspan', '4');

    // Row 2
    const row2Headers = container.querySelectorAll('thead tr:nth-child(2) th');
    expect(row2Headers).toHaveLength(3);

    expect(row2Headers[0]).toHaveTextContent('B');
    expect(row2Headers[0]).toHaveAttribute('scope', 'colgroup');
    expect(row2Headers[0]).toHaveAttribute('colspan', '1');

    expect(row2Headers[1]).toHaveTextContent('C');
    expect(row2Headers[1]).toHaveAttribute('scope', 'colgroup');
    expect(row2Headers[1]).toHaveAttribute('colspan', '2');

    expect(row2Headers[2]).toHaveTextContent('D');
    expect(row2Headers[2]).toHaveAttribute('scope', 'colgroup');
    expect(row2Headers[2]).toHaveAttribute('colspan', '1');

    // Row 3
    const row3Headers = container.querySelectorAll('thead tr:nth-child(3) th');
    expect(row3Headers).toHaveLength(4);

    expect(row3Headers[0]).toHaveTextContent('E');
    expect(row3Headers[0]).toHaveAttribute('scope', 'col');
    expect(row3Headers[0]).toHaveAttribute('colspan', '1');

    expect(row3Headers[1]).toHaveTextContent('F');
    expect(row3Headers[1]).toHaveAttribute('scope', 'col');
    expect(row3Headers[1]).toHaveAttribute('colspan', '1');

    expect(row3Headers[2]).toHaveTextContent('G');
    expect(row3Headers[2]).toHaveAttribute('scope', 'col');
    expect(row3Headers[2]).toHaveAttribute('colspan', '1');

    expect(row3Headers[3]).toHaveTextContent('H');
    expect(row3Headers[3]).toHaveAttribute('scope', 'col');
    expect(row3Headers[3]).toHaveAttribute('colspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });
});
