import React, { Component } from 'react';
import styles from './GroupedDataTable.module.scss';

export interface GroupedDataSet {
  label: string;
  rows: {
    label: string;
    columns: string[];
  }[];
}

interface Props {
  caption: string;
  header: string[];
  groups: GroupedDataSet[];
}

class GroupedDataTable extends Component<Props> {
  public render() {
    const { caption, header, groups } = this.props;

    return (
      <table className="govuk-table">
        <caption>{caption}</caption>
        <thead>
          <tr>
            <th />
            {header.map((column, index) => (
              <th
                className="govuk-table__header--numeric"
                scope="col"
                key={`${column}_${index}`}
              >
                {column}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {groups.map((group, groupIndex) => {
            return (
              <React.Fragment key={`${group.label}-${groupIndex}`}>
                <tr>
                  <th
                    colSpan={1 + header.length}
                    className={styles.groupHeadingRow}
                  >
                    {group.label}
                  </th>
                </tr>
                {group.rows.map((row, rowIndex) => {
                  return (
                    <tr key={`${group.label}-${row}-${rowIndex}`}>
                      <td role="colgroup">{row.label}</td>
                      {row.columns.map((column, columnIndex) => {
                        return (
                          <td
                            className="govuk-table__cell--numeric"
                            key={`${group.label}-${
                              row.label
                            }-${column}-${columnIndex}`}
                          >
                            {column}
                          </td>
                        );
                      })}
                    </tr>
                  );
                })}
              </React.Fragment>
            );
          })}
        </tbody>
      </table>
    );
  }
}

export default GroupedDataTable;
