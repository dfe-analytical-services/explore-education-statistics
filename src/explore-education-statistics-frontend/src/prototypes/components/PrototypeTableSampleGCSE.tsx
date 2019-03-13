import React from 'react';

interface Props {
  caption?: string;
}

const PrototypeTableSample = ({ caption }: Props) => {
  return (
    <>
      <div className="dfe-content-overflow">
        <section>
          <table className="govuk-table">
            <caption className="govuk-heading-s">{caption}</caption>
            <thead className="govuk-table__head">
              <tr className="tableizer-firstrow">
                <th className="govuk-table__header govuk-!-width-one-quarter">
                  School type
                </th>
                <th className="govuk-table__header govuk-table__cell--numeric govuk-!-width-one-quarter">
                  2016 (revised)
                </th>
                <th className="govuk-table__header govuk-table__cell--numeric govuk-!-width-one-quarter">
                  2016 matched to 2017 point scores (shadow data)
                </th>
                <th className="govuk-table__header govuk-table__cell--numeric govuk-!-width-one-quarter">
                  2017 (revised)
                </th>
              </tr>
            </thead>

            <tbody className="govuk-table__body">
              <tr className="govuk-table__row">
                <td className="govuk-table__cell">All schools</td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  48.5
                </td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  43.6
                </td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  44.6
                </td>
              </tr>
              <tr className="govuk-table__row">
                <td className="govuk-table__cell">State-funded schools</td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  49.9
                </td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  44.6
                </td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  46.3
                </td>
              </tr>
            </tbody>
          </table>
          <footer>
            <p className="govuk-body-s">
              The methodology for this measure has changed from 2016 to 2017
            </p>
          </footer>
        </section>

        <section>
          <table className="govuk-table govuk-!-margin-top-9">
            <caption className="govuk-heading-s">
              Table showing percentage achieving the threshold in English and
              maths
            </caption>
            <thead className="govuk-table__head">
              <tr className="tableizer-firstrow">
                <th className="govuk-table__header govuk-!-width-one-quarter">
                  School type
                </th>
                <th className="govuk-table__header govuk-table__cell--numeric govuk-!-width-one-quarter">
                  2016 revised
                </th>
                <th className="govuk-table__header govuk-table__cell--numeric govuk-!-width-one-quarter">
                  2017 revised (9-5 grades in English and maths)
                </th>
                <th className="govuk-table__header govuk-table__cell--numeric govuk-!-width-one-quarter">
                  2017 revised (9-4 grades in English and maths)
                </th>
              </tr>
            </thead>

            <tbody className="govuk-table__body">
              <tr className="govuk-table__row">
                <td className="govuk-table__cell">All schools</td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  59.3%
                </td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  39.6%
                </td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  59.1%
                </td>
              </tr>
              <tr className="govuk-table__row">
                <td className="govuk-table__cell">State-funded schools</td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  63.0%
                </td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  42.6%
                </td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  63.9%
                </td>
              </tr>
            </tbody>
          </table>
          <footer>
            <p className="govuk-body-s">
              This measure is calculated using the same methodology as 2016.
            </p>
          </footer>
        </section>

        <section>
          <table className="govuk-table govuk-!-margin-top-9">
            <caption className="govuk-heading-s">
              Table showing percentage entering the EBacc
            </caption>
            <thead className="govuk-table__head">
              <tr className="tableizer-firstrow">
                <th className="govuk-table__header govuk-!-width-one-quarter">
                  School type
                </th>
                <th className="govuk-table__header govuk-table__cell--numeric govuk-!-width-one-quarter">
                  2016 revised
                </th>
                <th className="govuk-table__header govuk-table__cell--numeric govuk-!-width-one-quarter">
                  2017 revised
                </th>
              </tr>
            </thead>

            <tbody className="govuk-table__body">
              <tr className="govuk-table__row">
                <td className="govuk-table__cell">All schools</td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  36.8%
                </td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  35.0%
                </td>
              </tr>
              <tr className="govuk-table__row">
                <td className="govuk-table__cell">State-funded schools</td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  39.7%
                </td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  38.2%
                </td>
              </tr>
            </tbody>
          </table>
          <footer>
            <p className="govuk-body-s">
              This measure is calculated using the same methodology as 2016.
            </p>
          </footer>
        </section>

        <section>
          <table className="govuk-table govuk-!-margin-top-9">
            <caption className="govuk-heading-s">
              Table showing percentage achieving the EBacc
            </caption>
            <thead className="govuk-table__head">
              <tr className="tableizer-firstrow">
                <th className="govuk-table__header govuk-!-width-one-quarter">
                  School type
                </th>
                <th className="govuk-table__header govuk-table__cell--numeric govuk-!-width-one-quarter">
                  2016 revised
                </th>
                <th className="govuk-table__header govuk-table__cell--numeric govuk-!-width-one-quarter">
                  2017 revised (9-5 grades in English and maths and A*-C in
                  unreformed subjects)
                </th>
                <th className="govuk-table__header govuk-table__cell--numeric govuk-!-width-one-quarter">
                  (9-4 grades in English and maths and A*-C in unreformed
                  subjects)
                </th>
              </tr>
            </thead>

            <tbody className="govuk-table__body">
              <tr className="govuk-table__row">
                <td className="govuk-table__cell">All schools</td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  23.1%
                </td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  19.7%
                </td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  21.9%
                </td>
              </tr>
              <tr className="govuk-table__row">
                <td className="govuk-table__cell">State-funded schools</td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  24.7%
                </td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  21.3%
                </td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  23.7%
                </td>
              </tr>
            </tbody>
          </table>
          <footer>
            <p className="govuk-body-s">
              The methodology for this measure has changed from 2016 to 2017
            </p>
          </footer>
        </section>
      </div>
    </>
  );
};

export default PrototypeTableSample;
