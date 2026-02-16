import classNames from 'classnames';
import React, { createElement, Ref } from 'react';
import { TableJson } from '../utils/mapTableToJson';
import styles from './MultiHeaderTable.module.scss';

export interface MultiHeaderTableProps {
  ariaLabelledBy?: string;
  className?: string;
  ref?: Ref<HTMLTableElement>;
  tableJson: TableJson;
}

const MultiHeaderTable = ({
  ariaLabelledBy,
  className,
  ref,
  tableJson,
}: MultiHeaderTableProps) => (
  <table
    data-testid={ariaLabelledBy && `${ariaLabelledBy}-table`}
    aria-labelledby={ariaLabelledBy}
    className={classNames('govuk-table', styles.table, className)}
    ref={ref}
  >
    <thead className={styles.tableHead}>
      {tableJson.thead.map((headerRow, rowIndex) => (
        // eslint-disable-next-line react/no-array-index-key
        <tr key={`headerRow-${rowIndex}`}>
          {headerRow.map((cell, i) =>
            createElement(
              cell.tag,
              {
                // eslint-disable-next-line react/no-array-index-key
                key: `cell-${rowIndex}-${i}`,
                colSpan: cell.colSpan,
                rowSpan: cell.rowSpan,
                scope: cell.scope,
              },
              cell.text,
            ),
          )}
        </tr>
      ))}
    </thead>
    <tbody>
      {tableJson.tbody.map((bodyRow, rowIndex) => (
        // eslint-disable-next-line react/no-array-index-key
        <tr key={`bodyRow-${rowIndex}`}>
          {bodyRow.map((cell, i) => {
            return (
              cell &&
              createElement(
                cell.tag,
                {
                  // eslint-disable-next-line react/no-array-index-key
                  key: `cell-${rowIndex}-${i}`,
                  colSpan: cell.colSpan,
                  rowSpan: cell.rowSpan,
                  scope: cell.scope,
                  className: classNames({
                    'govuk-table__cell--numeric dfe-white-space--nowrap':
                      cell.tag === 'td',
                  }),
                },
                cell.text,
              )
            );
          })}
        </tr>
      ))}
    </tbody>
  </table>
);

MultiHeaderTable.displayName = 'MultiHeaderTable';

export default MultiHeaderTable;
