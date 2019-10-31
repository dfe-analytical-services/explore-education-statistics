import classNames from 'classnames';
import times from 'lodash/times';
import React, {forwardRef, ReactElement} from 'react';
import styles from './MultiHeaderTable.module.scss';

interface Props {
  ariaLabelledBy?: string;
  className?: string;
  columnHeaders: string[][];
  rowHeaders: RowHeaderType[][];
  rowHeaderIsGroup?: boolean[];
  rows: string[][];
}

export interface SpanInfo {
  heading: string;
  count: number;
  start: number;
  isRowGroup: boolean;
}
export type RowHeaderType = string | undefined;

const generateSpanInfoForSingleGroup = (headingsForGroup:RowHeaderType[], overallHeaderLength : number, isRowGroup: boolean) => {
  return headingsForGroup.reduce<SpanInfo[]>(

    (spanInfo: SpanInfo[], nextHeading: RowHeaderType, rowIndex: number) => {

      if (spanInfo.length === 0) {
        return [
          {
            heading: nextHeading || '',
            count: overallHeaderLength,
            start: rowIndex,
            isRowGroup
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
          isRowGroup,
        },
      ];
    },
    []
  );
};

export const generateHeaderSpanInfo = (
  headerGroups: RowHeaderType[][],
) => {
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
      generateSpanInfoForSingleGroup(headingsForGroup, overallHeaderLength,groupIndex < headerGroups.length - 1, )
    ],
    [],
  );
};

export const transposeSpanInfoMatrix = (
  headers: SpanInfo[][],
): SpanInfo[][] => {
  const lengthOfRow = headers[0].reduce(
    (length, {count}) => length + count,
    0,
  );

  const rowColumnsReversed = headers.map(column => [...column].reverse());

  return [...Array(lengthOfRow)].map((_, currentRow) =>
    rowColumnsReversed
      .map(rowColumn => rowColumn.find(({start}) => start === currentRow))
      .filter(cell => !!cell),
  ) as SpanInfo[][];
};


export const generateAggregatedGroups = (
  headerGroups: RowHeaderType[][],
  headerIsGroup: boolean[] = [],
): (RowHeaderType)[][] => {
  const aggrigatedDuplication = headerGroups.reduce<{
    finalGroups: RowHeaderType[][];
    total: number;
  }>(
    ({finalGroups, total}, groups, index) => {
      const numberOfTimesToRepeat =
        index > 0 && headerIsGroup[index - 1] ? 1 : groups.length;

      const expandedPreviousGroups = finalGroups.map(finalRow =>
        ([] as RowHeaderType[]).concat(
          ...finalRow.map(cell =>
            [...Array(numberOfTimesToRepeat)].map((_, idx) => idx === 0 ? cell : undefined),
          ),
        ),
      );

      return {
        finalGroups: [
          ...expandedPreviousGroups,
          ([] as RowHeaderType[]).concat(...[...Array(total)].map(() => groups)),
        ],
        total: total * (headerIsGroup[index] ? 1 : groups.length),
      };
    },
    {finalGroups: [], total: 1},
  );

  return aggrigatedDuplication.finalGroups;
};

export const generateSpanInfoFromGroups = (
  groups: RowHeaderType[][],
  rowHeaderIsGroup: boolean[],
): SpanInfo[][] => {

  console.log(groups, rowHeaderIsGroup);

  return generateHeaderSpanInfo(
    generateAggregatedGroups(groups, rowHeaderIsGroup)
  );
};

export const MultiHeaderTable = forwardRef<HTMLTableElement, Props>(
  (
    {
      ariaLabelledBy,
      className,
      columnHeaders,
      rowHeaders,
      rows,
      rowHeaderIsGroup = [],
    },
    ref,
  ) => {
    const getSpan = (groups: RowHeaderType[][], groupIndex: number) => {
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
          {transposeSpanInfoMatrix(
            generateSpanInfoFromGroups(rowHeaders, rowHeaderIsGroup),
          )
            .reduce<ReactElement[][]>((agg, group, headerGroupIndex) => {
              return [
                ...agg,
                group.map((column, rowIndex) => (
                  <th
                    // eslint-disable-next-line react/no-array-index-key
                    key={`${rowIndex}_${headerGroupIndex}`}
                    className={classNames({
                      'govuk-table__cell--numeric': !column.isRowGroup,
                      'govuk-table__cell--center': column.isRowGroup,
                      [styles.borderBottom]: column.isRowGroup ,
                    })}
                    rowSpan={column.count}
                    scope={column.isRowGroup ? 'rowgroup' : 'row'}
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
