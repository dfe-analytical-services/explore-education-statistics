/* eslint-disable @typescript-eslint/no-unused-vars */
import React from 'react';
import {
  DataBlockData,
  DataBlockMetadata,
} from '@common/services/dataBlockService';

export interface Props {
  data: DataBlockData;
  meta: DataBlockMetadata;
  indicators: string[];
}

const Caption = ({ data, meta, indicators }: Props) => {
  return <caption className="govuk-heading-s">Table representing</caption>;
};

const TableHeading = ({ data, meta, indicators }: Props) => {
  return (
    <thead className="govuk-table__head">
      <tr>
        <th className="govuk-table__header" />
        {data.result.map(result => (
          <th
            key={result.year}
            className="govuk-table__header govuk-table__cell--numeric"
            scope="col"
          >
            {result.year}
          </th>
        ))}
      </tr>
    </thead>
  );
};

const TableBody = ({ data, meta, indicators }: Props) => {
  return (
    <tbody className="govuk-table__body">
      {indicators.map(indicator => (
        <tr key={indicator} className="govuk-table__row ">
          <th className="govuk-table__header">
            {meta.indicators[indicator].label}
          </th>
          {data.result.map(result => (
            <td
              key={`${indicator}_${result.year}`}
              className="govuk-table__cell govuk-table__cell--numeric"
            >
              {result.measures[indicator]}
            </td>
          ))}
        </tr>
      ))}
    </tbody>
  );
};

// eslint-disable-next-line @typescript-eslint/no-unused-vars
const TableRenderer2 = (props: Props) => {
  return (
    <table className="govuk-table">
      <Caption {...props} />
      <TableHeading {...props} />
      <TableBody {...props} />
    </table>
  );
};

export default TableRenderer2;
