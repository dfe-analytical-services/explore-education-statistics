import Header from '@common/modules/table-tool/utils/Header';
import { ExpandedHeader } from '@common/modules/table-tool/utils/mapTableToJson';

/**
 * Create the column headers for the table.
 * We 'expand' our headers so that we create the real table
 * cells we need to render in array format (instead of a tree).
 * Duplicate and empty headers are removed and remaining headers are
 * expanded to ensure the table layout is correct.
 */
export default function createExpandedColumnHeaders(
  columnHeaders: Header[],
): ExpandedHeader[][] {
  const acc: ExpandedHeader[][] = [];

  // To construct these headers, we use a breadth-first
  // search algorithm, consequently, we need a 'queue' to
  // track the correct order of header nodes as we process
  // them (queues have first in, first out ordering).
  const queue = [...columnHeaders];

  let currentDepth = 0;
  let row: ExpandedHeader[] = [];

  while (queue.length > 0) {
    const current = queue.shift();

    if (!current) {
      break;
    }

    // We're moving to the next level of the tree, so
    // we are now finished on the last row and should push it.
    if (currentDepth !== current.depth) {
      acc.push(row);

      row = [];
      currentDepth = current.depth;
    }

    const { parent } = current;

    // If the current header's parent appears identical to it,
    // the parent will have a cross span higher than 1.
    // In this case, we shouldn't add the current header to the row
    // as the parent will have already been added to the row above
    // and is supposed to be merged with the current header.
    if (!parent || parent?.crossSpan === 1) {
      row.push({
        id: current.id,
        text: current.text,
        span: current.span,
        isGroup: current.hasChildren(),
        crossSpan: current.crossSpan,
      });
    }

    if (current.text && current.hasChildren()) {
      queue.push(...current.children);
    }

    // There are no more children to iterate
    // through so push the final row.
    if (queue.length === 0 && row.length) {
      acc.push(row);
      row = [];
    }
  }

  return acc;
}
