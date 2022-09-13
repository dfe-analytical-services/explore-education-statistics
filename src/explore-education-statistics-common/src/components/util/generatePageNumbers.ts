import last from 'lodash/last';
import range from 'lodash/range';

type WindowSizes = 's' | 'm';

interface Options {
  /**
   * The default size, 'm', fits in 'govuk-grid-column-two-thirds' on desktop,
   * 's' fits on mobile.
   */
  windowSize?: WindowSizes;
  currentPage: number;
  totalPages: number;
}

/**
 * Generates pages for pagination.
 * Always returns the first and last page.
 * On desktop (size 'm') it returns pages around the current page and
 * null spacers in gaps between consecutive numbers.
 * On mobile (size 's') it returns the current page and spacers
 */
export default function generatePageNumbers({
  windowSize = 'm',
  currentPage: initialCurrentPage,
  totalPages,
}: Options): (number | null)[] {
  // Return an empty array if 0 or 1 pages
  if (totalPages <= 1) {
    return [];
  }

  // Make sure the current page is valid
  const currentPage =
    initialCurrentPage <= 0 ? 1 : Math.min(initialCurrentPage, totalPages);

  const pagesRange = getPagesRange({
    windowSize,
    currentPage,
    totalPages,
  });

  const pageNumbers = [1, ...pagesRange, totalPages];

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
  windowSize,
  currentPage,
  totalPages,
}: {
  windowSize: WindowSizes;
  currentPage: number;
  totalPages: number;
}) {
  // For small windows we only return the current page
  // or an empty array if it's the first or last page.
  if (windowSize === 's') {
    return currentPage === 1 || currentPage === totalPages ? [] : [currentPage];
  }

  // The first and the last numbers are added later to ensure they are always shown.
  const startNumber = 2;
  const endNumber = totalPages - 1;
  const maxItems = 7;
  // Return all pages if the total is less than maxItems
  if (totalPages <= maxItems) {
    return range(startNumber, totalPages);
  }

  // Number of items (pages and spacers) to show around the current page,
  // based on what fits up to the maxItems.
  // 3 is the first, last and current pages.
  const itemsAroundCurrent = maxItems - 3;
  // Number of items (pages and spacers) on either side of the current page.
  const offsetItems = Math.floor(itemsAroundCurrent / 2);
  const midPoint = Math.floor(maxItems / 2);

  // currentPage is near the start
  if (
    currentPage - startNumber < midPoint &&
    currentPage <= startNumber + offsetItems
  ) {
    return range(startNumber, startNumber + itemsAroundCurrent);
  }

  // currentPage is near the end
  if (endNumber - currentPage < midPoint) {
    return range(endNumber - (itemsAroundCurrent - 1), endNumber + 1);
  }

  return range(currentPage - offsetItems + 1, currentPage + offsetItems);
}
