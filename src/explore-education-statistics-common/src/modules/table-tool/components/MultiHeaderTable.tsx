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

          const matchesPreviousHeader = prev?.text === current.text;

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
            const headingRowKey = `row-${rowIndex}`;
            return (
              <tr key={headingRowKey}>
                {rowIndex === 0 && (
                  <td
                    colSpan={rowHeaderColumnLength}
                    rowSpan={expandedColumnHeaders.length}
                    className={styles.borderBottom}
                  />
                )}

                {columns.map((column, columnIndex) => {
                  const key = `${column.text}_${columnIndex}`;
                  // Add an empty td instead of a th for empty group headers
                  if (column.id === '') {
                    return (
                      <td
                        key={key}
                        colSpan={column.span}
                        rowSpan={column.crossSpan}
                        className={styles.emptyColumnHeaderCell}
                      />
                    );
                  }

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
          {rows.map((row, rowIndex) => {
            const rowKey = `row-${rowIndex}`;
            return (
              <tr key={rowKey}>
                {expandedRowHeaders[rowIndex]?.map((header, headerIndex) => {
                  const key = `header-${headerIndex}`;

                  // Add an empty td instead of a th for empty group headers
                  if (header.id === '') {
                    return (
                      <td
                        key={key}
                        className={classNames(styles.emptyRowHeaderCell, {
                          [styles.borderBottom]: header.isGroup,
                        })}
                        rowSpan={header.span}
                        colSpan={header.crossSpan}
                      />
                    );
                  }

                  return (
                    <th
                      key={key}
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

                {row.map((cell, cellIndex) => {
                  const cellKey = `cell-${cellIndex}`;
                  return (
                    <td
                      key={cellKey}
                      className={classNames('govuk-table__cell--numeric', {
                        [styles.borderBottom]:
                          (rowIndex + 1) % rowHeaders.length === 0,
                      })}
                    >
                      {cell}
                    </td>
                  );
                })}
              </tr>
            );
          })}
        </tbody>
      </table>
    );
  },
);

MultiHeaderTable.displayName = 'MultiHeaderTable';

export default MultiHeaderTable;
