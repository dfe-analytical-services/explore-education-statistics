import Header from '@common/modules/table-tool/utils/Header';
import { TableCellJson } from '@common/modules/table-tool/utils/mapTableToJson';

/**
 * Create the column headers for the table.
 * We 'expand' our headers so that we create the real table
 * cells we need to render in array format (instead of a tree).
 * Duplicate and empty headers are removed and remaining headers are
 * expanded to ensure the table layout is correct.
 */
export default function createExpandedColumnHeaders(
  columnHeaders: Header[],
): TableCellJson[][] {
  const maxDepth = columnHeaders.reduce((acc, header) => {
    const { maxCrossSpan } = header;
    return maxCrossSpan > acc ? maxCrossSpan : acc;
  }, 0);

  const acc: TableCellJson[][] = [];

  // To construct these headers, we use a breadth-first
  // search algorithm, consequently, we need a 'queue' to
  // track the correct order of header nodes as we process
  // them (queues have first in, first out ordering).
  const queue = [...columnHeaders];

  let currentDepth = 0;
  let row: TableCellJson[] = [];

  while (queue.length > 0) {
    const current = queue.shift();

    if (!current) {
      break;
    }

    // We're moving to the next level of the tree, so
    // we are now finished on the last row and should push it.
    if (currentDepth !== current.depth) {
      if (row.length) {
        acc.push(adjustRow(row));
      }

      row = [];
      currentDepth = current.depth;
    }

    const { crossSpan, parent } = current;

    // If the current header's parent appears identical to it,
    // the parent will have a cross span higher than 1.
    // In this case, we shouldn't add the current header to the row
    // as the parent will have already been added to the row above
    // and is supposed to be merged with the current header.
    if (!parent || parent?.crossSpan === 1) {
      row.push({
        colSpan: current.span,
        rowSpan: current.crossSpan,
        scope: maxDepth > currentDepth + crossSpan ? 'colgroup' : 'col',
        text: current.text,
        tag: 'th',
      });
    }

    if (current.text && current.hasChildren()) {
      queue.push(...current.children);
    }

    // There are no more children to iterate
    // through so push the final row.
    if (queue.length === 0 && row.length) {
      acc.push(adjustRow(row));
      row = [];
    }
  }

  return acc;
}

// Adjust the rowSpan for cells in rows where all cells are merged.
function adjustRow(row: TableCellJson[]): TableCellJson[] {
  const allMerged = row.every(cell => cell.rowSpan === 2);
  return row.map(cell => ({
    ...cell,
    rowSpan: allMerged ? 1 : cell.rowSpan,
  }));
}
