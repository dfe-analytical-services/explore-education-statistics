import React from 'react';
import Link from '@admin/components/Link';

const AdminDashboardReadyForApproval = () => {
  return (
    <>
      {window.location.search !== '?status=readyApproval' && (
        <div className="govuk-inset-text">
          There are currenly no releases ready for approval
        </div>
      )}
      {window.location.search === '?status=readyApproval' && (
        <>
          <h2 className="govuk-heading-m">Ready for approval</h2>
          <ul className="govuk-list-bullet">
            <li>
              {' '}
              <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
                Pupil absence statistics
              </h4>
              <dl className="dfe-meta-content govuk-!-margin-0">
                <dt className="govuk-caption-m">Last edited: </dt>
                <dd>
                  20 March 2019 at 17:37 by <a href="#">me</a>
                </dd>
              </dl>
              <div className="govuk-!-margin-top-0">
                <Link to="/prototypes/publication-create-new-absence-config">
                  Edit
                </Link>
              </div>
            </li>
          </ul>
        </>
      )}
    </>
  );
};

export default AdminDashboardReadyForApproval;
