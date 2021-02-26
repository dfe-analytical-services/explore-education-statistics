import classNames from 'classnames';
import React from 'react';
import PageSearchForm from '@common/components/PageSearchForm';

const PrototypeHighlightsLinks = () => {
  return (
    <>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-three-quarters">
          <h3 className="govuk-heading-m">Choose a table</h3>
          <div className="govuk-inset-text">
            Use the links below to quickly select existing featured tables for
            this publication. After viewing a table you can also adjust and
            change filters to quickly see different results.
          </div>
          <PageSearchForm inputLabel="Search featured tables" />
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
            <li>
              <a href="#" className="govuk-!-font-weight-bold">
                B2 - Number of new placement for children looked after during
                the year
              </a>
              <p>
                by locality of placement and distance between home and
                placement, in England, 2018 to 2020
              </p>
            </li>
            <li>
              <a href="#" className="govuk-!-font-weight-bold">
                B3 - Duration of placements ceasing during the year
              </a>
              <p>in England, 2018 to 2020</p>
            </li>
            <li>
              <a href="#" className="govuk-!-font-weight-bold">
                B4 - Reason for placement change for children who moved
                placements in the year
              </a>
              <p>in England, 2018 to 2020</p>
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
