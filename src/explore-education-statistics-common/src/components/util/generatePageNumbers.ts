import last from 'lodash/last';
import range from 'lodash/range';

interface Options {
  currentPage: number;
  maxReturnSize?: number;
  totalPages: number;
}

/**
 * Generates pages for pagination.
 * Always returns the first and last page. Returns pages around the current page and
 * null spacers in gaps between consecutive numbers.
 * @param currentPage
 * @param maxReturnSize - the maximum number of items to return.
 * Can be used to customise the length based on the space available,
 * the default, 7, fits in 'govuk-grid-column-two-thirds'.
 * @param totalPages
 */
export default function generatePageNumbers({
  currentPage: initialCurrentPage,
  maxReturnSize: initialMaxReturnSize = 7,
  totalPages,
}: Options): (number | null)[] {
  // Minimum return size of 5, to allow [first, spacer, current, spacer, last]
  const minReturnSize = 5;
  const maxReturnSize =
    initialMaxReturnSize < minReturnSize ? minReturnSize : initialMaxReturnSize;

  // Return an empty array if 0 or 1 pages
  if (totalPages <= 1) {
    return [];
  }

  // Return all pages if the total is less than maxReturnSize
  if (totalPages <= maxReturnSize) {
    return range(1, totalPages + 1);
  }

  // Make sure the current page is valid
  const currentPage =
    initialCurrentPage === 0 ? 1 : Math.min(initialCurrentPage, totalPages);

  const { rangeStart, rangeEnd } = getPagesRange({
    currentPage,
    maxReturnSize,
    totalPages,
  });

  const pageNumbers = [1, ...range(rangeStart, rangeEnd + 1), totalPages];

  return pageNumbers.reduce<(number | null)[]>((acc, current) => {
    const prev = last(acc);
    if (typeof prev === 'number' && current - prev !== 1) {
      acc.push(null);
    }
    acc.push(current);
    return acc;
  }, []);
}

function getPagesRange({
  currentPage,
  maxReturnSize,
  totalPages,
}: {
  currentPage: number;
  maxReturnSize: number;
  totalPages: number;
}) {
  // The first and the last numbers are added later to ensure they are always shown
  const startNumber = 2;
  const endNumber = totalPages - 1;

  // Number of items (pages and spacers) to show around the current page,
  // based on what fits up to the maxReturnSize.
  // 3 is the first, last and current pages.
  const itemsAroundCurrent = maxReturnSize - 3;
  // Number of items (pages and spacers) on either side of the current page.
  const offsetItems = Math.floor(itemsAroundCurrent / 2);
  const midPoint = Math.floor(maxReturnSize / 2);

  // currentPage is near the start
  if (
    currentPage - startNumber < midPoint &&
    currentPage <= startNumber + offsetItems
  ) {
    return {
      rangeStart: startNumber,
      rangeEnd: startNumber + (itemsAroundCurrent - 1),
    };
  }

  // currentPage is near the end
  if (endNumber - currentPage < midPoint) {
    return {
      rangeStart: endNumber - (itemsAroundCurrent - 1),
      rangeEnd: endNumber,
    };
  }

  // currentPage is in the middle
  return {
    rangeStart: currentPage - offsetItems + 1,
    rangeEnd: currentPage + offsetItems - 1,
  };
}
