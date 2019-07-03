import classNames from 'classnames';
import times from 'lodash/times';
import React, { forwardRef, ReactElement } from 'react';
import styles from './MultiHeaderTable.module.scss';

interface Props {
  ariaHidden?: boolean;
  ariaLabelledBy: string;
  className?: string;
  isStickyHeader?: boolean;
  isStickyColumn?: boolean;
  columnHeaders: string[][];
  rowHeaders: string[][];
  rows: string[][];
}

const MultiHeaderTable = forwardRef<HTMLTableElement, Props>(
  (
    {
      ariaHidden,
      ariaLabelledBy,
      className,
      columnHeaders,
      isStickyColumn,
      isStickyHeader,
      rowHeaders,
      rows,
    },
    ref,
  ) => {
    const getSpan = (groups: string[][], groupIndex: number) => {
      return groups
        .slice(groupIndex + 1)
        .reduce((total, row) => total * row.length, 1);
    };

    const totalColumns = getSpan(columnHeaders, -1);

    const firstColSpan = getSpan(columnHeaders, 0);
    const firstRowSpan = getSpan(rowHeaders, 0);

    return (
      <table
        aria-hidden={ariaHidden}
        aria-labelledby={ariaLabelledBy}
        className={classNames('govuk-table', className)}
        ref={ref}
      >
        <thead>
          {columnHeaders.map((columns, rowIndex) => {
            const colSpan = getSpan(columnHeaders, rowIndex);
            const isColGroup = rowIndex !== columnHeaders.length - 1;

            return (
              // eslint-disable-next-line react/no-array-index-key
              <tr key={rowIndex}>
                {rowIndex === 0 && (
                  <td
                    colSpan={rowHeaders.length}
                    rowSpan={columnHeaders.length}
                    className={classNames(
                      styles.borderBottom,
                      styles.borderRight,
                    )}
                  />
                )}

                {!isStickyColumn &&
                  times(totalColumns / (colSpan * columns.length), () =>
                    columns.map((column, columnIndex) => {
                      const key = `${column}_${columnIndex}`;

                      return (
                        <th
                          className={classNames({
                            'govuk-table__header--numeric': !isColGroup,
                            'govuk-table__header--center': isColGroup,
                            [styles.borderBottom]: !isColGroup,
                            [styles.borderLeft]:
                              columnIndex === 0 || isColGroup,
                          })}
                          colSpan={colSpan}
                          scope={isColGroup ? 'colgroup' : 'col'}
                          key={key}
                        >
                          {column}
                        </th>
                      );
                    }),
                  )}
              </tr>
            );
          })}
        </thead>

        {!isStickyHeader && (
          <tbody>
            {rowHeaders
              .reduce<ReactElement[][]>(
                (acc, headerGroup, headerGroupIndex) =>
                  acc.flatMap((row, rowIndex) =>
                    headerGroup.map((header, index) => {
                      const rowSpan = getSpan(rowHeaders, headerGroupIndex);
                      const isRowGroup =
                        headerGroupIndex !== rowHeaders.length - 1;

                      const headerCell = (
                        <th
                          // eslint-disable-next-line react/no-array-index-key
                          key={`${rowIndex}_${headerGroupIndex}_${index}`}
                          className={classNames({
                            'govuk-table__cell--numeric': !isRowGroup,
                            'govuk-table__cell--center': isRowGroup,
                            [styles.borderRight]: !isRowGroup,
                            [styles.borderBottom]:
                              isRowGroup || index === headerGroup.length - 1,
                          })}
                          rowSpan={rowSpan}
                          role={isRowGroup ? 'rowgroup' : 'row'}
                        >
                          {header}
                        </th>
                      );

                      if (index === 0) {
                        return [...row, headerCell];
                      }

                      return [...row.slice(headerGroupIndex), headerCell];
                    }),
                  ),
                [[]],
              )
              .map((headerCells, rowIndex) => (
                <tr
                  // eslint-disable-next-line react/no-array-index-key
                  key={rowIndex}
                  className={classNames({
                    [styles.borderBottom]: (rowIndex + 1) % firstRowSpan === 0,
                  })}
                >
                  {headerCells}
                  {!isStickyColumn && (
                    <>
                      {rows[rowIndex].map((cell, cellIndex) => (
                        <td
                          // eslint-disable-next-line react/no-array-index-key
                          key={cellIndex}
                          className={classNames('govuk-table__cell--numeric', {
                            [styles.borderLeft]: cellIndex % firstColSpan === 0,
                          })}
                        >
                          {cell}
                        </td>
                      ))}
                    </>
                  )}
                </tr>
              ))}
          </tbody>
        )}
      </table>
    );
  },
);

MultiHeaderTable.displayName = 'MultiHeaderTable';

export default MultiHeaderTable;
