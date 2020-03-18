import Page from '@admin/components/Page';
import { useAuthContext } from '@admin/contexts/AuthContext';
import PublicationReleaseContent from '@admin/modules/find-statistics/PublicationReleaseContent';
import permissionService, {
  PreReleaseWindowStatus,
} from '@admin/services/permissions/permissionService';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import { ManageContentPageViewModel } from '@admin/services/release/edit-release/content/types';
import {
  ErrorControlState,
  useErrorControl,
} from '@common/contexts/ErrorControlContext';
import { format } from 'date-fns';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps, withRouter } from 'react-router';

interface Model {
  preReleaseWindowStatus: PreReleaseWindowStatus;
  content?: ManageContentPageViewModel;
}

interface MatchProps {
  releaseId: string;
}

const PreReleasePage = ({
  match,
}: RouteComponentProps<MatchProps> & ErrorControlState) => {
  const { handleManualErrors } = useErrorControl();
  const [model, setModel] = useState<Model>();

  const { user } = useAuthContext();

  const { releaseId } = match.params;

  useEffect(() => {
    permissionService
      .getPreReleaseWindowStatus(releaseId)
      .then(preReleaseWindowStatus => {
        if (preReleaseWindowStatus.preReleaseAccess === 'NoneSet') {
          handleManualErrors.forbidden();
        } else if (preReleaseWindowStatus.preReleaseAccess === 'Within') {
          releaseContentService.getContent(releaseId).then(content => {
            const newContent = {
              ...content,
              release: {
                ...content.release,
                prerelease: true,
              },
            };

            setModel({
              preReleaseWindowStatus,
              content: newContent,
            });
          });
        } else {
          setModel({
            preReleaseWindowStatus,
          });
        }
      });
  }, [releaseId, handleManualErrors]);

  return (
    <>
      {model && (
        <Page
          wide
          breadcrumbs={
            user && user.permissions.canAccessAnalystPages
              ? [{ name: 'Pre Release access' }]
              : []
          }
          includeHomeBreadcrumb={user && user.permissions.canAccessAnalystPages}
        >
          {model.preReleaseWindowStatus.preReleaseAccess === 'Within' &&
            model.content && <PublicationReleaseContent />}

          {model.preReleaseWindowStatus.preReleaseAccess === 'Before' && (
            <>
              <h1 className="govuk-heading-l">
                Pre Release access is not yet available
              </h1>
              <p className="govuk-body">
                Pre Release access is not yet available for this release.
              </p>
              <p className="govuk-body">
                Pre Release access will be available from{' '}
                {format(
                  model.preReleaseWindowStatus.preReleaseWindowStartTime,
                  'd MMMM yyyy',
                )}
                {' at '}
                {format(
                  model.preReleaseWindowStatus.preReleaseWindowStartTime,
                  'HH:mm',
                )}{' '}
                until{' '}
                {format(
                  model.preReleaseWindowStatus.preReleaseWindowEndTime,
                  'd MMMM yyyy',
                )}
                {' at '}
                {format(
                  model.preReleaseWindowStatus.preReleaseWindowEndTime,
                  'HH:mm',
                )}
                .
              </p>
              <p className="govuk-body">Please try again later.</p>
            </>
          )}

          {model.preReleaseWindowStatus.preReleaseAccess === 'After' && (
            <>
              <h1 className="govuk-heading-l">Pre Release access has ended</h1>
              <p className="govuk-body">
                Pre Release access is no longer available for this release.
              </p>
            </>
          )}
        </Page>
      )}
    </>
  );
};

export default withRouter(PreReleasePage);
