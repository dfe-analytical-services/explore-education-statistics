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

  const tagLabel = window.location.search.includes('status=approved')
    ? 'Scheduled for publication'
    : 'In higher review';

  const checkStatus = window.location.search.includes('status=approved')
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
