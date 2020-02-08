import classNames from 'classnames';
import last from 'lodash/last';
import sumBy from 'lodash/sumBy';
import times from 'lodash/times';
import React, { forwardRef, ReactElement } from 'react';
import styles from './MultiHeaderTable.module.scss';

/**
 * Header groups can have subgroups. This is intended
 * for use with 'filter groups' in table tool to make it
 * more obvious which filters are grouped together.
 * e.g.
 * 'Ethnic group major' is the filter group for
 * 'Ethnicity Major Asian Total' and 'Ethnicity Major Black Total'.
 */
export interface HeaderSubGroup {
  text: string;
  span?: number;
}

/**
 * A header group describes a row/column of table headers.
 * We can have multiple header groups in the row or
 * column directions.
 * Header groups can have additional subgroups which
 * introduce additional rows/columns into the rendered headers.
 */
export interface HeaderGroup {
  headers: {
    text: string;
  }[];
  groups?: HeaderSubGroup[];
}

interface ExpandedHeader {
  text: string;
  span: number;
  crossSpan: number;
  start: number;
  isGroup: boolean;
}

/**
 * Create {@see ExpandedHeader}s that provide a more
 * detailed model for rendering the table headers.
 */
const createExpandedHeaders = (
  headerGroups: HeaderGroup[],
): ExpandedHeader[][] => {
  // The max number of rows/column spans that any
  // header group can be for this table.
  const maxSpan = headerGroups.reduce(
    (acc, headerGroup) => acc * headerGroup?.headers?.length,
    1,
  );

  return headerGroups.reduce<ExpandedHeader[][]>(
    (acc, headerGroup, headerGroupIndex) => {
      const previousGroupLength = last(acc)?.length ?? 1;
      const span = maxSpan / (headerGroup.headers.length * previousGroupLength);

      let expandedHeaderGroups: ExpandedHeader[] = [];

      if (headerGroup.groups && headerGroup.groups.length) {
        const hasMultipleGroups = headerGroup.groups.length > 1;

        // Header subgroups can have different sizes depending
        // on the `span` that is specified.
        // It should not be bigger or smaller than it's parent
        // header group's span as this will break the table.
        // If we have a single header subgroup, then we just
        // make it span the entire header group.
        const groupSpan = hasMultipleGroups
          ? sumBy(headerGroup.groups, header => (header.span ?? 1) * span)
          : maxSpan;

        expandedHeaderGroups = times(
          maxSpan / groupSpan,
          repeat =>
            headerGroup.groups?.reduce((headerSubGroups, header) => {
              const previous = last(headerSubGroups);

              headerSubGroups.push({
                text: header.text,
                span: hasMultipleGroups ? (header.span ?? 1) * span : groupSpan,
                crossSpan: 1,
                start: previous
                  ? previous.start + previous.span
                  : repeat * groupSpan,
                isGroup: true,
              });

              return headerSubGroups;
            }, [] as ExpandedHeader[]) ?? [],
        ).flat();

        acc.push(expandedHeaderGroups);
      }

      const expandedHeaders: ExpandedHeader[] = times(
        previousGroupLength,
        repeat =>
          headerGroup.headers.map((header, headerIndex) => {
            return {
              text: header.text,
              // Headers are expected to have equal sizes
              // within their group (unlike header subgroups)
              span,
              crossSpan: 1,
              start:
                repeat * headerGroup.headers.length * span + headerIndex * span,
              isGroup: headerGroupIndex !== headerGroups.length - 1,
            };
          }),
      )
        .flat()
        .filter(expandedHeader => {
          // We want to filter out any headers that duplicate
          // their corresponding header group to clean up the headers.
          const matchingHeaderGroup = expandedHeaderGroups.find(
            expandedHeaderGroup =>
              expandedHeaderGroup.start === expandedHeader.start &&
              expandedHeaderGroup.text === expandedHeader.text &&
              expandedHeaderGroup.span === 1,
          );

          // We are simply making the header group take
          // up the space that the removed header
          // would have taken in the table.
          if (matchingHeaderGroup) {
            // A bit naughty, but we mutate the header group
            // to re-use the current context and avoid
            // having to perform any further complex loops.
            matchingHeaderGroup.isGroup = false;
            matchingHeaderGroup.crossSpan = 2;
          }

          return !matchingHeaderGroup;
        });

      acc.push(expandedHeaders);

      return acc;
    },
    [],
  );
};

interface Props {
  ariaLabelledBy?: string;
  className?: string;
  columnHeaders: HeaderGroup[];
  rowHeaders: HeaderGroup[];
  rows: string[][];
}

const MultiHeaderTable = forwardRef<HTMLTableElement, Props>(
  ({ ariaLabelledBy, className, columnHeaders, rowHeaders, rows }, ref) => {
    const expandedColumnHeaders = createExpandedHeaders(columnHeaders);
    const expandedRowHeaders = createExpandedHeaders(rowHeaders);

    const firstRowLength = expandedRowHeaders[0].length;

    // Expanded headers need to be transposed so that
    // they are compatible with HTML table rows.
    const transposeToRows = (
      headerGroups: ExpandedHeader[][],
    ): ExpandedHeader[][] => {
      const totalRows = sumBy(headerGroups, headerGroup =>
        sumBy(headerGroup, header => (header.isGroup ? 0 : 1)),
      );

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

                {columns.map((column, columnIndex) => {
                  const key = `${column.text}_${columnIndex}`;

                  return (
                    <th
                      className={classNames({
                        'govuk-table__header--numeric': !column.isGroup,
                        [styles.cellHorizontalMiddle]: column.isGroup,
                        [styles.borderBottom]: !column.isGroup,
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
                rowGroup.map((row, rowIndex) => (
                  <th
                    // eslint-disable-next-line react/no-array-index-key
                    key={`${rowIndex}_${headerGroupIndex}`}
                    className={classNames({
                      'govuk-table__cell--numeric': !row.isGroup,
                      [styles.borderBottom]: row.isGroup,
                    })}
                    rowSpan={row.span}
                    colSpan={row.crossSpan}
                    scope={row.isGroup ? 'rowgroup' : 'row'}
                  >
                    {row.text}
                  </th>
                )),
              );

              return acc;
            }, [] as ReactElement[][])
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
