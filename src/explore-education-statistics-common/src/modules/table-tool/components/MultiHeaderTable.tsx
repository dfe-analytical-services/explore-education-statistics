import classNames from 'classnames';
import times from 'lodash/times';
import React, {forwardRef, ReactElement} from 'react';
import styles from './MultiHeaderTable.module.scss';

interface Props {
  ariaLabelledBy?: string;
  className?: string;
  columnHeaders: string[][];
  rowHeaders: string[][];
  ignoreRowHeaders?: boolean[];
  rows: string[][];
}

export interface SpanInfo {
  heading: string,
  count: number,
  start: number,
  isRowGroup: boolean,
  isLastInGroup: boolean
}

const reduceGroupSpanInfo = (overallHeaderLength: number, isRowGroup: boolean, isLastInGroup: boolean) =>
  (spanInfo: SpanInfo[], nextHeading: string, rowIndex: number) => {


    if (spanInfo.length === 0) {
      return [{heading: nextHeading, count: overallHeaderLength, start: rowIndex, isRowGroup, isLastInGroup}];
    }

    const previousHeading = spanInfo[spanInfo.length - 1];

    if (previousHeading.heading !== nextHeading) {

      return [...spanInfo.slice(0, -1),
        {
          ...previousHeading,
          count: rowIndex - previousHeading.start,
        },
        {
          heading: nextHeading,
          count: overallHeaderLength - rowIndex,
          start: rowIndex,
          isRowGroup,
          isLastInGroup
        }];
    }

    return spanInfo;

  };

export const generateHeaderSpanInfo = (headerGroups: string[][], isRowGroup: boolean, isLastInGroup: boolean) => {

  const overallHeaderLength = headerGroups.reduce((curMaxLength: number, group) => curMaxLength < group.length ? group.length : curMaxLength, 0);

  return headerGroups.reduce<SpanInfo[][]>((headerSpanInfo: SpanInfo[][], headingsForGroup: string[], groupIndex: number) => (
    [
      ...headerSpanInfo,
      headingsForGroup.reduce<SpanInfo[]>(
        reduceGroupSpanInfo(overallHeaderLength,
          isRowGroup || groupIndex === headingsForGroup.length - 1,
          isLastInGroup
        ), [])
    ]
  ), []);

};


export const transposeSpanInfoMatrix = (headers: SpanInfo[][]): SpanInfo[][] => {
  const lengthOfRow = headers[0].reduce((length, {count}) => length + count, 0);

  const rowColumnsReversed = headers.map(column => [...column].reverse());

  return [...Array(lengthOfRow)].map((_, currentRow) =>
    rowColumnsReversed.map(rowColumn => rowColumn.find(({start}) => start === currentRow)).filter(cell => !!cell)
  ) as SpanInfo[][];
};

export const repeatSusequentGroups = (headers: SpanInfo[][]): SpanInfo[][] => {
  return headers.reduce<SpanInfo[][]>((spanInfo, next) => {

    let additional: SpanInfo[];

    if (spanInfo.length > 0) {
      const previous = spanInfo[spanInfo.length - 1];

      if (previous.length > next.length) {

        additional = [...Array(previous.length)].map((_, index) => {
          const previousIndex = index % previous.length;
          return {
            ...next[index % next.length],
            count: previous[previousIndex].count,
            start: previous[previousIndex].start,
          }
        });


      } else {
        additional = next;
      }

    } else {
      additional = next;
    }

    return [...spanInfo, additional];

  }, []);

};

const scaleSpanInfo = (spanInfo: SpanInfo[][], scale: number) => spanInfo.map(
  group => group.map(
    heading => (
      {
        ...heading,
        count: heading.count * scale,
        start: heading.start * scale,
      }
    )
  )
);

const generateRepeatedSpanInfo = (group: string[], numberOfRepetitions: number): SpanInfo[][] => {
  const repeatedGroup = ([] as string[]).concat(...[...Array(numberOfRepetitions)].map(_ => group));
  const numberInFinalGroup = group.length;
  const numberInFinalGroupMinus1 = numberInFinalGroup - 1;

  return generateHeaderSpanInfo([repeatedGroup], false, false)
    .reduce<SpanInfo[][]>((repeatedSpanInfo, currentGroup) =>
      ([...repeatedSpanInfo,
        currentGroup.map((span, index) => ({
          ...span,
          isLastInGroup: span.isLastInGroup || (index % numberInFinalGroup === numberInFinalGroupMinus1)
        }))
      ]), [])
    ;
};

export const generateSpan2 = (headerGroups: string[][], ignoreRow: boolean[] = []): string[][] => {

  const aggrigatedDuplication = headerGroups.reduce<{ final: string[][], total: number }>(({final, total}, groups, index) => {
    const newFinal: string[][] = [
      ...final,
      ([] as string[]).concat(...[...Array(total)].map(() => groups))
    ];

    const newTotal = total * ( ignoreRow[index] ? 1 : groups.length );

    return {final: newFinal, total: newTotal};


  }, {final: [], total: 1});


  return aggrigatedDuplication.final;
};


export const generateSpanInfoFromGroups = (groups: string[][], ignoreRowHeaders: boolean[]): SpanInfo[][] => {
  const groups2 = generateSpan2(groups, ignoreRowHeaders);

  console.log(groups, ignoreRowHeaders, groups2);

  const groupsWithoutIndicators = [...groups2];
  const finalGroup = groupsWithoutIndicators.pop() as string[];
  const maxNumberOfGroups = groupsWithoutIndicators.reduce((curMaxLength: number, group) => curMaxLength < group.length ? group.length : curMaxLength, 0);

  const numberInFinalGroup = finalGroup.length;

  console.log(numberInFinalGroup);

  const scaledSpanInfo = scaleSpanInfo(
    repeatSusequentGroups(
      generateHeaderSpanInfo(groupsWithoutIndicators, true, false)
    ), numberInFinalGroup);

  const finalGroupSpanInfo = generateHeaderSpanInfo([finalGroup], false, false);
  return [...scaledSpanInfo, ...finalGroupSpanInfo];
};


const MultiHeaderTable = forwardRef<HTMLTableElement, Props>(
  ({ariaLabelledBy, className, columnHeaders, rowHeaders, rows, ignoreRowHeaders = []}, ref) => {
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
          {

            transposeSpanInfoMatrix(generateSpanInfoFromGroups(rowHeaders, ignoreRowHeaders))
              .reduce<ReactElement[][]>((agg, group, headerGroupIndex) => {
                  return [...agg,
                    group.map((column, rowIndex) => (
                      <th
                        // eslint-disable-next-line react/no-array-index-key
                        key={`${rowIndex}_${headerGroupIndex}`}
                        className={classNames({
                          'govuk-table__cell--numeric': !column.isRowGroup,
                          'govuk-table__cell--center': column.isRowGroup,
                          [styles.borderRight]: !column.isRowGroup,
                          [styles.borderBottom]:
                          column.isRowGroup || column.isLastInGroup
                        })}
                        rowSpan={column.count}
                        scope={column.isRowGroup ? 'rowgroup' : 'row'}
                      >
                        {column.heading}
                      </th>
                    ))
                  ]
                }, []
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
