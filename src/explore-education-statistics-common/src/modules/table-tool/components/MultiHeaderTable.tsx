import classNames from 'classnames';
import times from 'lodash/times';
import React, { forwardRef, ReactElement } from 'react';
import styles from './MultiHeaderTable.module.scss';

interface Props {
  ariaLabelledBy?: string;
  className?: string;
  columnHeaders: string[][];
  rowHeaders: string[][];
  rows: string[][];
}

const MultiHeaderTable = forwardRef<HTMLTableElement, Props>(
  ({ ariaLabelledBy, className, columnHeaders, rowHeaders, rows }, ref) => {
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
        aria-labelledby={ariaLabelledBy}
        className={classNames('govuk-table', styles.table, className)}
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

                {times(totalColumns / (colSpan * columns.length), () =>
                  columns.map((column, columnIndex) => {
                    const key = `${column}_${columnIndex}`;

                    return (
                      <th
                        className={classNames({
                          'govuk-table__header--numeric': !isColGroup,
                          'govuk-table__header--center': isColGroup,
                          [styles.borderBottom]: !isColGroup,
                          [styles.borderRight]:
                            columnIndex === columns.length - 1 || isColGroup,
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
                        scope={isRowGroup ? 'rowgroup' : 'row'}
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
                          [styles.borderRight]:
                            (cellIndex + 1) % firstColSpan === 0,
                          [styles.borderBottom]:
                            (rowIndex + 1) % firstRowSpan === 0,
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
