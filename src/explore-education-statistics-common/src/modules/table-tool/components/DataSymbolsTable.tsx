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
            Usage
          </th>
          <th scope="col" className="govuk-table__header">
            Example
          </th>
          <th scope="col" className="govuk-table__header">
            Obsolete equivalents
          </th>
        </tr>
      </thead>
      <tbody className="govuk-table__body">
        <tr className="govuk-table__row">
          <th scope="row" className="govuk-table__header">
            z
          </th>
          <td className="govuk-table__cell">
            When an observation is not applicable
          </td>
          <td className="govuk-table__cell">
            No data for boys at an all-girls school
          </td>
          <td className="govuk-table__cell" />
        </tr>
        <tr className="govuk-table__row">
          <th scope="row" className="govuk-table__header">
            x
          </th>
          <td className="govuk-table__cell">
            When data is unavailable for other reasons
          </td>
          <td className="govuk-table__cell">
            Data for an indicator is not collected in a certain region
          </td>
          <td className="govuk-table__cell govuk-table__header--center">:</td>
        </tr>
        <tr className="govuk-table__row">
          <th scope="row" className="govuk-table__header">
            c
          </th>
          <td className="govuk-table__cell">Confidential data</td>
          <td className="govuk-table__cell">Data has been suppressed</td>
          <td className="govuk-table__cell" />
        </tr>
        <tr className="govuk-table__row">
          <th scope="row" className="govuk-table__header">
            low
          </th>
          <td className="govuk-table__cell">Rounds to 0, but is not 0</td>
          <td className="govuk-table__cell">
            Rounding to the nearest thousand, 499 would otherwise show as 0.
            Only use 0 for true 0’s
          </td>
          <td className="govuk-table__cell govuk-table__header--center">~</td>
        </tr>
        <tr className="govuk-table__row">
          <th scope="row" className="govuk-table__header">
            u
          </th>
          <td className="govuk-table__cell">
            When an observation is of low reliability
          </td>
          <td className="govuk-table__cell">
            Data for a local authority is identified as missing returns so is
            removed from regional and national totals
          </td>
          <td className="govuk-table__cell" />
        </tr>
      </tbody>
    </table>
  );
}
