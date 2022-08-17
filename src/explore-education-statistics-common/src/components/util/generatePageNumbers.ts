interface Options {
  currentPage: number;
  maxPageNumbers?: number;
  offset?: number;
  totalPages: number;
}

export default function generatePageNumbers({
  currentPage,
  maxPageNumbers = 8,
  offset = 2,
  totalPages,
}: Options): (number | null)[] {
  // return all if less than the max
  if (totalPages <= maxPageNumbers) {
    return new Array(totalPages).fill(null).map((_, i) => i + 1);
  }

  const offsetNumber =
    currentPage <= offset || currentPage > totalPages - offset
      ? offset
      : offset - 1;

  const pageNumbers = [1]; // always have the first page
  const pageNumbersWithSpacers: (number | null)[] = [];

  const startCount = currentPage - offsetNumber;
  const endCount = currentPage + offsetNumber;

  for (let i = startCount; i <= endCount; i += 1) {
    if (i < totalPages && i > 1) {
      pageNumbers.push(i);
    }
  }
  pageNumbers.push(totalPages); // always have the last page

  // Add null spacers
  pageNumbers.reduce((acc, current) => {
    if (acc === 1) {
      pageNumbersWithSpacers.push(acc);
    }
    if (current - acc !== 1) {
      pageNumbersWithSpacers.push(null);
    }
    pageNumbersWithSpacers.push(current);

    return current;
  });

  return pageNumbersWithSpacers;
}
