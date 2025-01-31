import React from 'react';

export default function DataSymbolsTable() {
  return (
    <table className="govuk-table">
      <thead className="govuk-table__head">
        <tr className="govuk-table__row">
          <th scope="col" className="govuk-table__header">
            Symbols
          </th>
          <th scope="col" className="govuk-table__header">
            Definition
          </th>
        </tr>
      </thead>
      <tbody className="govuk-table__body">
        <tr className="govuk-table__row">
          <th scope="row" className="govuk-table__header">
            z
          </th>
          <td className="govuk-table__cell">Data is not applicable</td>
        </tr>
        <tr className="govuk-table__row">
          <th scope="row" className="govuk-table__header">
            x
          </th>
          <td className="govuk-table__cell">
            Data is unavailable for unknown or other reasons
          </td>
        </tr>
        <tr className="govuk-table__row">
          <th scope="row" className="govuk-table__header">
            c
          </th>
          <td className="govuk-table__cell">
            Suppressed to protect confidential information
          </td>
        </tr>
        <tr className="govuk-table__row">
          <th scope="row" className="govuk-table__header">
            low / k
          </th>
          <td className="govuk-table__cell">Rounds to 0, but is not 0</td>
        </tr>
      </tbody>
    </table>
  );
}
