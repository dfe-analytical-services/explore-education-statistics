import React from 'react';

interface Props {
  caption?: string;
}

const PrototypeTableSample = ({ caption }: Props) => {
  return (
    <>
      <div className="dfe-content-overflow">
        <table className="govuk-table">
          <caption className="govuk-heading-s">{caption}</caption>
          <thead className="govuk-table__head">
            <tr className="tableizer-firstrow">
              <th className="govuk-table__header" />
              <th
                className="govuk-table__header govuk-table__cell--numeric"
                scope="col"
              >
                2012/13
              </th>
              <th
                className="govuk-table__header govuk-table__cell--numeric"
                scope="col"
              >
                2013/14
              </th>
              <th
                className="govuk-table__header govuk-table__cell--numeric"
                scope="col"
              >
                2014/15
              </th>
              <th
                className="govuk-table__header govuk-table__cell--numeric"
                scope="col"
              >
                2015/16
              </th>
              <th
                className="govuk-table__header govuk-table__cell--numeric"
                scope="col"
              >
                2016/17
              </th>
            </tr>
          </thead>

          <tbody className="govuk-table__body">
            <tr className="govuk-table__row">
              <th className="govuk-table__header" scope="row">
                Number of schools
              </th>
              <td className="govuk-table__cell govuk-table__cell--numeric">
                21,130
              </td>
              <td className="govuk-table__cell govuk-table__cell--numeric">
                21,151
              </td>
              <td className="govuk-table__cell govuk-table__cell--numeric">
                21,178
              </td>
              <td className="govuk-table__cell govuk-table__cell--numeric">
                21,163
              </td>
              <td className="govuk-table__cell govuk-table__cell--numeric">
                {' '}
                21,247{' '}
              </td>
            </tr>
            <tr className="govuk-table__row">
              <th className="govuk-table__header" scope="row">
                Number of pupil enrolments
              </th>
              <td className="govuk-table__cell govuk-table__cell--numeric">
                6,477,725
              </td>
              <td className="govuk-table__cell govuk-table__cell--numeric">
                6,554,005
              </td>
              <td className="govuk-table__cell govuk-table__cell--numeric">
                6,642,755
              </td>
              <td className="govuk-table__cell govuk-table__cell--numeric">
                6,737,190
              </td>
              <td className="govuk-table__cell govuk-table__cell--numeric">
                {' '}
                6,899,770{' '}
              </td>
            </tr>
            <tr className="govuk-table__row">
              <th className="govuk-table__header" colSpan={6}>
                Percentage of sessions missed due to:
              </th>
            </tr>
            <tr className="govuk-table__row">
              <th className="govuk-table__header" scope="row">
                Overall absence
              </th>
              <td className="govuk-table__cell govuk-table__cell--numeric">
                5.3
              </td>
              <td className="govuk-table__cell govuk-table__cell--numeric">
                4.5
              </td>
              <td className="govuk-table__cell govuk-table__cell--numeric">
                4.6
              </td>
              <td className="govuk-table__cell govuk-table__cell--numeric">
                4.6
              </td>
              <td className="govuk-table__cell govuk-table__cell--numeric">
                4.7
              </td>
            </tr>
            <tr className="govuk-table__row">
              <th className="govuk-table__header" scope="row">
                Authorised absence
              </th>
              <td className="govuk-table__cell govuk-table__cell--numeric">
                4.2
              </td>
              <td className="govuk-table__cell govuk-table__cell--numeric">
                3.5
              </td>
              <td className="govuk-table__cell govuk-table__cell--numeric">
                3.5
              </td>
              <td className="govuk-table__cell govuk-table__cell--numeric">
                3.4
              </td>
              <td className="govuk-table__cell govuk-table__cell--numeric">
                3.4
              </td>
            </tr>
            <tr className="govuk-table__row">
              <th className="govuk-table__header" scope="row">
                Unauthorised absence
              </th>
              <td className="govuk-table__cell govuk-table__cell--numeric">
                1.1
              </td>
              <td className="govuk-table__cell govuk-table__cell--numeric">
                1.1
              </td>
              <td className="govuk-table__cell govuk-table__cell--numeric">
                1.1
              </td>
              <td className="govuk-table__cell govuk-table__cell--numeric">
                1.1
              </td>
              <td className="govuk-table__cell govuk-table__cell--numeric">
                1.3
              </td>
            </tr>
          </tbody>
        </table>
        <p className="govuk-body-s">Source: Education Prototype Statistics</p>
      </div>
    </>
  );
};

export default PrototypeTableSample;
