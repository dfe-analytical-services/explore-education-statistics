import classNames from 'classnames';
import times from 'lodash/times';
import React, { forwardRef } from 'react';
import styles from './MultiHeaderTable.module.scss';

export interface RowGroup {
  label: string;
  rows: {
    label: string;
    columnGroups: string[][];
  }[];
}

interface Props {
  ariaHidden?: boolean;
  caption: string;
  className?: string;
  headers: string[][];
  isStickyHeader?: boolean;
  isStickyColumn?: boolean;
  rowGroups: RowGroup[];
}

const MultiHeaderTable = forwardRef<HTMLTableElement, Props>(
  (
    {
      ariaHidden,
      className,
      caption,
      headers,
      isStickyColumn,
      isStickyHeader,
      rowGroups,
    },
    ref,
  ) => {
    const maxColSpan = headers.reduce(
      (total, header) => total * header.length,
      1,
    );

    const getColSpan = (headerRowIndex: number) => {
      return headers
        .slice(headerRowIndex + 1)
        .reduce((total, row) => total * row.length, 1);
    };

    return (
      <table
        aria-hidden={ariaHidden}
        aria-labelledby={caption}
        className={classNames('govuk-table', className)}
        ref={ref}
      >
        <thead>
          {headers.map((columns, rowIndex) => {
            const colSpan = getColSpan(rowIndex) || 1;
            const isColGroup = rowIndex !== headers.length - 1;

            return (
              // eslint-disable-next-line react/no-array-index-key
              <tr key={rowIndex}>
                {rowIndex === 0 && (
                  <th
                    colSpan={2}
                    rowSpan={headers.length}
                    className={classNames(
                      styles.borderBottom,
                      styles.borderRight,
                    )}
                  />
                )}

                {!isStickyColumn &&
                  times(maxColSpan / (colSpan * columns.length), () =>
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
                          scope={colSpan > 1 ? 'colgroup' : 'col'}
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

        {rowGroups.map((group, groupIndex) => {
          const groupKey = `${group.label}-${groupIndex}`;

          return (
            !isStickyHeader && (
              <tbody key={groupKey} className={styles.borderBottom}>
                {group.rows.map((row, rowIndex) => {
                  const rowKey = `${groupKey}_${row.label}_${rowIndex}`;

                  return (
                    <tr key={rowKey}>
                      {rowIndex === 0 && (
                        <th
                          scope="rowgroup"
                          rowSpan={group.rows.length || 1}
                          className={classNames(styles.borderBottom)}
                        >
                          {group.label}
                        </th>
                      )}
                      <th
                        scope="row"
                        className={classNames(
                          'govuk-table__cell--numeric',
                          styles.borderRight,
                        )}
                      >
                        {row.label}
                      </th>
                      {!isStickyColumn &&
                        row.columnGroups.flatMap((colGroup, colGroupIndex) =>
                          colGroup.map((column, columnIndex) => {
                            const cellKey = `${rowKey}_${colGroupIndex}_${column}_${columnIndex}`;

                            return (
                              <td
                                className={classNames(
                                  'govuk-table__cell--numeric',
                                  {
                                    [styles.borderLeft]:
                                      columnIndex === 0 && colGroupIndex > 0,
                                  },
                                )}
                                key={cellKey}
                              >
                                {column}
                              </td>
                            );
                          }),
                        )}
                    </tr>
                  );
                })}
              </tbody>
            )
          );
        })}
      </table>
    );
  },
);

MultiHeaderTable.displayName = 'MultiHeaderTable';

export default MultiHeaderTable;
