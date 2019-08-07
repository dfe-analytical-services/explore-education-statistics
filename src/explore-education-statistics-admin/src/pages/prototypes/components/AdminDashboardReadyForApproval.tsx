import PrototypeDashboardRelease from '@admin/pages/prototypes/components/PrototypeDashboardRelease';
import { LoginContext } from '@admin/components/Login';
import React from 'react';

interface Props {
  task?: string;
  reviewType?: string;
  user?: string;
}

const AdminDashboardReadyForApproval = ({ task, user }: Props) => {
  const userContext = React.useContext(LoginContext);

  const tagLabel = window.location.search.includes('status=readyHigherReview')
    ? 'Ready for sign-off'
    : 'In draft';

  const checkStatus = window.location.search.includes('status=ready')
    ? 'checkReleases'
    : 'noReleases';

  return (
    <>
      {checkStatus === 'noReleases' && (
        <>
          <div className="govuk-inset-text">
            {task === 'readyReview'
              ? 'There are currenly no releases ready for you to review'
              : 'There are currently no draft releases'}
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

          {/*<p className="govuk-body">
            {task === 'readyReview' ? (
              'Review the following draft releases.'
            ) : (
              <div className="govuk-warning-text">
                <span className="govuk-warning-text__icon" aria-hidden="true">
                  !
                </span>
                <strong className="govuk-warning-text__text">
                  <span className="govuk-warning-text__assistive">Warning</span>
                  Resolve comments in the following draft releases
                </strong>
              </div>
            )}
            </p>*/}

          <hr />

          <h3 className="govuk-heading-m">
            Pupil absence statistics and data for schools in England
          </h3>
          <PrototypeDashboardRelease
            title="Academic year,"
            years="2018 to 2019"
            tag={tagLabel}
            review
            lastEdited={new Date('2019-03-20 17:37')}
            lastEditor={{ id: 'me', name: 'me', permissions: [] }}
            published={new Date('2019-09-20 09:30')}
            nextRelease={new Date('2020-09-20 09:30')}
            showComments
            task={task}
            user={user}
            isNew
          />
        </>
      )}
    </>
  );
};

export default AdminDashboardReadyForApproval;
