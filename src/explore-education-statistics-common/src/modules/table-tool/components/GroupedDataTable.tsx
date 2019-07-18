import React from 'react';
import styles from './GroupedDataTable.module.scss';

export interface GroupedDataSet {
  name: string;
  rows: {
    name: string;
    columns: string[];
  }[];
}

interface Props {
  caption: string;
  header: string[];
  groups: GroupedDataSet[];
}

function GroupedDataTable({ caption, header, groups }: Props) {
  return (
    <table className="govuk-table">
      <caption>{caption}</caption>
      <thead>
        <tr>
          <th />
          {header.map((column, index) => {
            const key = `${column}_${index}`;

            return (
              <th
                className="govuk-table__header--numeric"
                scope="col"
                key={key}
              >
                {column}
              </th>
            );
          })}
        </tr>
      </thead>
      <tbody>
        {groups.map((group, groupIndex) => {
          const groupKey = `${group.name}-${groupIndex}`;

          return (
            <React.Fragment key={groupKey}>
              <tr>
                <th
                  colSpan={1 + header.length}
                  className={styles.groupHeadingRow}
                >
                  {group.name}
                </th>
              </tr>
              {group.rows.map((row, rowIndex) => {
                const rowKey = `${group.name}-${row}-${rowIndex}`;

                return (
                  <tr key={rowKey}>
                    <td>{row.name}</td>
                    {row.columns.map((column, columnIndex) => {
                      const cellKey = `${group.name}_${
                        row.name
                      }_${column}_${columnIndex}`;

                      return (
                        <td
                          className="govuk-table__cell--numeric"
                          key={cellKey}
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

export default GroupedDataTable;
