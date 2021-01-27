import classNames from 'classnames';
import React from 'react';

const PrototypeHighlightsLinks = () => {
  return (
    <>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-three-quarters">
          <h3 className="govuk-heading-s">View popular tables</h3>
          <div className="govuk-inset-text">
            Use the links below to quickly select existing popular tables for
            this publication, you can also download the data in .xls and .csv
            formats. After viewing you can also adjust and change filters to
            quickly see different results.
          </div>
          <ul
            className={classNames(
              'govuk-list',
              'govuk-list--bullet',
              'govuk-list--spaced',
              'govuk-!-margin-bottom-9',
            )}
          >
            <li>
              <a href="#" className="govuk-!-font-weight-bold">
                A1 - Children looked after at 31 March{' '}
              </a>
              <p>
                by gender, age at 31 March, category of need, ethnic origin,
                legal status and motherhood status, in England, 2018 to 2020
              </p>
            </li>
            <li>
              <a href="#" className="govuk-!-font-weight-bold">
                A2 - Children looked after at 31 March
              </a>
              <p>by placement, in England, 2018 to 2020</p>
            </li>
            <li>
              <a href="#" className="govuk-!-font-weight-bold">
                A3 - Unaccompanied asylum-seeking children looked after at 31
                March{' '}
              </a>
              <p>
                by gender, age at 31 March, category of need and ethnic origin,
                in England, 2018 to 2020
              </p>
            </li>
            <li>
              <a href="#" className="govuk-!-font-weight-bold">
                A4 - Children looked after at 31 March{' '}
              </a>
              <p>
                by distance between home and placement and locality of
                placement, in England, 2018 to 2020
              </p>
            </li>
            <li>
              <a href="#" className="govuk-!-font-weight-bold">
                A5 - Children looked after at 31 March
              </a>
              <p>
                by placement, placement location and placement provider, in
                England, 2018 to 2020
              </p>
            </li>
            <li>
              <a href="#" className="govuk-!-font-weight-bold">
                B1 - Children looked after at any time during the year ending 31
                March{' '}
              </a>
              <p>
                and those looked after continuously for at least 12 months at 31
                March, in England, 2018 to 2020
              </p>
            </li>
          </ul>
          <p>
            If you can't find the table you're looking for, then you can{' '}
            <a href="#test-2">create your own</a>
          </p>
        </div>
      </div>
    </>
  );
};

export default PrototypeHighlightsLinks;
