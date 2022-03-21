import Header from '@common/modules/table-tool/components/utils/Header';
import classNames from 'classnames';
import last from 'lodash/last';
import sumBy from 'lodash/sumBy';
import React, { forwardRef, useMemo } from 'react';
import styles from './MultiHeaderTable.module.scss';

interface ExpandedHeader {
  id: string;
  text: string;
  span: number;
  crossSpan: number;
  isGroup: boolean;
}

export interface MultiHeaderTableProps {
  ariaLabelledBy?: string;
  className?: string;
  columnHeaders: Header[];
  rowHeaders: Header[];
  rows: string[][];
}

const MultiHeaderTable = forwardRef<HTMLTableElement, MultiHeaderTableProps>(
  ({ ariaLabelledBy, className, columnHeaders, rowHeaders, rows }, ref) => {
    // We 'expand' our headers so that we create the real table
    // cells we need to render in array format (instead of a tree).
    const expandedColumnHeaders = useMemo(() => {
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

        if (current.hasChildren()) {
          queue.push(...current.children);
        }

        // There are no more children to iterate
        // through so push the final row.
        if (queue.length === 0) {
          acc.push(row);
          row = [];
        }
      }

      return acc;
    }, [columnHeaders]);

    const expandedRowHeaders = useMemo(() => {
      return rowHeaders.reduce<ExpandedHeader[][]>((acc, header) => {
        // To construct these headers, we use a depth-first
        // search algorithm. This requires a 'stack' to
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

          // We only add the current header to the row if we know
          // that the previous header doesn't match it.
          // Otherwise, we want the previous header to span
          // across where the current header would be in the row.
          if (prev?.text !== current?.text || prev?.crossSpan === 1) {
            row.push({
              id: current.id,
              text: current.text,
              span: current.span,
              isGroup: current.hasChildren(),
              crossSpan: current.crossSpan,
            });
          } else if (!current.hasChildren() && prev.crossSpan > 1) {
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
    }, [rowHeaders]);

    const rowHeaderColumnLength = sumBy(
      expandedRowHeaders[0],
      header => header.crossSpan,
    );

    return (
      <table
        data-testid={ariaLabelledBy && `${ariaLabelledBy}-table`}
        aria-labelledby={ariaLabelledBy}
        className={classNames('govuk-table', styles.table, className)}
        ref={ref}
      >
        <thead className={styles.tableHead}>
          {expandedColumnHeaders.map((columns, rowIndex) => {
            return (
              // eslint-disable-next-line react/no-array-index-key
              <tr key={rowIndex}>
                {rowIndex === 0 && (
                  <td
                    colSpan={rowHeaderColumnLength}
                    rowSpan={expandedColumnHeaders.length}
                    className={styles.borderBottom}
                  />
                )}

                {columns.map((column, columnIndex) => {
                  const key = `${column.text}_${columnIndex}`;

                  return (
                    <th
                      colSpan={column.span}
                      rowSpan={column.crossSpan}
                      scope={
                        rowIndex + column.crossSpan !==
                        expandedColumnHeaders.length
                          ? 'colgroup'
                          : 'col'
                      }
                      key={key}
                    >
                      {column.text}
                    </th>
                  );
                })}
              </tr>
            );
          })}
        </thead>

        <tbody>
          {rows.map((row, rowIndex) => (
            // eslint-disable-next-line react/no-array-index-key
            <tr key={rowIndex}>
              {expandedRowHeaders[rowIndex]?.map((header, headerIndex) => {
                return (
                  <th
                    // eslint-disable-next-line react/no-array-index-key
                    key={headerIndex}
                    className={classNames({
                      [styles.borderBottom]: header.isGroup,
                    })}
                    rowSpan={header.span}
                    colSpan={header.crossSpan}
                    scope={header.isGroup ? 'rowgroup' : 'row'}
                  >
                    {header.text}
                  </th>
                );
              })}

              {row.map((cell, cellIndex) => (
                <td
                  // eslint-disable-next-line react/no-array-index-key
                  key={cellIndex}
                  className={classNames('govuk-table__cell--numeric', {
                    [styles.borderBottom]:
                      (rowIndex + 1) % rowHeaders.length === 0,
                  })}
                >
                  {cell}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    );
  },
);

MultiHeaderTable.displayName = 'MultiHeaderTable';

export default MultiHeaderTable;
