import Header from '@common/modules/table-tool/components/utils/Header';
import classNames from 'classnames';
import last from 'lodash/last';
import React, { forwardRef } from 'react';
import styles from './MultiHeaderTable.module.scss';

interface ExpandedHeader {
  id: string;
  text: string;
  start: number;
  span: number;
  crossSpan: number;
  isHidden: boolean;
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
    const createExpandedHeaders = (headers: Header[]): ExpandedHeader[][] => {
      const createExpandedHeader = (
        header: Header,
        expandedHeaders: ExpandedHeader[][],
      ): ExpandedHeader => {
        const { depth } = header;
        const previousSibling = last(expandedHeaders[depth]);

        const newExpandedHeader = {
          id: header.id,
          text: header.text,
          start: previousSibling
            ? previousSibling.start + previousSibling.span
            : 0,
          span: header.span,
          crossSpan: 1,
          isHidden: false,
          isGroup: header.hasChildren(),
        };

        // Header and its parents appear identical
        // so we can merge them together.
        if (
          header.text === header.parent?.text &&
          header.span === header.parent?.span
        ) {
          const parent = expandedHeaders[depth - 1]?.find(
            expandedHeader => expandedHeader.start === newExpandedHeader.start,
          );

          if (parent) {
            newExpandedHeader.isHidden = true;

            // Need to modify parent so that it can take
            // up the position of the child header.
            parent.crossSpan += 1;
            parent.isGroup = newExpandedHeader.isGroup;
          }
        }

        return newExpandedHeader;
      };

      const addExpandedChildren = (
        acc: ExpandedHeader[][],
        children: Header[],
      ): ExpandedHeader[][] => {
        children.forEach(child => {
          const { depth } = child;

          if (!acc[depth]) {
            acc[depth] = [createExpandedHeader(child, acc)];
          } else {
            acc[depth].push(createExpandedHeader(child, acc));
          }

          if (child.hasChildren()) {
            addExpandedChildren(acc, child.children);
          }
        });

        return acc;
      };

      return headers.reduce<ExpandedHeader[][]>((acc, header) => {
        const { depth } = header;

        if (!acc[depth]) {
          acc[depth] = [createExpandedHeader(header, acc)];
        } else {
          acc[depth].push(createExpandedHeader(header, acc));
        }

        return addExpandedChildren(acc, header.children);
      }, []);
    };

    const expandedRowHeaders = createExpandedHeaders(rowHeaders);
    const expandedColumnHeaders = createExpandedHeaders(columnHeaders);

    const firstRowHeaderLength = expandedRowHeaders[0].length;

    // Expanded headers need to be transposed so that
    // they are compatible with HTML table rows.
    const transposeToRows = (
      headerGroups: ExpandedHeader[][],
    ): ExpandedHeader[][] => {
      return headerGroups.reduce<ExpandedHeader[][]>((acc, headerGroup) => {
        headerGroup.forEach(header => {
          if (acc[header.start]) {
            acc[header.start].push(header);
          } else {
            acc[header.start] = [header];
          }
        });

        return acc;
      }, []);
    };

    const transposedRowHeaders = transposeToRows(
      expandedRowHeaders,
    ) as ExpandedHeader[][];

    return (
      <table
        data-testid={ariaLabelledBy && `${ariaLabelledBy}-table`}
        aria-labelledby={ariaLabelledBy}
        className={classNames('govuk-table', styles.table, className)}
        ref={ref}
      >
        <thead className={styles.tableHead}>
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
          {rows.map((row, rowIndex) => (
            // eslint-disable-next-line react/no-array-index-key
            <tr key={rowIndex}>
              {transposedRowHeaders[rowIndex]
                ?.filter(header => !header.isHidden)
                ?.map((header, headerIndex) => (
                  <th
                    // eslint-disable-next-line react/no-array-index-key
                    key={headerIndex}
                    className={classNames({
                      [styles.borderBottom]: header.isGroup,
                    })}
                    rowSpan={header.span}
                    colSpan={header.crossSpan}
                    scope={header.isGroup ? 'rowgroup' : 'row'}
                  >
                    {header.text}
                  </th>
                ))}

              {row.map((cell, cellIndex) => (
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
            </tr>
          ))}
        </tbody>
      </table>
    );
  },
);

MultiHeaderTable.displayName = 'MultiHeaderTable';

export default MultiHeaderTable;
