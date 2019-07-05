import PrototypeDashboardRelease from '@admin/pages/prototypes/components/PrototypeDashboardRelease';
import React from 'react';

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
          <h2 className="govuk-heading-l govuk-!-margin-bottom-0">
            Pupils and schools, pupil absence
          </h2>
          <p className="govuk-body">
            Please review the following draft release(s)
          </p>

          <hr />

          <h3 className="govuk-heading-m">
            Pupil absence statistics and data for schools in England
          </h3>
          <PrototypeDashboardRelease
            title="Academic year,"
            years="2018 to 2019"
            tag="Ready to review"
            review
            lastEdited={new Date('2019-03-20 17:37')}
            lastEditor={{ id: 'me', name: 'me', permissions: [] }}
            published={new Date('2019-09-20 09:30')}
            nextRelease={new Date('2020-09-20 09:30')}
            showComments
            dataType="Revised"
          />
        </>
      )}
    </>
  );
};

export default AdminDashboardReadyForApproval;
