import TableHighlightsList from '@common/modules/table-tool/components/TableHighlightsList';
import { TableHighlight } from '@common/services/tableBuilderService';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

describe('TableHighlightsList', () => {
  const testHighlights: TableHighlight[] = [
    {
      id: 'highlight-1',
      name: '1 - Unique name',
      description: '1 - Same description',
    },
    {
      id: 'highlight-2',
      name: '2 - Similar name',
      description: '2 - Unique description',
    },
    {
      id: 'highlight-3',
      name: '3 - Similar name',
      description: '3 - Same description',
    },
  ];

  const renderLink = (highlight: TableHighlight) => (
    <a href="/">{highlight.name}</a>
  );

  beforeEach(() => {
    jest.useFakeTimers();
  });

  afterEach(() => {
    jest.useRealTimers();
  });

  test('renders highlights in correct order', () => {
    render(
      <TableHighlightsList
        highlights={testHighlights}
        renderLink={renderLink}
      />,
    );

    const listItems = screen.getAllByRole('listitem');

    expect(listItems).toHaveLength(3);
    expect(
      within(listItems[0]).getByRole('link', { name: '1 - Unique name' }),
    ).toBeInTheDocument();
    expect(listItems[0]).toHaveTextContent('1 - Same description');

    expect(
      within(listItems[1]).getByRole('link', { name: '2 - Similar name' }),
    ).toBeInTheDocument();
    expect(listItems[1]).toHaveTextContent('2 - Unique description');

    expect(
      within(listItems[2]).getByRole('link', { name: '3 - Similar name' }),
    ).toBeInTheDocument();
    expect(listItems[2]).toHaveTextContent('3 - Same description');
  });

  test('renders empty message if no highlights', () => {
    render(<TableHighlightsList highlights={[]} renderLink={renderLink} />);

    expect(screen.getByText(/No popular tables available/)).toBeInTheDocument();
    expect(screen.queryAllByRole('listitem')).toHaveLength(0);
  });

  test('renders single result for search term on name', async () => {
    render(
      <TableHighlightsList
        highlights={testHighlights}
        renderLink={renderLink}
      />,
    );

    await userEvent.type(screen.getByRole('textbox'), 'unique name');

    jest.runOnlyPendingTimers();

    await waitFor(() => {
      expect(screen.getByText(/Found 1 matching table/)).toBeInTheDocument();

      const listItems = screen.getAllByRole('listitem');

      expect(listItems).toHaveLength(1);
      expect(listItems[0]).toHaveTextContent('1 - Unique name');
    });
  });

  test('renders multiple results for search term on name', async () => {
    render(
      <TableHighlightsList
        highlights={testHighlights}
        renderLink={renderLink}
      />,
    );

    await userEvent.type(screen.getByRole('textbox'), 'similar name');

    jest.runOnlyPendingTimers();

    await waitFor(() => {
      expect(screen.getByText(/Found 2 matching tables/)).toBeInTheDocument();

      const listItems = screen.getAllByRole('listitem');

      expect(listItems).toHaveLength(2);
      expect(listItems[0]).toHaveTextContent('2 - Similar name');
      expect(listItems[1]).toHaveTextContent('3 - Similar name');
    });
  });

  test('renders single result for search term on description', async () => {
    render(
      <TableHighlightsList
        highlights={testHighlights}
        renderLink={renderLink}
      />,
    );

    await userEvent.type(screen.getByRole('textbox'), 'unique description');

    jest.runOnlyPendingTimers();

    await waitFor(() => {
      expect(screen.getByText(/Found 1 matching table/)).toBeInTheDocument();

      const listItems = screen.getAllByRole('listitem');

      expect(listItems).toHaveLength(1);
      expect(listItems[0]).toHaveTextContent('2 - Unique description');
    });
  });

  test('renders multiple results for search term on name', async () => {
    render(
      <TableHighlightsList
        highlights={testHighlights}
        renderLink={renderLink}
      />,
    );

    await userEvent.type(screen.getByRole('textbox'), 'same description');

    jest.runOnlyPendingTimers();

    await waitFor(() => {
      expect(screen.getByText(/Found 2 matching tables/)).toBeInTheDocument();

      const listItems = screen.getAllByRole('listitem');

      expect(listItems).toHaveLength(2);
      expect(listItems[0]).toHaveTextContent('1 - Same description');
      expect(listItems[1]).toHaveTextContent('3 - Same description');
    });
  });

  test('renders empty message if no results', async () => {
    render(
      <TableHighlightsList
        highlights={testHighlights}
        renderLink={renderLink}
      />,
    );

    await userEvent.type(screen.getByRole('textbox'), 'does not exist');

    jest.runOnlyPendingTimers();

    await waitFor(() => {
      expect(screen.getByText(/Found 0 matching tables/)).toBeInTheDocument();

      expect(screen.queryAllByRole('listitem')).toHaveLength(0);
    });
  });
});
