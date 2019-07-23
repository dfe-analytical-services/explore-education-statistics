import PrototypeDashboardRelease from '@admin/pages/prototypes/components/PrototypeDashboardRelease';
import { LoginContext } from '@admin/components/Login';
import React from 'react';

interface Props {
  task?: string;
  reviewType?: string;
}

const AdminDashboardReadyForApproval = ({ task, reviewType }: Props) => {
  const userContext = React.useContext(LoginContext);

  const tagLabel =
    userContext.user &&
    userContext.user.permissions.includes('responsible statistician')
      ? 'Ready for final sign off'
      : 'Ready for you to review';

  const checkStatus = window.location.search.includes('?status=readyApproval')
    ? 'checkReleases'
    : 'noReleases';

  return (
    <>
      {checkStatus === 'noReleases' && (
        <>
          <div className="govuk-inset-text">
            {task === 'readyReview'
              ? 'There are currenly no releases ready for you to review'
              : 'There are currently no unresolved comments'}
          </div>
        </>
      )}
      {checkStatus === 'checkReleases' && (
        <>
          {userContext.user &&
            userContext.user.permissions.includes('team member') && (
              <p>Level 1: peer review</p>
            )}
          {userContext.user &&
            userContext.user.permissions.includes(
              'responsible statistician',
            ) && <p>Level 2: higher review</p>}

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
            tag={task === 'readyReview' ? tagLabel : 'Unresolved comments'}
            review
            lastEdited={new Date('2019-03-20 17:37')}
            lastEditor={{ id: 'me', name: 'me', permissions: [] }}
            published={new Date('2019-09-20 09:30')}
            nextRelease={new Date('2020-09-20 09:30')}
            showComments
            task={task}
          />
        </>
      )}
    </>
  );
};

export default AdminDashboardReadyForApproval;
