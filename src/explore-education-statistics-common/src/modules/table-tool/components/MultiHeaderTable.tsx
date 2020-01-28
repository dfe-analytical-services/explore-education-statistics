import classNames from 'classnames';
import React, { forwardRef, ReactElement } from 'react';
import { times } from 'lodash';
import styles from './MultiHeaderTable.module.scss';

export type RowHeaderType = string | undefined;

interface Props {
  ariaLabelledBy?: string;
  className?: string;
  columnHeaders: RowHeaderType[][];
  columnHeaderIsGroup?: boolean[];
  rowHeaders: RowHeaderType[][];
  rowHeaderIsGroup?: boolean[];
  rows: string[][];
}

export interface SpanInfo {
  heading: string;
  count: number;
  start: number;
  isGroup: boolean;
}

const grow2DArray = <T, R>(
  finalGroups: T[][],
  numberOfTimesToRepeat: number,
  emptySpace: R,
): (T | R)[][] =>
  finalGroups.map(finalRow =>
    ([] as (T | R)[]).concat(
      ...finalRow.map(cell =>
        [...Array(numberOfTimesToRepeat)].map((_, idx) =>
          idx === 0 ? cell : emptySpace,
        ),
      ),
    ),
  );

const generateSpanInfoForSingleGroup = (
  headingsForGroup: RowHeaderType[],
  overallHeaderLength: number,
  isRowGroup: boolean,
) => {
  return headingsForGroup.reduce<SpanInfo[]>(
    (spanInfo: SpanInfo[], nextHeading: RowHeaderType, rowIndex: number) => {
      if (spanInfo.length === 0) {
        return [
          {
            heading: nextHeading || '',
            count: overallHeaderLength,
            start: rowIndex,
            isGroup: isRowGroup,
          },
        ];
      }

      const previousHeading = spanInfo[spanInfo.length - 1];

      if (nextHeading === undefined) {
        return spanInfo;
      }

      return [
        ...spanInfo.slice(0, -1),
        {
          ...previousHeading,
          count: rowIndex - previousHeading.start,
        },
        {
          heading: nextHeading,
          count: overallHeaderLength - rowIndex,
          start: rowIndex,
          isGroup: isRowGroup,
        },
      ];
    },
    [],
  );
};

export const generateHeaderSpanInfo = (headerGroups: RowHeaderType[][]) => {
  const overallHeaderLength = headerGroups.reduce(
    (curMaxLength: number, group) =>
      curMaxLength < group.length ? group.length : curMaxLength,
    0,
  );

  return headerGroups.reduce<SpanInfo[][]>(
    (
      headerSpanInfo: SpanInfo[][],
      headingsForGroup: RowHeaderType[],
      groupIndex: number,
    ) => [
      ...headerSpanInfo,
      generateSpanInfoForSingleGroup(
        headingsForGroup,
        overallHeaderLength,
        groupIndex < headerGroups.length - 1,
      ),
    ],
    [],
  );
};

export const transposeSpanColumnsToRows = (
  headers: SpanInfo[][],
): SpanInfo[][] => {
  const lengthOfRow = headers[0].reduce(
    (length, { count }) => length + count,
    0,
  );

  const rowColumnsReversed = headers.map(column => [...column].reverse());

  return [...Array(lengthOfRow)].map((_, currentRow) =>
    rowColumnsReversed
      .map(rowColumn => rowColumn.find(({ start }) => start === currentRow))
      .filter(cell => !!cell),
  ) as SpanInfo[][];
};

export const generateAggregatedGroups = (
  headerGroups: RowHeaderType[][],
  headerIsGroup: boolean[] = [],
): RowHeaderType[][] => {
  const aggrigatedDuplication = headerGroups.reduce<{
    finalGroups: RowHeaderType[][];
    total: number;
  }>(
    ({ finalGroups, total }, groups, index) => {
      const numberOfTimesToRepeat =
        index > 0 && headerIsGroup[index - 1] ? 1 : groups.length;

      const expandedPreviousGroups = grow2DArray(
        finalGroups,
        numberOfTimesToRepeat,
        undefined,
      );

      return {
        finalGroups: [
          ...expandedPreviousGroups,
          ([] as RowHeaderType[]).concat(...times(total).map(() => groups)),
        ],
        total: total * (headerIsGroup[index] ? 1 : groups.length),
      };
    },
    { finalGroups: [], total: 1 },
  );

  return aggrigatedDuplication.finalGroups;
};

export const generateSpanInfoFromHeaders = (
  groups: RowHeaderType[][],
  rowHeaderIsGroup: boolean[],
): SpanInfo[][] => {
  return generateHeaderSpanInfo(
    generateAggregatedGroups(groups, rowHeaderIsGroup),
  );
};

const MultiHeaderTable = forwardRef<HTMLTableElement, Props>(
  (
    {
      ariaLabelledBy,
      className,
      columnHeaders,
      rowHeaders,
      rows,
      rowHeaderIsGroup = [],
      columnHeaderIsGroup = [],
    },
    ref,
  ) => {
    const columnSpanInfo = generateSpanInfoFromHeaders(
      columnHeaders,
      columnHeaderIsGroup,
    );
    const rowSpanInfo = generateSpanInfoFromHeaders(
      rowHeaders,
      rowHeaderIsGroup,
    );

    const firstRowLength = rowSpanInfo[0].length;

    return (
      <table
        aria-labelledby={ariaLabelledBy}
        className={classNames('govuk-table', styles.table, className)}
        ref={ref}
      >
        <thead>
          {columnSpanInfo.map((columns, rowIndex) => {
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

                {columns.map((column, columnIndex) => {
                  const key = `${column.heading}_${columnIndex}`;

                  return (
                    <th
                      className={classNames({
                        'govuk-table__header--numeric': !column.isGroup,
                        [styles.cellHorizontalMiddle]: column.isGroup,
                        [styles.borderBottom]: !column.isGroup,
                      })}
                      colSpan={column.count}
                      scope={column.isGroup ? 'colgroup' : 'col'}
                      key={key}
                    >
                      {column.heading}
                    </th>
                  );
                })}
              </tr>
            );
          })}
        </thead>

        <tbody>
          {transposeSpanColumnsToRows(rowSpanInfo)
            .reduce<ReactElement[][]>((agg, group, headerGroupIndex) => {
              return [
                ...agg,
                group.map((column, rowIndex) => (
                  <th
                    // eslint-disable-next-line react/no-array-index-key
                    key={`${rowIndex}_${headerGroupIndex}`}
                    className={classNames({
                      'govuk-table__cell--numeric': !column.isGroup,
                      [styles.borderBottom]: column.isGroup,
                    })}
                    rowSpan={column.count}
                    scope={column.isGroup ? 'rowgroup' : 'row'}
                  >
                    {column.heading}
                  </th>
                )),
              ];
            }, [])
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
                            (rowIndex + 1) % firstRowLength === 0,
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
