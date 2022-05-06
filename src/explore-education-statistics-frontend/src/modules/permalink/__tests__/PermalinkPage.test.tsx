import {
  testPermalink,
  testPermalinkWithHierarchicalLocations,
  testPermalinkWithLocationCodes,
  testPermalinkWithResultLocationIds,
} from '@frontend/modules/permalink/__data__/testPermalinkData';
import PermalinkPage from '@frontend/modules/permalink/PermalinkPage';
import { render, screen, within } from '@testing-library/react';
import React from 'react';

describe('PermalinkPage', () => {
  test('renders correctly with permalink', () => {
    render(<PermalinkPage data={testPermalink} />);

    expect(
      screen.getByText("'Subject 1' from 'Test publication'", {
        selector: 'h1',
      }),
    ).toBeInTheDocument();

    expect(screen.getByTestId('created-date')).toHaveTextContent(
      '7 October 2020',
    );

    expect(screen.getByRole('figure')).toHaveTextContent(
      "Number of authorised absence sessions for 'Subject 1' for Gender female in Barnet for 2020/21",
    );

    expect(screen.getByRole('table')).toBeInTheDocument();

    const rows = screen.getAllByRole('row');

    expect(rows).toHaveLength(2);
    expect(within(rows[0]).getByRole('columnheader')).toHaveTextContent(
      '2020/21',
    );
    expect(within(rows[1]).getByRole('rowheader')).toHaveTextContent('Barnet');
    expect(within(rows[1]).getByRole('cell')).toHaveTextContent('123');

    expect(
      screen.getByText('Source: Test publication, Subject 1'),
    ).toBeInTheDocument();
  });

  test('renders correctly with hierarchical locations', () => {
    render(<PermalinkPage data={testPermalinkWithHierarchicalLocations} />);

    expect(
      screen.getByText("'Subject 1' from 'Test publication'", {
        selector: 'h1',
      }),
    ).toBeInTheDocument();

    expect(screen.getByTestId('created-date')).toHaveTextContent(
      '7 October 2020',
    );

    expect(screen.getByRole('figure')).toHaveTextContent(
      "Number of authorised absence sessions for 'Subject 1' for Gender female in Barnet and Bolton for 2020/21",
    );

    expect(screen.getByRole('table')).toBeInTheDocument();

    const table = screen.getByRole('table');

    expect(table.querySelectorAll('thead tr')).toHaveLength(1);
    expect(table.querySelectorAll('thead th')).toHaveLength(1);
    expect(table.querySelectorAll('thead th[scope="colgroup"]')).toHaveLength(
      0,
    );
    expect(table.querySelectorAll('thead th[scope="col"]')).toHaveLength(1);
    expect(table.querySelector('thead th[scope="col"]')).toHaveTextContent(
      '2020/21',
    );

    const rows = table.querySelectorAll('tbody tr');

    expect(rows).toHaveLength(2);

    // Row 1

    const row1Headers = rows[0].querySelectorAll('th');
    const row1Cells = rows[0].querySelectorAll('td');

    expect(row1Headers).toHaveLength(2);

    expect(row1Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row1Headers[0]).toHaveAttribute('colspan', '1');
    expect(row1Headers[0]).toHaveTextContent('Outer London');

    expect(row1Headers[1]).toHaveAttribute('scope', 'row');
    expect(row1Headers[1]).toHaveAttribute('colspan', '1');
    expect(row1Headers[1]).toHaveTextContent('Barnet');

    expect(row1Cells).toHaveLength(1);
    expect(row1Cells[0]).toHaveTextContent('123');

    // Row 2

    const row2Headers = rows[1].querySelectorAll('th');
    const row2Cells = rows[1].querySelectorAll('td');

    expect(row2Headers).toHaveLength(2);

    expect(row2Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row2Headers[0]).toHaveAttribute('colspan', '1');
    expect(row2Headers[0]).toHaveTextContent('North West');

    expect(row2Headers[1]).toHaveAttribute('scope', 'row');
    expect(row2Headers[1]).toHaveAttribute('colspan', '1');
    expect(row2Headers[1]).toHaveTextContent('Bolton');

    expect(row2Cells).toHaveLength(1);
    expect(row2Cells[0]).toHaveTextContent('456');

    expect(
      screen.getByText('Source: Test publication, Subject 1'),
    ).toBeInTheDocument();
  });

  test("renders correctly with a permalink created prior to the switchover from Location codes to id's", () => {
    render(<PermalinkPage data={testPermalinkWithLocationCodes} />);

    expect(
      screen.getByText("'Subject 1' from 'Test publication'", {
        selector: 'h1',
      }),
    ).toBeInTheDocument();

    expect(screen.getByTestId('created-date')).toHaveTextContent(
      '7 October 2020',
    );

    expect(screen.getByRole('figure')).toHaveTextContent(
      "Number of authorised absence sessions for 'Subject 1' for Gender female in Barnet for 2020/21",
    );

    expect(screen.getByRole('table')).toBeInTheDocument();

    const rows = screen.getAllByRole('row');

    expect(rows).toHaveLength(2);
    expect(within(rows[0]).getByRole('columnheader')).toHaveTextContent(
      '2020/21',
    );
    expect(within(rows[1]).getByRole('rowheader')).toHaveTextContent('Barnet');
    expect(within(rows[1]).getByRole('cell')).toHaveTextContent('123');

    expect(
      screen.getByText('Source: Test publication, Subject 1'),
    ).toBeInTheDocument();
  });

  // Test a special case where a Permalink has 'locationId' in each result but still uses location codes.
  // This was a possible state after EES-3203.
  test("renders correctly with a permalink that has location id's in results", () => {
    render(<PermalinkPage data={testPermalinkWithResultLocationIds} />);

    expect(
      screen.getByText("'Subject 1' from 'Test publication'", {
        selector: 'h1',
      }),
    ).toBeInTheDocument();

    expect(screen.getByTestId('created-date')).toHaveTextContent(
      '7 October 2020',
    );

    expect(screen.getByRole('figure')).toHaveTextContent(
      "Number of authorised absence sessions for 'Subject 1' for Gender female in Barnet for 2020/21",
    );

    expect(screen.getByRole('table')).toBeInTheDocument();

    const rows = screen.getAllByRole('row');

    expect(rows).toHaveLength(2);
    expect(within(rows[0]).getByRole('columnheader')).toHaveTextContent(
      '2020/21',
    );
    expect(within(rows[1]).getByRole('rowheader')).toHaveTextContent('Barnet');
    expect(within(rows[1]).getByRole('cell')).toHaveTextContent('123');

    expect(
      screen.getByText('Source: Test publication, Subject 1'),
    ).toBeInTheDocument();
  });

  test('renders no warning message with permalink status Current', () => {
    render(
      <PermalinkPage
        data={{
          ...testPermalink,
          status: 'Current',
        }}
      />,
    );

    expect(screen.queryByText(/WARNING/)).not.toBeInTheDocument();

    // Table still renders
    expect(screen.getByRole('table')).toBeInTheDocument();
    expect(screen.getAllByRole('row')).toHaveLength(2);
  });

  test('renders warning message with permalink status SubjectRemoved', () => {
    render(
      <PermalinkPage
        data={{
          ...testPermalink,
          status: 'SubjectRemoved',
        }}
      />,
    );

    expect(
      screen.getByText(
        'WARNING - The data used in this table is no longer valid.',
      ),
    ).toBeInTheDocument();

    // Table still renders
    expect(screen.getByRole('table')).toBeInTheDocument();
    expect(screen.getAllByRole('row')).toHaveLength(2);
  });

  test('renders warning message with permalink status SubjectReplacedOrRemoved', () => {
    render(
      <PermalinkPage
        data={{
          ...testPermalink,
          status: 'SubjectReplacedOrRemoved',
        }}
      />,
    );

    expect(
      screen.getByText(
        'WARNING - The data used in this table may be invalid as the subject file has been amended or removed since its creation.',
      ),
    ).toBeInTheDocument();

    // Table still renders
    expect(screen.getByRole('table')).toBeInTheDocument();
    expect(screen.getAllByRole('row')).toHaveLength(2);
  });

  test('renders warning message with permalink status NotForLatestRelease', () => {
    render(
      <PermalinkPage
        data={{
          ...testPermalink,
          status: 'NotForLatestRelease',
        }}
      />,
    );

    expect(
      screen.getByText(
        'WARNING - The data used in this table may now be out-of-date as a new release has been published since its creation.',
      ),
    ).toBeInTheDocument();

    // Table still renders
    expect(screen.getByRole('table')).toBeInTheDocument();
    expect(screen.getAllByRole('row')).toHaveLength(2);
  });

  test('renders warning message with permalink status PublicationSuperseded', () => {
    render(
      <PermalinkPage
        data={{
          ...testPermalink,
          status: 'PublicationSuperseded',
        }}
      />,
    );

    expect(
      screen.getByText(
        'WARNING - The data used in this table may now be out-of-date as a new release has been published since its creation.',
      ),
    ).toBeInTheDocument();

    // Table still renders
    expect(screen.getByRole('table')).toBeInTheDocument();
    expect(screen.getAllByRole('row')).toHaveLength(2);
  });
});
