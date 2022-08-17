import Pagination from '@common/components/Pagination';
import { render as baseRender, screen, within } from '@testing-library/react';
import React from 'react';

describe('Pagination', () => {
  test('renders the pagination without spacers when there are fewer than the max page numbers', () => {
    render({ currentPage: 3, totalPages: 5 });

    expect(
      screen.getByRole('navigation', { name: 'Pagination navigation' }),
    ).toBeInTheDocument();

    const links = screen.getAllByRole('link');
    expect(links).toHaveLength(7);

    const listItems = screen.getAllByRole('listitem');
    expect(listItems).toHaveLength(5);

    // Previous link
    expect(links[0]).toHaveAttribute('rel', 'prev');
    expect(links[0]).toHaveAttribute('href', `/test-url?page=${2}`);
    expect(within(links[0]).getByText('Previous')).toBeInTheDocument();

    // Page 1
    expect(links[1]).toHaveAttribute('aria-label', 'Page 1');
    expect(links[1]).toHaveAttribute('href', `/test-url?page=${1}`);
    expect(links[1]).not.toHaveAttribute('aria-current');
    expect(within(links[1]).getByText('1')).toBeInTheDocument();

    // Page 2
    expect(links[2]).toHaveAttribute('aria-label', 'Page 2');
    expect(links[2]).toHaveAttribute('href', `/test-url?page=${2}`);
    expect(links[2]).not.toHaveAttribute('aria-current');
    expect(within(links[2]).getByText('2')).toBeInTheDocument();

    // Page 3 - current page
    expect(links[3]).toHaveAttribute('aria-label', 'Page 3');
    expect(links[3]).toHaveAttribute('href', `/test-url?page=${3}`);
    expect(links[3]).toHaveAttribute('aria-current', 'page');
    expect(within(links[3]).getByText('3')).toBeInTheDocument();

    // Page 4
    expect(links[4]).toHaveAttribute('aria-label', 'Page 4');
    expect(links[4]).toHaveAttribute('href', `/test-url?page=${4}`);
    expect(links[4]).not.toHaveAttribute('aria-current');
    expect(within(links[4]).getByText('4')).toBeInTheDocument();

    // Page 5
    expect(links[5]).toHaveAttribute('aria-label', 'Page 5');
    expect(links[5]).toHaveAttribute('href', `/test-url?page=${5}`);
    expect(links[5]).not.toHaveAttribute('aria-current');
    expect(within(links[5]).getByText('5')).toBeInTheDocument();

    // Next link
    expect(links[6]).toHaveAttribute('rel', 'next');
    expect(links[6]).toHaveAttribute('href', `/test-url?page=${4}`);
    expect(within(links[6]).getByText('Next')).toBeInTheDocument();
  });

  test('renders the pagination with spacers when there are more than the max page numbers', () => {
    render({ currentPage: 5, totalPages: 10 });

    expect(
      screen.getByRole('navigation', { name: 'Pagination navigation' }),
    ).toBeInTheDocument();

    const links = screen.getAllByRole('link');
    expect(links).toHaveLength(7);

    const listItems = screen.getAllByRole('listitem');
    expect(listItems).toHaveLength(7);

    // Previous link
    expect(links[0]).toHaveAttribute('rel', 'prev');
    expect(links[0]).toHaveAttribute('href', `/test-url?page=${4}`);
    expect(within(links[0]).getByText('Previous')).toBeInTheDocument();

    // Page 1
    expect(links[1]).toHaveAttribute('aria-label', 'Page 1');
    expect(links[1]).toHaveAttribute('href', `/test-url?page=${1}`);
    expect(links[1]).not.toHaveAttribute('aria-current');
    expect(within(links[1]).getByText('1')).toBeInTheDocument();

    // Spacer
    expect(listItems[1]).toHaveTextContent('…');

    // Page 2
    expect(
      screen.queryByRole('link', { name: 'Page 2' }),
    ).not.toBeInTheDocument();

    // Page 3
    expect(
      screen.queryByRole('link', { name: 'Page 3' }),
    ).not.toBeInTheDocument();

    // Page 4
    expect(links[2]).toHaveAttribute('aria-label', 'Page 4');
    expect(links[2]).toHaveAttribute('href', `/test-url?page=${4}`);
    expect(links[2]).not.toHaveAttribute('aria-current');
    expect(within(links[2]).getByText('4')).toBeInTheDocument();

    // Page 5 - current page
    expect(links[3]).toHaveAttribute('aria-label', 'Page 5');
    expect(links[3]).toHaveAttribute('href', `/test-url?page=${5}`);
    expect(links[3]).toHaveAttribute('aria-current', 'page');
    expect(within(links[3]).getByText('5')).toBeInTheDocument();

    // Page 6
    expect(links[4]).toHaveAttribute('aria-label', 'Page 6');
    expect(links[4]).toHaveAttribute('href', `/test-url?page=${6}`);
    expect(links[4]).not.toHaveAttribute('aria-current');
    expect(within(links[4]).getByText('6')).toBeInTheDocument();

    // Spacer
    expect(listItems[5]).toHaveTextContent('…');

    // Page 7
    expect(
      screen.queryByRole('link', { name: 'Page 7' }),
    ).not.toBeInTheDocument();

    // Page 8
    expect(
      screen.queryByRole('link', { name: 'Page 8' }),
    ).not.toBeInTheDocument();

    // Page 9
    expect(
      screen.queryByRole('link', { name: 'Page 9' }),
    ).not.toBeInTheDocument();

    // Page 10
    expect(links[5]).toHaveAttribute('aria-label', 'Page 10');
    expect(links[5]).toHaveAttribute('href', `/test-url?page=${10}`);
    expect(links[5]).not.toHaveAttribute('aria-current');
    expect(within(links[5]).getByText('10')).toBeInTheDocument();

    // Next link
    expect(links[6]).toHaveAttribute('rel', 'next');
    expect(links[6]).toHaveAttribute('href', `/test-url?page=${6}`);
    expect(within(links[6]).getByText('Next')).toBeInTheDocument();
  });

  test('does not render the Previous link when the first page is selected', () => {
    render({ currentPage: 1, totalPages: 3 });

    expect(
      screen.getByRole('navigation', { name: 'Pagination navigation' }),
    ).toBeInTheDocument();

    const links = screen.getAllByRole('link');
    expect(links).toHaveLength(4);

    expect(
      screen.queryByRole('link', { name: 'Previous' }),
    ).not.toBeInTheDocument();

    // Page 1
    expect(links[0]).toHaveAttribute('aria-label', 'Page 1');
    expect(links[0]).toHaveAttribute('href', `/test-url?page=${1}`);
    expect(links[0]).toHaveAttribute('aria-current');
    expect(within(links[0]).getByText('1')).toBeInTheDocument();

    // Page 2
    expect(links[1]).toHaveAttribute('aria-label', 'Page 2');
    expect(links[1]).toHaveAttribute('href', `/test-url?page=${2}`);
    expect(links[1]).not.toHaveAttribute('aria-current');
    expect(within(links[1]).getByText('2')).toBeInTheDocument();

    // Page 3
    expect(links[2]).toHaveAttribute('aria-label', 'Page 3');
    expect(links[2]).toHaveAttribute('href', `/test-url?page=${3}`);
    expect(links[2]).not.toHaveAttribute('aria-current');
    expect(within(links[2]).getByText('3')).toBeInTheDocument();

    // Next link
    expect(links[3]).toHaveAttribute('rel', 'next');
    expect(links[3]).toHaveAttribute('href', `/test-url?page=${2}`);
    expect(within(links[3]).getByText('Next')).toBeInTheDocument();
  });

  test('does not render the Next link when the last page is selected', () => {
    render({ currentPage: 3, totalPages: 3 });

    expect(
      screen.getByRole('navigation', { name: 'Pagination navigation' }),
    ).toBeInTheDocument();

    const links = screen.getAllByRole('link');
    expect(links).toHaveLength(4);

    expect(
      screen.queryByRole('link', { name: 'Next' }),
    ).not.toBeInTheDocument();

    // Previous link
    expect(links[0]).toHaveAttribute('rel', 'prev');
    expect(links[0]).toHaveAttribute('href', `/test-url?page=${2}`);
    expect(within(links[0]).getByText('Previous')).toBeInTheDocument();

    // Page 1
    expect(links[1]).toHaveAttribute('aria-label', 'Page 1');
    expect(links[1]).toHaveAttribute('href', `/test-url?page=${1}`);
    expect(links[1]).not.toHaveAttribute('aria-current');
    expect(within(links[1]).getByText('1')).toBeInTheDocument();

    // Page 2
    expect(links[2]).toHaveAttribute('aria-label', 'Page 2');
    expect(links[2]).toHaveAttribute('href', `/test-url?page=${2}`);
    expect(links[2]).not.toHaveAttribute('aria-current');
    expect(within(links[2]).getByText('2')).toBeInTheDocument();

    // Page 3
    expect(links[3]).toHaveAttribute('aria-label', 'Page 3');
    expect(links[3]).toHaveAttribute('href', `/test-url?page=${3}`);
    expect(links[3]).toHaveAttribute('aria-current');
    expect(within(links[3]).getByText('3')).toBeInTheDocument();
  });

  test('does not render if there are no pages', () => {
    render({ currentPage: 1, totalPages: 0 });

    expect(
      screen.queryByRole('navigation', { name: 'Pagination navigation' }),
    ).not.toBeInTheDocument();
  });

  test('does not render if there is only one page', () => {
    render({ currentPage: 1, totalPages: 1 });

    expect(
      screen.queryByRole('navigation', { name: 'Pagination navigation' }),
    ).not.toBeInTheDocument();
  });
});

function render({
  currentPage,
  totalPages,
}: {
  currentPage: number;
  totalPages: number;
}) {
  baseRender(
    <Pagination
      currentPage={currentPage}
      nextPrevLinkRenderer={({ children, className, pageNumber, rel }) => (
        <a
          className={className}
          href={`/test-url?page=${pageNumber}`}
          rel={rel}
        >
          {children}
        </a>
      )}
      pageLinkRenderer={({ ariaCurrent, ariaLabel, className, pageNumber }) => (
        <a
          aria-current={ariaCurrent}
          aria-label={ariaLabel}
          className={className}
          href={`/test-url?page=${pageNumber}`}
        >
          {pageNumber}
        </a>
      )}
      totalPages={totalPages}
    />,
  );
}
