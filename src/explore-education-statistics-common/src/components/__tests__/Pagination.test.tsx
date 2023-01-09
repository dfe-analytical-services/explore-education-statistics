import Pagination from '@common/components/Pagination';
import { render as baseRender, screen } from '@testing-library/react';
import React from 'react';

let mockIsMedia = false;
jest.mock('@common/hooks/useMedia', () => ({
  useMobileMedia: () => {
    return {
      isMedia: mockIsMedia,
    };
  },
}));

describe('Pagination', () => {
  describe('desktop', () => {
    test('renders the pagination without spacers when there are fewer than 8 pages', () => {
      render({ currentPage: 3, totalPages: 5 });

      expect(
        screen.getByRole('navigation', { name: 'Pagination' }),
      ).toBeInTheDocument();

      const links = screen.getAllByRole('link');
      expect(links).toHaveLength(7);

      const listItems = screen.getAllByRole('listitem');
      expect(listItems).toHaveLength(5);

      // Previous link
      expect(links[0]).toHaveAttribute('rel', 'prev');
      expect(links[0]).toHaveAttribute('href', '/test-url?page=2');
      expect(links[0]).toHaveTextContent('Previous');

      // Page 1
      expect(links[1]).toHaveAttribute('aria-label', 'Page 1');
      expect(links[1]).toHaveAttribute('href', '/test-url?page=1');
      expect(links[1]).not.toHaveAttribute('aria-current');
      expect(links[1]).toHaveTextContent('1');

      // Page 2
      expect(links[2]).toHaveAttribute('aria-label', 'Page 2');
      expect(links[2]).toHaveAttribute('href', '/test-url?page=2');
      expect(links[2]).not.toHaveAttribute('aria-current');
      expect(links[2]).toHaveTextContent('2');

      // Page 3 - current page
      expect(links[3]).toHaveAttribute('aria-label', 'Page 3');
      expect(links[3]).toHaveAttribute('href', '/test-url?page=3');
      expect(links[3]).toHaveAttribute('aria-current', 'page');
      expect(links[3]).toHaveTextContent('3');

      // Page 4
      expect(links[4]).toHaveAttribute('aria-label', 'Page 4');
      expect(links[4]).toHaveAttribute('href', '/test-url?page=4');
      expect(links[4]).not.toHaveAttribute('aria-current');
      expect(links[4]).toHaveTextContent('4');

      // Page 5
      expect(links[5]).toHaveAttribute('aria-label', 'Page 5');
      expect(links[5]).toHaveAttribute('href', '/test-url?page=5');
      expect(links[5]).not.toHaveAttribute('aria-current');
      expect(links[5]).toHaveTextContent('5');

      // Next link
      expect(links[6]).toHaveAttribute('rel', 'next');
      expect(links[6]).toHaveAttribute('href', '/test-url?page=4');
      expect(links[6]).toHaveTextContent('Next');
    });

    test('renders the pagination with spacers when there are more than 7 pages', () => {
      render({ currentPage: 5, totalPages: 10 });

      expect(
        screen.getByRole('navigation', { name: 'Pagination' }),
      ).toBeInTheDocument();

      const links = screen.getAllByRole('link');
      expect(links).toHaveLength(7);

      const listItems = screen.getAllByRole('listitem');
      expect(listItems).toHaveLength(7);

      // Previous link
      expect(links[0]).toHaveAttribute('rel', 'prev');
      expect(links[0]).toHaveAttribute('href', '/test-url?page=4');
      expect(links[0]).toHaveTextContent('Previous');

      // Page 1
      expect(links[1]).toHaveAttribute('aria-label', 'Page 1');
      expect(links[1]).toHaveAttribute('href', '/test-url?page=1');
      expect(links[1]).not.toHaveAttribute('aria-current');
      expect(links[1]).toHaveTextContent('1');

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
      expect(links[2]).toHaveAttribute('href', '/test-url?page=4');
      expect(links[2]).not.toHaveAttribute('aria-current');
      expect(links[2]).toHaveTextContent('4');

      // Page 5 - current page
      expect(links[3]).toHaveAttribute('aria-label', 'Page 5');
      expect(links[3]).toHaveAttribute('href', '/test-url?page=5');
      expect(links[3]).toHaveAttribute('aria-current', 'page');
      expect(links[3]).toHaveTextContent('5');

      // Page 6
      expect(links[4]).toHaveAttribute('aria-label', 'Page 6');
      expect(links[4]).toHaveAttribute('href', '/test-url?page=6');
      expect(links[4]).not.toHaveAttribute('aria-current');
      expect(links[4]).toHaveTextContent('6');

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
      expect(links[5]).toHaveAttribute('href', '/test-url?page=10');
      expect(links[5]).not.toHaveAttribute('aria-current');
      expect(links[5]).toHaveTextContent('10');

      // Next link
      expect(links[6]).toHaveAttribute('rel', 'next');
      expect(links[6]).toHaveAttribute('href', '/test-url?page=6');
      expect(links[6]).toHaveTextContent('Next');
    });

    test('does not render the Previous link when the first page is selected', () => {
      render({ currentPage: 1, totalPages: 3 });

      expect(
        screen.getByRole('navigation', { name: 'Pagination' }),
      ).toBeInTheDocument();

      const links = screen.getAllByRole('link');
      expect(links).toHaveLength(4);

      const listItems = screen.getAllByRole('listitem');
      expect(listItems).toHaveLength(3);

      expect(
        screen.queryByRole('link', { name: 'Previous' }),
      ).not.toBeInTheDocument();

      // Page 1
      expect(links[0]).toHaveAttribute('aria-label', 'Page 1');
      expect(links[0]).toHaveAttribute('href', '/test-url?page=1');
      expect(links[0]).toHaveAttribute('aria-current');
      expect(links[0]).toHaveTextContent('1');

      // Page 2
      expect(links[1]).toHaveAttribute('aria-label', 'Page 2');
      expect(links[1]).toHaveAttribute('href', '/test-url?page=2');
      expect(links[1]).not.toHaveAttribute('aria-current');
      expect(links[1]).toHaveTextContent('2');

      // Page 3
      expect(links[2]).toHaveAttribute('aria-label', 'Page 3');
      expect(links[2]).toHaveAttribute('href', '/test-url?page=3');
      expect(links[2]).not.toHaveAttribute('aria-current');
      expect(links[2]).toHaveTextContent('3');

      // Next link
      expect(links[3]).toHaveAttribute('rel', 'next');
      expect(links[3]).toHaveAttribute('href', '/test-url?page=2');
      expect(links[3]).toHaveTextContent('Next');
    });

    test('does not render the Next link when the last page is selected', () => {
      render({ currentPage: 3, totalPages: 3 });

      expect(
        screen.getByRole('navigation', { name: 'Pagination' }),
      ).toBeInTheDocument();

      const links = screen.getAllByRole('link');
      expect(links).toHaveLength(4);

      const listItems = screen.getAllByRole('listitem');
      expect(listItems).toHaveLength(3);

      expect(
        screen.queryByRole('link', { name: 'Next' }),
      ).not.toBeInTheDocument();

      // Previous link
      expect(links[0]).toHaveAttribute('rel', 'prev');
      expect(links[0]).toHaveAttribute('href', '/test-url?page=2');
      expect(links[0]).toHaveTextContent('Previous');

      // Page 1
      expect(links[1]).toHaveAttribute('aria-label', 'Page 1');
      expect(links[1]).toHaveAttribute('href', '/test-url?page=1');
      expect(links[1]).not.toHaveAttribute('aria-current');
      expect(links[1]).toHaveTextContent('1');

      // Page 2
      expect(links[2]).toHaveAttribute('aria-label', 'Page 2');
      expect(links[2]).toHaveAttribute('href', '/test-url?page=2');
      expect(links[2]).not.toHaveAttribute('aria-current');
      expect(links[2]).toHaveTextContent('2');

      // Page 3
      expect(links[3]).toHaveAttribute('aria-label', 'Page 3');
      expect(links[3]).toHaveAttribute('href', '/test-url?page=3');
      expect(links[3]).toHaveAttribute('aria-current');
      expect(links[3]).toHaveTextContent('3');
    });

    test('does not render if there are no pages', () => {
      render({ currentPage: 1, totalPages: 0 });

      expect(
        screen.queryByRole('navigation', { name: 'Pagination' }),
      ).not.toBeInTheDocument();
    });

    test('does not render if there is only one page', () => {
      render({ currentPage: 1, totalPages: 1 });

      expect(
        screen.queryByRole('navigation', { name: 'Pagination' }),
      ).not.toBeInTheDocument();
    });
  });

  describe('mobile', () => {
    beforeAll(() => {
      mockIsMedia = true;
    });
    test('renders the pagination when there are fewer than 4 pages', () => {
      render({ currentPage: 2, totalPages: 3 });

      expect(
        screen.getByRole('navigation', { name: 'Pagination' }),
      ).toBeInTheDocument();

      const links = screen.getAllByRole('link');
      expect(links).toHaveLength(5);

      const listItems = screen.getAllByRole('listitem');
      expect(listItems).toHaveLength(3);

      // Previous link
      expect(links[0]).toHaveAttribute('rel', 'prev');
      expect(links[0]).toHaveAttribute('href', '/test-url?page=1');
      expect(links[0]).toHaveTextContent('Previous');

      // Page 1
      expect(links[1]).toHaveAttribute('aria-label', 'Page 1');
      expect(links[1]).toHaveAttribute('href', '/test-url?page=1');
      expect(links[1]).not.toHaveAttribute('aria-current');
      expect(links[1]).toHaveTextContent('1');

      // Page 2 - current page
      expect(links[2]).toHaveAttribute('aria-label', 'Page 2');
      expect(links[2]).toHaveAttribute('href', '/test-url?page=2');
      expect(links[2]).toHaveAttribute('aria-current', 'page');
      expect(links[2]).toHaveTextContent('2');

      // Page 3
      expect(links[3]).toHaveAttribute('aria-label', 'Page 3');
      expect(links[3]).toHaveAttribute('href', '/test-url?page=3');
      expect(links[3]).not.toHaveAttribute('aria-current');
      expect(links[3]).toHaveTextContent('3');

      // Next link
      expect(links[4]).toHaveAttribute('rel', 'next');
      expect(links[4]).toHaveAttribute('href', '/test-url?page=3');
      expect(links[4]).toHaveTextContent('Next');
    });

    test('renders the pagination with the current page and spacers when there are more than 3 pages', () => {
      render({ currentPage: 5, totalPages: 10 });

      expect(
        screen.getByRole('navigation', { name: 'Pagination' }),
      ).toBeInTheDocument();

      const links = screen.getAllByRole('link');
      expect(links).toHaveLength(5);

      const listItems = screen.getAllByRole('listitem');
      expect(listItems).toHaveLength(5);

      // Previous link
      expect(links[0]).toHaveAttribute('rel', 'prev');
      expect(links[0]).toHaveAttribute('href', '/test-url?page=4');
      expect(links[0]).toHaveTextContent('Previous');

      // Page 1
      expect(links[1]).toHaveAttribute('aria-label', 'Page 1');
      expect(links[1]).toHaveAttribute('href', '/test-url?page=1');
      expect(links[1]).not.toHaveAttribute('aria-current');
      expect(links[1]).toHaveTextContent('1');

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
      expect(
        screen.queryByRole('link', { name: 'Page 4' }),
      ).not.toBeInTheDocument();

      // Page 5 - current page
      expect(links[2]).toHaveAttribute('aria-label', 'Page 5');
      expect(links[2]).toHaveAttribute('href', '/test-url?page=5');
      expect(links[2]).toHaveAttribute('aria-current', 'page');
      expect(links[2]).toHaveTextContent('5');

      // Spacer
      expect(listItems[3]).toHaveTextContent('…');

      // Page 6
      expect(
        screen.queryByRole('link', { name: 'Page 6' }),
      ).not.toBeInTheDocument();

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
      expect(links[3]).toHaveAttribute('aria-label', 'Page 10');
      expect(links[3]).toHaveAttribute('href', '/test-url?page=10');
      expect(links[3]).not.toHaveAttribute('aria-current');
      expect(links[3]).toHaveTextContent('10');

      // Next link
      expect(links[4]).toHaveAttribute('rel', 'next');
      expect(links[4]).toHaveAttribute('href', '/test-url?page=6');
      expect(links[4]).toHaveTextContent('Next');
    });

    test('does not render the Previous link when the first page is selected', () => {
      render({ currentPage: 1, totalPages: 3 });

      expect(
        screen.getByRole('navigation', { name: 'Pagination' }),
      ).toBeInTheDocument();

      const links = screen.getAllByRole('link');
      expect(links).toHaveLength(3);

      const listItems = screen.getAllByRole('listitem');
      expect(listItems).toHaveLength(3);

      expect(
        screen.queryByRole('link', { name: 'Previous' }),
      ).not.toBeInTheDocument();

      // Page 1
      expect(links[0]).toHaveAttribute('aria-label', 'Page 1');
      expect(links[0]).toHaveAttribute('href', '/test-url?page=1');
      expect(links[0]).toHaveAttribute('aria-current');
      expect(links[0]).toHaveTextContent('1');

      // Spacer
      expect(listItems[1]).toHaveTextContent('…');

      // Page 2
      expect(
        screen.queryByRole('link', { name: 'Page 2' }),
      ).not.toBeInTheDocument();

      // Page 3
      expect(links[1]).toHaveAttribute('aria-label', 'Page 3');
      expect(links[1]).toHaveAttribute('href', '/test-url?page=3');
      expect(links[1]).not.toHaveAttribute('aria-current');
      expect(links[1]).toHaveTextContent('3');

      // Next link
      expect(links[2]).toHaveAttribute('rel', 'next');
      expect(links[2]).toHaveAttribute('href', '/test-url?page=2');
      expect(links[2]).toHaveTextContent('Next');
    });

    test('does not render the Next link when the last page is selected', () => {
      render({ currentPage: 3, totalPages: 3 });

      expect(
        screen.getByRole('navigation', { name: 'Pagination' }),
      ).toBeInTheDocument();

      const links = screen.getAllByRole('link');
      expect(links).toHaveLength(3);

      const listItems = screen.getAllByRole('listitem');
      expect(listItems).toHaveLength(3);

      expect(
        screen.queryByRole('link', { name: 'Next' }),
      ).not.toBeInTheDocument();

      // Previous link
      expect(links[0]).toHaveAttribute('rel', 'prev');
      expect(links[0]).toHaveAttribute('href', '/test-url?page=2');
      expect(links[0]).toHaveTextContent('Previous');

      // Page 1
      expect(links[1]).toHaveAttribute('aria-label', 'Page 1');
      expect(links[1]).toHaveAttribute('href', '/test-url?page=1');
      expect(links[1]).not.toHaveAttribute('aria-current');
      expect(links[1]).toHaveTextContent('1');

      // Page 2
      expect(
        screen.queryByRole('link', { name: 'Page 2' }),
      ).not.toBeInTheDocument();

      // Page 3
      expect(links[2]).toHaveAttribute('aria-label', 'Page 3');
      expect(links[2]).toHaveAttribute('href', '/test-url?page=3');
      expect(links[2]).toHaveAttribute('aria-current');
      expect(links[2]).toHaveTextContent('3');
    });

    test('does not render if there are no pages', () => {
      render({ currentPage: 1, totalPages: 0 });

      expect(
        screen.queryByRole('navigation', { name: 'Pagination' }),
      ).not.toBeInTheDocument();
    });

    test('does not render if there is only one page', () => {
      render({ currentPage: 1, totalPages: 1 });

      expect(
        screen.queryByRole('navigation', { name: 'Pagination' }),
      ).not.toBeInTheDocument();
    });
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
      baseUrl="/test-url"
      currentPage={currentPage}
      // eslint-disable-next-line jsx-a11y/anchor-has-content, react/jsx-props-no-spreading
      renderLink={props => <a {...props} href={props.to} />}
      totalPages={totalPages}
    />,
  );
}
