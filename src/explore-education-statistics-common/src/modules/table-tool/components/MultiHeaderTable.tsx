import { OmitStrict } from '@common/types';
import classNames from 'classnames';
import last from 'lodash/last';
import omit from 'lodash/omit';
import React, { forwardRef, ReactNode } from 'react';
import styles from './MultiHeaderTable.module.scss';

export interface Header {
  id: string;
  text: string;
  /**
   * The number of cells that
   * this header should span.
   */
  span: number;
  /**
   * The starting cell index in
   * the row/col direction.
   */
  start: number;
  /**
   * Headers may have extra subgroups.
   * These can be used to provide more
   * context.
   *
   * They are expanded as additional
   * rowgroup/colgroups in the table.
   */
  group?: string;
}

interface ExpandedHeader extends OmitStrict<Header, 'group'> {
  crossSpan: number;
  isHidden: boolean;
  isGroup: boolean;
}

export interface MultiHeaderTableProps {
  ariaLabelledBy?: string;
  className?: string;
  columnHeaders: Header[][];
  rowHeaders: Header[][];
  rows: string[][];
}

const MultiHeaderTable = forwardRef<HTMLTableElement, MultiHeaderTableProps>(
  ({ ariaLabelledBy, className, columnHeaders, rowHeaders, rows }, ref) => {
    const createExpandedHeaders = (
      headerGroups: Header[][],
    ): ExpandedHeader[][] => {
      return headerGroups.reduce<ExpandedHeader[][]>(
        (acc, headerGroup, headerGroupIndex) => {
          const isLastHeaderGroup =
            headerGroupIndex === headerGroups.length - 1;

          // Insert a new set of rowgroup/colgroups for
          // header groups with subgroups defined
          const hasExtraHeaderGroups = headerGroup.some(group => group.group);

          if (hasExtraHeaderGroups) {
            acc.push(
              headerGroup.reduce<ExpandedHeader[]>(
                (subHeaders, { group, ...header }) => {
                  const prevSubHeader = last(subHeaders);

                  // Increase the span size of the previous header.
                  if (prevSubHeader && prevSubHeader?.id === group) {
                    prevSubHeader.span += header.span;

                    // Previous header can't simultaneously have spans in
                    // both directions. This breaks the HTML table, so we
                    // convert it back to a normal rowgroup/colgroup.
                    if (prevSubHeader.crossSpan > 1) {
                      prevSubHeader.crossSpan = 1;
                      prevSubHeader.isGroup = true;
                    }
                  } else {
                    const hasMatchingChild = header.text === group;

                    subHeaders.push({
                      ...header,
                      id: group ?? '',
                      text: group ?? '',
                      crossSpan: hasMatchingChild ? 2 : 1,
                      isGroup: hasMatchingChild ? !isLastHeaderGroup : true,
                      isHidden: false,
                    });
                  }

                  return subHeaders;
                },
                [],
              ),
            );
          }

          acc.push(
            headerGroup.map(header => {
              const prevHeaderGroup = last(acc);

              const hasMatchingParent =
                (hasExtraHeaderGroups &&
                  prevHeaderGroup?.some(
                    parentHeader =>
                      parentHeader.start <= header.start &&
                      header.start <= parentHeader.start + parentHeader.span &&
                      parentHeader.text === header.text &&
                      parentHeader.crossSpan === 2,
                  )) ??
                false;

              return {
                ...omit(header, 'group'),
                crossSpan: 1,
                isHidden: hasMatchingParent,
                isGroup: !isLastHeaderGroup,
              };
            }),
          );

          return acc;
        },
        [],
      );
    };

    const expandedRowHeaders = createExpandedHeaders(rowHeaders);
    const expandedColumnHeaders = createExpandedHeaders(columnHeaders);

    const firstRowHeaderLength = expandedRowHeaders[0].length;

    // Expanded headers need to be transposed so that
    // they are compatible with HTML table rows.
    const transposeToRows = (
      headerGroups: ExpandedHeader[][],
    ): ExpandedHeader[][] => {
      const totalRows = last(headerGroups)?.length ?? 0;

      const reversedHeaderGroups = headerGroups.map(headers =>
        [...headers].reverse(),
      );

      return [...Array(totalRows)].map((_, rowIndex) => {
        return reversedHeaderGroups
          .map(headerGroup =>
            headerGroup.find(header => header.start === rowIndex),
          )
          .filter(Boolean);
      }) as ExpandedHeader[][];
    };

    const transposedRowHeaders = transposeToRows(
      expandedRowHeaders,
    ) as ExpandedHeader[][];

    return (
      <table
        aria-labelledby={ariaLabelledBy}
        className={classNames('govuk-table', styles.table, className)}
        ref={ref}
      >
        <thead>
          {expandedColumnHeaders.map((columns, rowIndex) => {
            return (
              // eslint-disable-next-line react/no-array-index-key
              <tr key={rowIndex}>
                {rowIndex === 0 && (
                  <td
                    colSpan={expandedRowHeaders.length}
                    rowSpan={expandedColumnHeaders.length}
                    className={styles.borderBottom}
                  />
                )}

                {columns
                  .filter(column => !column.isHidden)
                  .map((column, columnIndex) => {
                    const key = `${column.text}_${columnIndex}`;

                    return (
                      <th
                        className={classNames({
                          'govuk-table__header--numeric': !column.isGroup,
                          [styles.columnHeaderGroup]:
                            column.isGroup || column.crossSpan > 1,
                          [styles.borderBottom]: !column.isGroup,
                          [styles.borderRightNone]:
                            column.start + column.span === rows[0].length,
                        })}
                        colSpan={column.span}
                        rowSpan={column.crossSpan}
                        scope={column.isGroup ? 'colgroup' : 'col'}
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
          {transposedRowHeaders
            .reduce((acc, rowGroup, headerGroupIndex) => {
              acc.push(
                rowGroup
                  .filter(row => !row.isHidden)
                  .map((row, rowIndex) => {
                    return (
                      <th
                        // eslint-disable-next-line react/no-array-index-key
                        key={`${rowIndex}_${headerGroupIndex}`}
                        className={classNames({
                          [styles.borderBottom]: row.isGroup,
                        })}
                        rowSpan={row.span}
                        colSpan={row.crossSpan}
                        scope={row.isGroup ? 'rowgroup' : 'row'}
                      >
                        {row.text}
                      </th>
                    );
                  }),
              );

              return acc;
            }, [] as ReactNode[][])
            .map((headerCells, rowIndex) => {
              return (
                // eslint-disable-next-line react/no-array-index-key
                <tr key={rowIndex}>
                  {headerCells}
                  <>
                    {rows[rowIndex].map((cell, cellIndex) => (
                      <td
                        // eslint-disable-next-line react/no-array-index-key
                        key={cellIndex}
                        className={classNames('govuk-table__cell--numeric', {
                          [styles.borderBottom]:
                            (rowIndex + 1) % firstRowHeaderLength === 0,
                        })}
                      >
                        {cell}
                      </td>
                    ))}
                  </>
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
