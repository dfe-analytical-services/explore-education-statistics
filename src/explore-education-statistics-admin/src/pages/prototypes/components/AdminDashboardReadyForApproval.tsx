import PrototypeDashboardRelease from '@admin/pages/prototypes/components/PrototypeDashboardRelease';
import React from 'react';

interface Props {
  task?: string;
}

const AdminDashboardReadyForApproval = ({ task }: Props) => {
  return (
    <>
      {window.location.search !== '?status=readyApproval' && (
        <>
          <div className="govuk-inset-text">
            {task === 'readyReview'
              ? 'There are currenly no releases ready for you to review'
              : 'There are currently no unresolved comments'}
          </div>
        </>
      )}
      {window.location.search === '?status=readyApproval' && (
        <>
          <p className="govuk-body">
            {task === 'readyReview' ? (
              'Please review the following draft release(s)'
            ) : (
              <div className="govuk-warning-text">
                <span className="govuk-warning-text__icon" aria-hidden="true">
                  !
                </span>
                <strong className="govuk-warning-text__text">
                  <span className="govuk-warning-text__assistive">Warning</span>
                  There are unresolved comments requiring your attention
                </strong>
              </div>
            )}
          </p>

          <hr />

          <h3 className="govuk-heading-m">
            Pupil absence statistics and data for schools in England
          </h3>
          <PrototypeDashboardRelease
            title="Academic year,"
            years="2018 to 2019"
            tag={
              task === 'readyReview'
                ? 'Ready for you to review'
                : 'Unresolved comments'
            }
            review
            lastEdited={new Date('2019-03-20 17:37')}
            lastEditor={{ id: 'me', name: 'me', permissions: [] }}
            published={new Date('2019-09-20 09:30')}
            nextRelease={new Date('2020-09-20 09:30')}
            showComments
            dataType="Revised"
            task={task}
          />
        </>
      )}
    </>
  );
};

export default AdminDashboardReadyForApproval;
