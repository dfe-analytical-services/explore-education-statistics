import Header from '@common/modules/table-tool/utils/Header';
import { TableCellJson } from '@common/modules/table-tool/utils/mapTableToJson';
import last from 'lodash/last';

/**
 * Create the row headers for the table.
 * We 'expand' our headers so that we create the real table
 * cells we need to render in array format (instead of a tree).
 * Duplicate and empty headers are removed and remaining headers are
 * expanded to ensure the table layout is correct.
 */
export default function createExpandedRowHeaders(
  rowHeaders: Header[],
): TableCellJson[][] {
  const maxDepth = rowHeaders.reduce((acc, header) => {
    const { maxCrossSpan } = header;
    return maxCrossSpan > acc ? maxCrossSpan : acc;
  }, 0);
  const headers = rowHeaders.reduce<TableCellJson[][]>((acc, header) => {
    // To construct these headers, we use a depth-first
    // search algorithm. This requires a 'stack' data-structure to
    // track the correct order of header nodes as we process
    // them (stacks have last in, first out ordering).
    const stack = [header];

    let row: TableCellJson[] = [];

    while (stack.length > 0) {
      const current = stack.shift();

      if (!current) {
        break;
      }

      const prev = last(row);

      const matchesPreviousHeader = prev?.text && prev.text === current.text;

      // Add the current header to the row when:
      // - it doesn't match the previous header
      // - it does match the previous header, but it's in a sub-group with
      //   siblings so needs to be included or the layout breaks.
      // Otherwise, we want the previous header to span
      // across where the current header would be in the row.
      if (
        !matchesPreviousHeader ||
        (matchesPreviousHeader && current.hasSiblings())
      ) {
        // Check to see if all headers at the current level have only
        // a single matching child so will be merged.
        const headersAtCurrentLevel = current.parent
          ? current.parent.children
          : rowHeaders;
        const allHeadersAtCurrentLevelWillBeMerged = headersAtCurrentLevel.every(
          rowHeader => rowHeader.hasSingleMatchingChild(),
        );

        row.push({
          text: current.text,
          rowSpan: current.span,
          scope:
            maxDepth > current.depth + current.crossSpan ? 'rowgroup' : 'row',
          colSpan: allHeadersAtCurrentLevelWillBeMerged ? 1 : current.crossSpan,
          tag: 'th',
        });
      } else if (
        !current.hasChildren() &&
        prev &&
        prev.colSpan &&
        prev.colSpan > 1
      ) {
        // This one is a bit weird, but we have to directly update
        // the previous header's `isGroup` to allow the header
        // to have `scope="row"` in the table i.e. it's the
        // header cell directly adjacent to non-header cells.
        prev.scope = 'row';
      }

      if (current.hasChildren()) {
        stack.unshift(...current.children);
      } else {
        // The following is a bit of an edge case, but it's worth handling.
        // We get the previous row's final header span so that we can
        // determine if the previous row is going to span more than
        // one row across all of its headers.
        // This means that these following row positions should be
        // completely empty and we want to avoid placing our current
        // row into any of these positions.
        const prevSpan = last(last(acc))?.rowSpan ?? 0;

        const index = acc.length > 0 ? acc.length - 1 + prevSpan : 0;

        acc[index] = row;
        row = [];
      }
    }

    return acc;
  }, []);

  return headers;
}
