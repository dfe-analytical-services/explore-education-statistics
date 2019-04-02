import classNames from 'classnames';
import React, { Component } from 'react';
import styles from 'src/prototypes/table-tool/components/TimePeriodDataTable.module.scss';

export interface HeaderGroup {
  label: string;
  columns: string[];
}

export interface RowGroup {
  label: string;
  rows: {
    label: string;
    columnGroups: string[][];
  }[];
}

interface Props {
  caption: string;
  headers: HeaderGroup[];
  rowGroups: RowGroup[];
}

class PrototypeGroupedDataTable extends Component<Props> {
  public render() {
    const { caption, headers, rowGroups } = this.props;

    return (
      <div className={styles.tableContainer}>
        <table className="govuk-table">
          <caption>{caption}</caption>

          <thead>
            <tr>
              <th colSpan={2} />
              {headers.map((group, index) => (
                <th
                  className="govuk-table__header--center govuk-table__header--border-left"
                  colSpan={group.columns.length || 1}
                  scope="colgroup"
                  key={`${group.label}-${index}`}
                >
                  {group.label}
                </th>
              ))}
            </tr>
            <tr>
              <th colSpan={2} />
              {headers.flatMap(group =>
                group.columns.map((column, index) => (
                  <th
                    className={classNames('govuk-table__header--numeric', {
                      'govuk-table__header--border-left': index === 0,
                    })}
                    scope="col"
                    key={`${group.label}-${column}-${index}`}
                  >
                    {column}
                  </th>
                )),
              )}
            </tr>
          </thead>
          {rowGroups.map((group, groupIndex) => {
            const groupKey = `${group.label}-${groupIndex}`;

            return (
              <tbody key={groupKey}>
                {group.rows.map((row, rowIndex) => {
                  const rowKey = `${groupKey}-${row.label}-${rowIndex}`;

                  return (
                    <tr key={rowKey}>
                      {rowIndex === 0 && (
                        <th scope="rowgroup" rowSpan={group.rows.length || 1}>
                          {group.label}
                        </th>
                      )}
                      <th scope="row">{row.label}</th>
                      {row.columnGroups.flatMap((colGroup, colGroupIndex) =>
                        colGroup.map((column, columnIndex) => {
                          return (
                            <td
                              className={classNames(
                                'govuk-table__cell--numeric',
                                {
                                  'govuk-table__cell--border-left':
                                    columnIndex === 0,
                                },
                              )}
                              key={`${rowKey}-${colGroupIndex}-${column}-${columnIndex}`}
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
            );
          })}
        </table>
      </div>
    );
  }
}

export default PrototypeGroupedDataTable;
