import { getDescribedBy } from '@common-test/queries';
import FeaturedTablesList from '@common/modules/table-tool/components/FeaturedTablesList';
import { FeaturedTable } from '@common/services/tableBuilderService';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

describe('FeaturedTablesList', () => {
  const testFeaturedTables: FeaturedTable[] = [
    {
      id: 'highlight-1',
      name: '1 - Unique name',
      description: '1 - Same description',
    },
    {
      id: 'highlight-2',
      name: '10 - Similar name',
      description: '10 - Unique description',
    },
    {
      id: 'highlight-3',
      name: '2 - Similar name',
      description: '2 - Same description',
    },
  ];

  const renderLink = (highlight: FeaturedTable) => (
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
      <FeaturedTablesList
        featuredTables={testFeaturedTables}
        renderLink={renderLink}
      />,
    );

    const listItems = screen.getAllByRole('listitem');

    expect(listItems).toHaveLength(3);

    const link1 = within(listItems[0]).getByRole('link', {
      name: '1 - Unique name',
    });

    expect(link1).toBeInTheDocument();
    expect(getDescribedBy(link1, listItems[0])).toHaveTextContent(
      '1 - Same description',
    );

    const link2 = within(listItems[1]).getByRole('link', {
      name: '2 - Similar name',
    });
    expect(link2).toBeInTheDocument();
    expect(getDescribedBy(link2, listItems[1])).toHaveTextContent(
      '2 - Same description',
    );

    const link3 = within(listItems[2]).getByRole('link', {
      name: '10 - Similar name',
    });

    expect(link3).toBeInTheDocument();
    expect(getDescribedBy(link3, listItems[2])).toHaveTextContent(
      '10 - Unique description',
    );
  });

  test('does not add `aria-describedby` to highlight link if `description` is empty', () => {
    render(
      <FeaturedTablesList
        featuredTables={[
          {
            id: 'highlight-1',
            name: 'Highlight 1',
            description: '',
          },
        ]}
        renderLink={renderLink}
      />,
    );

    expect(
      screen.getByRole('link', { name: 'Highlight 1' }),
    ).not.toHaveAttribute('aria-describedby');
  });

  test('renders empty message if no highlights', () => {
    render(<FeaturedTablesList featuredTables={[]} renderLink={renderLink} />);

    expect(
      screen.getByText(/No featured tables available/),
    ).toBeInTheDocument();
    expect(screen.queryAllByRole('listitem')).toHaveLength(0);
  });

  test('renders single result for search term on name', async () => {
    render(
      <FeaturedTablesList
        featuredTables={testFeaturedTables}
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
      <FeaturedTablesList
        featuredTables={testFeaturedTables}
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
      expect(listItems[1]).toHaveTextContent('10 - Similar name');
    });
  });

  test('renders single result for search term on description', async () => {
    render(
      <FeaturedTablesList
        featuredTables={testFeaturedTables}
        renderLink={renderLink}
      />,
    );

    await userEvent.type(screen.getByRole('textbox'), 'unique description');

    jest.runOnlyPendingTimers();

    await waitFor(() => {
      expect(screen.getByText(/Found 1 matching table/)).toBeInTheDocument();

      const listItems = screen.getAllByRole('listitem');

      expect(listItems).toHaveLength(1);
      expect(listItems[0]).toHaveTextContent('10 - Unique description');
    });
  });

  test('handles search when some highlights have no description', async () => {
    const highlightWithoutDescription = {
      id: 'highlight-4',
      name: '4 - no description',
    };

    render(
      <FeaturedTablesList
        featuredTables={[highlightWithoutDescription, ...testFeaturedTables]}
        renderLink={renderLink}
      />,
    );

    await userEvent.type(screen.getByRole('textbox'), 'unique description');

    jest.runOnlyPendingTimers();

    await waitFor(() => {
      expect(screen.getByText(/Found 1 matching table/)).toBeInTheDocument();

      const listItems = screen.getAllByRole('listitem');

      expect(listItems).toHaveLength(1);
      expect(listItems[0]).toHaveTextContent('10 - Unique description');
    });
  });

  test('renders empty message if no results', async () => {
    render(
      <FeaturedTablesList
        featuredTables={testFeaturedTables}
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
