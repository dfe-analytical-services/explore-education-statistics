import React from 'react';
import { render } from 'react-testing-library';
import MultiHeaderTable from '../MultiHeaderTable';

describe('MultiHeaderTable', () => {
  test('renders 2x2 table correctly', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[
          { headers: [{ text: 'A' }, { text: 'B' }] },
          { headers: [{ text: 'C' }, { text: 'D' }] },
        ]}
        rowHeaders={[
          { headers: [{ text: '1' }, { text: '2' }] },
          { headers: [{ text: '3' }, { text: '4' }] },
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
          { headers: [{ text: 'A' }, { text: 'B' }] },
          { headers: [{ text: 'C' }, { text: 'D' }] },
          { headers: [{ text: 'E' }, { text: 'F' }] },
        ]}
        rowHeaders={[
          { headers: [{ text: '1' }, { text: '2' }] },
          { headers: [{ text: '3' }, { text: '4' }] },
          { headers: [{ text: '5' }, { text: '6' }] },
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

  test('renders table with single row header group', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[
          { headers: [{ text: 'A' }, { text: 'B' }] },
          { headers: [{ text: 'C' }, { text: 'D' }] },
        ]}
        rowHeaders={[
          { headers: [{ text: '1' }] },
          {
            groups: [{ text: '2' }],
            headers: [{ text: '3' }, { text: '4' }],
          },
        ]}
        rows={[
          ['AC13', 'AD13', 'BC13', 'BD13'],
          ['AC14', 'AD14', 'BC14', 'BD14'],
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

    // Body
    expect(container.querySelectorAll('tbody tr')).toHaveLength(2);
    expect(container.querySelectorAll('tbody td')).toHaveLength(8);

    // Row 1
    expect(container.querySelectorAll('tbody tr:nth-child(1) th')).toHaveLength(
      3,
    );
    expect(
      container.querySelector(
        'tbody tr:nth-child(1) th[scope="rowgroup"]:nth-child(1)',
      ),
    ).toHaveAttribute('rowspan', '2');
    expect(
      container.querySelector(
        'tbody tr:nth-child(1) th[scope="rowgroup"]:nth-child(2)',
      ),
    ).toHaveAttribute('rowspan', '2');
    expect(
      container.querySelector(
        'tbody tr:nth-child(1) th[scope="row"]:nth-child(3)',
      ),
    ).toHaveAttribute('rowspan', '1');

    // Row 2
    expect(
      container.querySelectorAll('tbody tr:nth-child(2) th[scope="row"]'),
    ).toHaveLength(1);
    expect(
      container.querySelector('tbody tr:nth-child(2) th:nth-child(1)'),
    ).toHaveAttribute('rowspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with single row header group at start', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[
          { headers: [{ text: 'A' }, { text: 'B' }] },
          { headers: [{ text: 'C' }, { text: 'D' }] },
        ]}
        rowHeaders={[
          {
            groups: [{ text: '1' }],
            headers: [{ text: '2' }],
          },
          {
            headers: [{ text: '3' }, { text: '4' }],
          },
        ]}
        rows={[
          ['AC23', 'AD23', 'BC23', 'BD23'],
          ['AC24', 'AD24', 'BC24', 'BD24'],
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

    expect(container.querySelectorAll('tbody tr')).toHaveLength(2);
    expect(container.querySelectorAll('tbody td')).toHaveLength(8);

    // Row 1
    expect(container.querySelectorAll('tbody tr:nth-child(1) th')).toHaveLength(
      3,
    );
    expect(
      container.querySelector(
        'tbody tr:nth-child(1) th[scope="rowgroup"]:nth-child(1)',
      ),
    ).toHaveAttribute('rowspan', '2');
    expect(
      container.querySelector(
        'tbody tr:nth-child(1) th[scope="rowgroup"]:nth-child(2)',
      ),
    ).toHaveAttribute('rowspan', '2');
    expect(
      container.querySelector(
        'tbody tr:nth-child(1) th[scope="row"]:nth-child(3)',
      ),
    ).toHaveAttribute('rowspan', '1');

    // Row 2
    expect(container.querySelectorAll('tbody tr:nth-child(2) th')).toHaveLength(
      1,
    );
    expect(
      container.querySelector(
        'tbody tr:nth-child(2) th[scope="row"]:nth-child(1)',
      ),
    ).toHaveAttribute('rowspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with two row header groups', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[
          { headers: [{ text: 'A' }, { text: 'B' }] },
          { headers: [{ text: 'C' }, { text: 'D' }] },
        ]}
        rowHeaders={[
          { headers: [{ text: '1' }] },
          {
            groups: [{ text: '2' }, { text: '3', span: 2 }],
            headers: [{ text: '4' }, { text: '5' }, { text: '6' }],
          },
        ]}
        rows={[
          ['AC14', 'AD14', 'BC14', 'BD14'],
          ['AC15', 'AD15', 'BC15', 'BD15'],
          ['AC16', 'AD16', 'BC16', 'BD16'],
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

    expect(container.querySelectorAll('tbody tr')).toHaveLength(3);
    expect(container.querySelectorAll('tbody td')).toHaveLength(12);

    // Row 1
    expect(container.querySelectorAll('tbody tr:nth-child(1) th')).toHaveLength(
      3,
    );
    expect(
      container.querySelector(
        'tbody tr:nth-child(1) th[scope="rowgroup"]:nth-child(1)',
      ),
    ).toHaveAttribute('rowspan', '3');
    expect(
      container.querySelector(
        'tbody tr:nth-child(1) th[scope="rowgroup"]:nth-child(2)',
      ),
    ).toHaveAttribute('rowspan', '1');
    expect(
      container.querySelector(
        'tbody tr:nth-child(1) th[scope="row"]:nth-child(3)',
      ),
    ).toHaveAttribute('rowspan', '1');

    // Row 2
    expect(container.querySelectorAll('tbody tr:nth-child(2) th')).toHaveLength(
      2,
    );
    expect(
      container.querySelector(
        'tbody tr:nth-child(2) th[scope="rowgroup"]:nth-child(1)',
      ),
    ).toHaveAttribute('rowspan', '2');
    expect(
      container.querySelector(
        'tbody tr:nth-child(2) th[scope="row"]:nth-child(2)',
      ),
    ).toHaveAttribute('rowspan', '1');

    // Row 3
    expect(container.querySelectorAll('tbody tr:nth-child(3) th')).toHaveLength(
      1,
    );
    expect(
      container.querySelector(
        'tbody tr:nth-child(3) th[scope="row"]:nth-child(1)',
      ),
    ).toHaveAttribute('rowspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with two row header groups with 4 row headers', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[
          { headers: [{ text: 'A' }, { text: 'B' }] },
          { headers: [{ text: 'C' }, { text: 'D' }] },
        ]}
        rowHeaders={[
          { headers: [{ text: '1' }] },
          {
            groups: [{ text: '2' }, { text: '3', span: 3 }],
            headers: [
              { text: '4' },
              { text: '5' },
              { text: '6' },
              { text: '7' },
            ],
          },
          { headers: [{ text: '8' }] },
        ]}
        rows={[
          ['AC148', 'AD148', 'BC148', 'BD148'],
          ['AC158', 'AD158', 'BC158', 'BD158'],
          ['AC168', 'AD168', 'BC168', 'BD168'],
          ['AC178', 'AD178', 'BC178', 'BD168'],
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
    expect(container.querySelectorAll('tbody td')).toHaveLength(16);

    expect(
      container.querySelectorAll('tbody tr:nth-child(1) th[rowspan="2"]'),
    ).toBeDefined();

    // Row 1
    expect(container.querySelectorAll('tbody tr:nth-child(1) th')).toHaveLength(
      4,
    );
    expect(
      container.querySelector(
        'tbody tr:nth-child(1) th[scope="rowgroup"]:nth-child(1)',
      ),
    ).toHaveAttribute('rowspan', '4');
    expect(
      container.querySelector(
        'tbody tr:nth-child(1) th[scope="rowgroup"]:nth-child(2)',
      ),
    ).toHaveAttribute('rowspan', '1');
    expect(
      container.querySelector(
        'tbody tr:nth-child(1) th[scope="rowgroup"]:nth-child(3)',
      ),
    ).toHaveAttribute('rowspan', '1');
    expect(
      container.querySelector(
        'tbody tr:nth-child(1) th[scope="row"]:nth-child(4)',
      ),
    ).toHaveAttribute('rowspan', '1');

    // Row 2
    expect(container.querySelectorAll('tbody tr:nth-child(2) th')).toHaveLength(
      3,
    );
    expect(
      container.querySelector(
        'tbody tr:nth-child(2) th[scope="rowgroup"]:nth-child(1)',
      ),
    ).toHaveAttribute('rowspan', '3');
    expect(
      container.querySelector(
        'tbody tr:nth-child(2) th[scope="rowgroup"]:nth-child(2)',
      ),
    ).toHaveAttribute('rowspan', '1');
    expect(
      container.querySelector(
        'tbody tr:nth-child(2) th[scope="row"]:nth-child(3)',
      ),
    ).toHaveAttribute('rowspan', '1');

    // Row 3
    expect(container.querySelectorAll('tbody tr:nth-child(3) th')).toHaveLength(
      2,
    );
    expect(
      container.querySelector(
        'tbody tr:nth-child(3) th[scope="rowgroup"]:nth-child(1)',
      ),
    ).toHaveAttribute('rowspan', '1');
    expect(
      container.querySelector(
        'tbody tr:nth-child(3) th[scope="row"]:nth-child(2)',
      ),
    ).toHaveAttribute('rowspan', '1');

    // Row 4
    expect(container.querySelectorAll('tbody tr:nth-child(4) th')).toHaveLength(
      2,
    );
    expect(
      container.querySelector(
        'tbody tr:nth-child(4) th[scope="rowgroup"]:nth-child(1)',
      ),
    ).toHaveAttribute('rowspan', '1');
    expect(
      container.querySelector(
        'tbody tr:nth-child(4) th[scope="row"]:nth-child(2)',
      ),
    ).toHaveAttribute('rowspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with two row header groups at start', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[
          { headers: [{ text: 'A' }, { text: 'B' }] },
          { headers: [{ text: 'C' }, { text: 'D' }] },
        ]}
        rowHeaders={[
          {
            groups: [{ text: '1' }, { text: '2', span: 2 }],
            headers: [{ text: '3' }, { text: '4' }, { text: '5' }],
          },
          {
            headers: [{ text: '6' }],
          },
        ]}
        rows={[
          ['AC36', 'AD36', 'BC36', 'BD36'],
          ['AC46', 'AD46', 'BC46', 'BD46'],
          ['AC56', 'AD56', 'BC56', 'BD56'],
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

    expect(container.querySelectorAll('tbody tr')).toHaveLength(3);
    expect(container.querySelectorAll('tbody td')).toHaveLength(12);

    // Row 1
    expect(container.querySelectorAll('tbody tr:nth-child(1) th')).toHaveLength(
      3,
    );
    expect(
      container.querySelector(
        'tbody tr:nth-child(1) th[scope="rowgroup"]:nth-child(1)',
      ),
    ).toHaveAttribute('rowspan', '1');
    expect(
      container.querySelector(
        'tbody tr:nth-child(1) th[scope="rowgroup"]:nth-child(2)',
      ),
    ).toHaveAttribute('rowspan', '1');
    expect(
      container.querySelector(
        'tbody tr:nth-child(1) th[scope="row"]:nth-child(3)',
      ),
    ).toHaveAttribute('rowspan', '1');

    // Row 2
    expect(container.querySelectorAll('tbody tr:nth-child(2) th')).toHaveLength(
      3,
    );
    expect(
      container.querySelector(
        'tbody tr:nth-child(2) th[scope="rowgroup"]:nth-child(1)',
      ),
    ).toHaveAttribute('rowspan', '2');
    expect(
      container.querySelector(
        'tbody tr:nth-child(2) th[scope="rowgroup"]:nth-child(2)',
      ),
    ).toHaveAttribute('rowspan', '1');
    expect(
      container.querySelector(
        'tbody tr:nth-child(2) th[scope="row"]:nth-child(3)',
      ),
    ).toHaveAttribute('rowspan', '1');

    // Row 3
    expect(container.querySelectorAll('tbody tr:nth-child(3) th')).toHaveLength(
      2,
    );
    expect(
      container.querySelector(
        'tbody tr:nth-child(3) th[scope="rowgroup"]:nth-child(1)',
      ),
    ).toHaveAttribute('rowspan', '1');
    expect(
      container.querySelector(
        'tbody tr:nth-child(3) th[scope="row"]:nth-child(2)',
      ),
    ).toHaveAttribute('rowspan', '1');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with single column header group', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[
          { headers: [{ text: 'A' }] },
          {
            groups: [{ text: 'B' }],
            headers: [{ text: 'C' }, { text: 'D' }],
          },
        ]}
        rowHeaders={[
          { headers: [{ text: '1' }, { text: '2' }] },
          { headers: [{ text: '3' }, { text: '4' }] },
        ]}
        rows={[
          ['AC13', 'AD13'],
          ['AC14', 'AD14'],
          ['AC23', 'AD23'],
          ['AC24', 'AD24'],
        ]}
      />,
    );

    expect(container.querySelectorAll('thead tr')).toHaveLength(3);

    // Row 1
    expect(container.querySelectorAll('thead tr:nth-child(1) th')).toHaveLength(
      1,
    );
    expect(
      container.querySelector(
        'thead tr:nth-child(1) th[scope="colgroup"]:nth-child(2)',
      ),
    ).toHaveAttribute('colspan', '2');

    // Row 2
    expect(container.querySelectorAll('thead tr:nth-child(2) th')).toHaveLength(
      1,
    );
    expect(
      container.querySelector(
        'thead tr:nth-child(2) th[scope="colgroup"]:nth-child(1)',
      ),
    ).toHaveAttribute('colspan', '2');

    // Row 3
    expect(container.querySelectorAll('thead tr:nth-child(3) th')).toHaveLength(
      2,
    );
    expect(
      container.querySelector(
        'thead tr:nth-child(3) th[scope="col"]:nth-child(1)',
      ),
    ).toHaveAttribute('colspan', '1');
    expect(
      container.querySelector(
        'thead tr:nth-child(3) th[scope="col"]:nth-child(2)',
      ),
    ).toHaveAttribute('colspan', '1');

    // Body
    expect(container.querySelectorAll('tbody tr')).toHaveLength(4);
    expect(container.querySelectorAll('tbody td')).toHaveLength(8);

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with single column header group at start', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[
          {
            groups: [{ text: 'A' }],
            headers: [{ text: 'B' }, { text: 'C' }],
          },
          { headers: [{ text: 'D' }] },
        ]}
        rowHeaders={[
          { headers: [{ text: '1' }, { text: '2' }] },
          { headers: [{ text: '3' }, { text: '4' }] },
        ]}
        rows={[
          ['BD13', 'CD13'],
          ['BD14', 'CD14'],
          ['BD23', 'CD23'],
          ['BD24', 'CD24'],
        ]}
      />,
    );

    expect(container.querySelectorAll('thead tr')).toHaveLength(3);

    // Row 1
    expect(container.querySelectorAll('thead tr:nth-child(1) th')).toHaveLength(
      1,
    );
    expect(
      container.querySelector(
        'thead tr:nth-child(1) th[scope="colgroup"]:nth-child(2)',
      ),
    ).toHaveAttribute('colspan', '2');

    // Row 2
    expect(container.querySelectorAll('thead tr:nth-child(2) th')).toHaveLength(
      2,
    );
    expect(
      container.querySelector(
        'thead tr:nth-child(2) th[scope="colgroup"]:nth-child(1)',
      ),
    ).toHaveAttribute('colspan', '1');
    expect(
      container.querySelector(
        'thead tr:nth-child(2) th[scope="colgroup"]:nth-child(2)',
      ),
    ).toHaveAttribute('colspan', '1');

    // Row 3
    expect(container.querySelectorAll('thead tr:nth-child(3) th')).toHaveLength(
      2,
    );
    expect(
      container.querySelector(
        'thead tr:nth-child(3) th[scope="col"]:nth-child(1)',
      ),
    ).toHaveAttribute('colspan', '1');
    expect(
      container.querySelector(
        'thead tr:nth-child(3) th[scope="col"]:nth-child(2)',
      ),
    ).toHaveAttribute('colspan', '1');

    // Body
    expect(container.querySelectorAll('tbody tr')).toHaveLength(4);
    expect(container.querySelectorAll('tbody td')).toHaveLength(8);

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with two column header groups', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[
          { headers: [{ text: 'A' }] },
          {
            groups: [{ text: 'B' }, { text: 'C', span: 2 }],
            headers: [{ text: 'D' }, { text: 'E' }, { text: 'F' }],
          },
        ]}
        rowHeaders={[
          { headers: [{ text: '1' }, { text: '2' }] },
          { headers: [{ text: '3' }, { text: '4' }] },
        ]}
        rows={[
          ['AD13', 'AE13', 'AF13'],
          ['AD14', 'AE14', 'AF14'],
          ['AD23', 'AE23', 'AF23'],
          ['AD24', 'AE24', 'AF24'],
        ]}
      />,
    );

    expect(container.querySelectorAll('thead tr')).toHaveLength(3);

    // Row 1
    expect(container.querySelectorAll('thead tr:nth-child(1) th')).toHaveLength(
      1,
    );
    expect(
      container.querySelector(
        'thead tr:nth-child(1) th[scope="colgroup"]:nth-child(2)',
      ),
    ).toHaveAttribute('colspan', '3');

    // Row 2
    expect(container.querySelectorAll('thead tr:nth-child(2) th')).toHaveLength(
      2,
    );
    expect(
      container.querySelector(
        'thead tr:nth-child(2) th[scope="colgroup"]:nth-child(1)',
      ),
    ).toHaveAttribute('colspan', '1');
    expect(
      container.querySelector(
        'thead tr:nth-child(2) th[scope="colgroup"]:nth-child(2)',
      ),
    ).toHaveAttribute('colspan', '2');

    // Row 3
    expect(container.querySelectorAll('thead tr:nth-child(3) th')).toHaveLength(
      3,
    );
    expect(
      container.querySelector(
        'thead tr:nth-child(3) th[scope="col"]:nth-child(1)',
      ),
    ).toHaveAttribute('colspan', '1');
    expect(
      container.querySelector(
        'thead tr:nth-child(3) th[scope="col"]:nth-child(2)',
      ),
    ).toHaveAttribute('colspan', '1');
    expect(
      container.querySelector(
        'thead tr:nth-child(3) th[scope="col"]:nth-child(3)',
      ),
    ).toHaveAttribute('colspan', '1');

    // Body
    expect(container.querySelectorAll('tbody tr')).toHaveLength(4);
    expect(container.querySelectorAll('tbody td')).toHaveLength(12);

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders table with two column header groups at start', () => {
    const { container } = render(
      <MultiHeaderTable
        columnHeaders={[
          {
            groups: [{ text: 'A' }, { text: 'B', span: 2 }],
            headers: [{ text: 'C' }, { text: 'D' }, { text: 'E' }],
          },
          { headers: [{ text: 'F' }] },
        ]}
        rowHeaders={[
          { headers: [{ text: '1' }, { text: '2' }] },
          { headers: [{ text: '3' }, { text: '4' }] },
        ]}
        rows={[
          ['CF13', 'DF13', 'EF13'],
          ['CF14', 'DF14', 'EF14'],
          ['CF23', 'DF23', 'EF23'],
          ['CF24', 'DF24', 'EF24'],
        ]}
      />,
    );

    expect(container.querySelectorAll('thead tr')).toHaveLength(3);

    // Row 1
    expect(container.querySelectorAll('thead tr:nth-child(1) th')).toHaveLength(
      2,
    );
    expect(
      container.querySelector(
        'thead tr:nth-child(1) th[scope="colgroup"]:nth-child(2)',
      ),
    ).toHaveAttribute('colspan', '1');
    expect(
      container.querySelector(
        'thead tr:nth-child(1) th[scope="colgroup"]:nth-child(3)',
      ),
    ).toHaveAttribute('colspan', '2');

    // Row 2
    expect(container.querySelectorAll('thead tr:nth-child(2) th')).toHaveLength(
      3,
    );
    expect(
      container.querySelector(
        'thead tr:nth-child(2) th[scope="colgroup"]:nth-child(1)',
      ),
    ).toHaveAttribute('colspan', '1');
    expect(
      container.querySelector(
        'thead tr:nth-child(2) th[scope="colgroup"]:nth-child(2)',
      ),
    ).toHaveAttribute('colspan', '1');
    expect(
      container.querySelector(
        'thead tr:nth-child(2) th[scope="colgroup"]:nth-child(3)',
      ),
    ).toHaveAttribute('colspan', '1');

    // Row 3
    expect(container.querySelectorAll('thead tr:nth-child(3) th')).toHaveLength(
      3,
    );
    expect(
      container.querySelector(
        'thead tr:nth-child(3) th[scope="col"]:nth-child(1)',
      ),
    ).toHaveAttribute('colspan', '1');
    expect(
      container.querySelector(
        'thead tr:nth-child(3) th[scope="col"]:nth-child(2)',
      ),
    ).toHaveAttribute('colspan', '1');
    expect(
      container.querySelector(
        'thead tr:nth-child(3) th[scope="col"]:nth-child(3)',
      ),
    ).toHaveAttribute('colspan', '1');

    // Body
    expect(container.querySelectorAll('tbody tr')).toHaveLength(4);
    expect(container.querySelectorAll('tbody td')).toHaveLength(12);

    expect(container.innerHTML).toMatchSnapshot();
  });
});
