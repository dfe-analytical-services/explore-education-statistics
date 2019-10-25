import classNames from 'classnames';
import times from 'lodash/times';
import React, {forwardRef, ReactElement} from 'react';
import styles from './MultiHeaderTable.module.scss';

interface Props {
  ariaLabelledBy?: string;
  className?: string;
  columnHeaders: string[][];
  rowHeaders: string[][];
  rows: string[][];
}

interface SpanInfo {
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

    if (nextHeading && previousHeading.heading !== nextHeading) {

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
          isRowGroup || groupIndex === headingsForGroup.length -1,
          isLastInGroup
      ), [])
    ]
  ), []);

};


export const generateHeaders = (headers: SpanInfo[][]): SpanInfo[][] => {
  const numberOfRows = headers[0].reduce((length, {count}) => length + count, 0);

  const rowColumnsReversed = headers.map(column => [...column].reverse());

  const h = [...Array(numberOfRows)].map((_, i) =>
    rowColumnsReversed.map(rowColumn => rowColumn.find(({start}) => start === i)).filter(cell => !!cell)
  );

  return h as SpanInfo[][];
};

export const iDontKnow = (groups: string[][]): SpanInfo[][] => {

  const groupsWithoutIndicators = [...groups];
  const indicators = groupsWithoutIndicators.pop() as string[];
  const numberOfColGroups = groupsWithoutIndicators.reduce((curMaxLength: number, group) => curMaxLength < group.length ? group.length : curMaxLength, 0);

  const numberOfIndicators = indicators.length;

  const spanInfo = generateHeaderSpanInfo(groupsWithoutIndicators, true, false);

  const scaledSpanInfo = spanInfo.map(
    group => group.map(
      heading => (
        {
          ...heading,
          count: heading.count * numberOfIndicators,
          start: heading.start * numberOfIndicators,
        }
      )
    )
  );

  const repeatedIndicators = ([] as string[]).concat(...[...Array(numberOfColGroups)].map(_ => indicators));
  const indicatorSpanInfo =
    generateHeaderSpanInfo([repeatedIndicators], false, false)
      .reduce<SpanInfo[][]>( (indicatorSpanGroups, indicatorSpanGroup) =>
        ([...indicatorSpanGroups,
          indicatorSpanGroup.map( (span, index) => ({...span, isLastInGroup: span.isLastInGroup || (index % numberOfIndicators === numberOfIndicators-1)}))
      ]), [])
  ;

  const combinedSpanInfo = [...scaledSpanInfo, ...indicatorSpanInfo];


  console.log(combinedSpanInfo);

  return combinedSpanInfo;
}

const MultiHeaderTable = forwardRef<HTMLTableElement, Props>(
  ({ariaLabelledBy, className, columnHeaders, rowHeaders, rows}, ref) => {
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

            generateHeaders(iDontKnow(rowHeaders))
              .reduce<ReactElement[][]>((agg, group, headerGroupIndex) => {

                  console.log(group);

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
                        scope={column.isRowGroup  ? 'rowgroup' : 'row'}
                      >
                        {column.heading}
                      </th>
                    ))
                  ]
                }, []
              )
              /*
                          rowHeaders
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
*/

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
