import testPermalinkSnapshot from '@frontend/modules/permalink/__data__/testPermalinkData';
import PermalinkPage from '@frontend/modules/permalink/PermalinkPage';
import { render, screen, within } from '@testing-library/react';
import React from 'react';

describe('PermalinkPage', () => {
  test('renders correctly with permalink', () => {
    render(<PermalinkPage data={testPermalinkSnapshot} />);

    expect(
      screen.getByText("'Data Set 1' from 'Publication 1'", {
        selector: 'h1',
      }),
    ).toBeInTheDocument();

    expect(screen.getByTestId('created-date')).toHaveTextContent(
      '7 October 2020',
    );

    expect(screen.getByRole('figure')).toHaveTextContent(
      'Test table caption 1',
    );

    const table = screen.getByRole('table');

    expect(table.querySelectorAll('thead tr')).toHaveLength(1);
    expect(table.querySelectorAll('thead th')).toHaveLength(4);

    expect(table.querySelectorAll('tbody tr')).toHaveLength(3);
    expect(table.querySelectorAll('tbody th')).toHaveLength(3);
    expect(table.querySelectorAll('tbody td')).toHaveLength(12);

    expect(table).toMatchSnapshot();

    const footnotes = within(screen.getByTestId('footnotes')).getAllByRole(
      'listitem',
    );
    expect(footnotes).toHaveLength(2);
    expect(footnotes[0]).toHaveTextContent('Footnote 1');
    expect(footnotes[1]).toHaveTextContent('Footnote 2');
  });

  test('renders no warning message with permalink status Current', () => {
    render(<PermalinkPage data={testPermalinkSnapshot} />);

    expect(screen.queryByText(/WARNING/)).not.toBeInTheDocument();

    // Table still renders
    expect(screen.getByRole('table')).toBeInTheDocument();
    expect(screen.getAllByRole('row')).toHaveLength(4);
  });

  test('renders warning message with permalink status SubjectRemoved', () => {
    render(
      <PermalinkPage
        data={{
          ...testPermalinkSnapshot,
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
    expect(screen.getAllByRole('row')).toHaveLength(4);
  });

  test('renders warning message with permalink status SubjectReplacedOrRemoved', () => {
    render(
      <PermalinkPage
        data={{
          ...testPermalinkSnapshot,
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
    expect(screen.getAllByRole('row')).toHaveLength(4);
  });

  test('renders warning message with permalink status NotForLatestRelease', () => {
    render(
      <PermalinkPage
        data={{
          ...testPermalinkSnapshot,
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
    expect(screen.getAllByRole('row')).toHaveLength(4);
  });

  test('renders warning message with permalink status PublicationSuperseded', () => {
    render(
      <PermalinkPage
        data={{
          ...testPermalinkSnapshot,
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
    expect(screen.getAllByRole('row')).toHaveLength(4);
  });
});
