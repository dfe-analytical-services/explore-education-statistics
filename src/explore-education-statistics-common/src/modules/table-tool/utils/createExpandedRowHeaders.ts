import Header from '@common/modules/table-tool/utils/Header';
import last from 'lodash/last';
import { ExpandedHeader } from './mapTableToJson';

/**
 * TODO: - add description
 * @param rowHeaders
 * @returns
 */
export default function createExpandedRowHeaders(
  rowHeaders: Header[],
): ExpandedHeader[][] {
  const headers = rowHeaders.reduce<ExpandedHeader[][]>((acc, header) => {
    // To construct these headers, we use a depth-first
    // search algorithm. This requires a 'stack' data-structure to
    // track the correct order of header nodes as we process
    // them (stacks have last in, first out ordering).
    const stack = [header];

    let row: ExpandedHeader[] = [];

    while (stack.length > 0) {
      const current = stack.shift();

      if (!current) {
        break;
      }

      const prev = last(row);

      // do we have text in the previous header and does it match the current header?
      const matchesPreviousHeader = prev?.text && prev.text === current.text;

      // detect secnario where we're at the root node

      // Add the current header to the row when:
      // - it doesn't match the previous header
      // - it does match the previous header, but it's in a sub-group with
      //   siblings so needs to be included or the layout breaks.
      // - there is some text
      //
      // Otherwise, we want the previous header to span
      // across where the current header would be in the row.

      if (
        current.text &&
        (!matchesPreviousHeader ||
          (matchesPreviousHeader && current.hasSiblings()))
      ) {
        const isGroup = current.hasChildren();

        row.push({
          id: current.id,
          text: current.text,
          span: current.span,
          isGroup,
          crossSpan: isGroup
            ? current.crossSpan
            : calculateLastChildCrossSpan(current),
        });
      } else if (!current.hasChildren() && prev && prev.crossSpan > 1) {
        // This one is a bit weird, but we have to directly update
        // the previous header's `isGroup` to allow the header
        // to have `scope="row"` in the table i.e. it's the
        // header cell directly adjacent to non-header cells.
        prev.isGroup = false;
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
        const prevSpan = last(last(acc))?.span ?? 0;
        const index = acc.length > 0 ? acc.length - 1 + prevSpan : 0;

        acc[index] = row;

        row = [];
      }
    }

    return acc;
  }, []);

  // if there is only one header in the row AND the cross span is below the width of the columns then we can collapse it down to a single header

  // Do some more checks on the headers to check if we can collapse any down
  // to a single header.

  // We can collapse headers down to a single header if:
  // - the crosspan is below the width of the columns (the header is by itself in a column)

  return headers;
}

function calculateLastChildCrossSpan(header: Header): number {
  const { parent } = header;
  if (header.text === parent?.text && parent.text === parent?.parent?.text) {
    return 1;
  }

  if (header.parent && header.parent.crossSpan > 1) {
    // it's already being front-filled so no point going from children to parent node (as it's already been done)
    return 1;
  }

  // back fill here to see if we can find a parent that has the same text
  let current: Header | undefined = header;
  let crossSpan = 1;

  while (current) {
    if (
      current.text === current.parent?.text ||
      (current.parent && !current.parent?.text)
    ) {
      crossSpan += 1;
      current = current.parent;
    } else {
      break;
    }
  }

  return crossSpan;
}
