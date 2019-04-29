import React from 'react';
import Link from '@admin/components/Link';

const AdminDashboardInProgress = () => {
  return (
    <>
      {window.location.search === '?status=editNewRelease' && (
        <>
          <h2 className="govuk-heading-m">New releases in progress</h2>
          <ul className="govuk-list-bullet  govuk-!-margin-bottom-9">
            <li>
              {' '}
              <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
                Pupil absence statistics
              </h4>
              <dl className="dfe-meta-content govuk-!-margin-0">
                <dt className="govuk-caption-m">Last edited:</dt>
                <dd>
                  20 March 2019 at 17:37 by <a href="#">me</a>
                </dd>
              </dl>
              <div className="govuk-!-margin-top-0">
                <Link to="/prototypes/publication-create-new-absence-config">
                  Edit release
                </Link>
              </div>
            </li>
          </ul>
        </>
      )}
      <h2 className="govuk-heading-m">Editing current releases</h2>
      <ul className="govuk-list-bullet">
        <li className="govuk-!-margin-top-6">
          <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
            Permanent and fixed-period exclusions statistics
          </h4>
          <dl className="dfe-meta-content govuk-!-margin-0">
            <dt className="govuk-caption-m">Last edited:</dt>
            <dd>
              24 April 2019 at 10:37 <a href="#">me</a>
            </dd>
          </dl>
          <div className="govuk-!-margin-top-0">
            <Link to="#">Edit</Link>
          </div>
        </li>
      </ul>
    </>
  );
};

export default AdminDashboardInProgress;
