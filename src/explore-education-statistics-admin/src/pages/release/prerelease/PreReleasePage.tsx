import LoginContext from '@admin/components/Login';
import Page from '@admin/components/Page';
import PublicationReleaseContent from '@admin/modules/find-statistics/PublicationReleaseContent';
import permissionService from '@admin/services/permissions/service';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import { ManageContentPageViewModel } from '@admin/services/release/edit-release/content/types';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import { format } from 'date-fns';
import React, { useContext, useEffect, useState } from 'react';
import { RouteComponentProps, withRouter } from 'react-router';

interface Model {
  preReleaseWindowStatus: PreReleaseWindowStatus;
  content?: ManageContentPageViewModel;
}

interface MatchProps {
  releaseId: string;
}

const PreReleasePage = ({
  handleApiErrors,
  handleManualErrors,
  match,
}: RouteComponentProps<MatchProps> & ErrorControlProps) => {
  const [model, setModel] = useState<Model>();

  const { user } = useContext(LoginContext);

  const { releaseId } = match.params;

  useEffect(() => {
    permissionService
      .getPreReleaseWindowStatus(releaseId)
      .then(preReleaseWindowStatus => {
        if (preReleaseWindowStatus.preReleaseAccess === 'NoneSet') {
          handleManualErrors.forbidden();
        } else if (preReleaseWindowStatus.preReleaseAccess === 'Within') {
          releaseContentService
            .getContent(releaseId)
            .then(content => {
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
            })
            .catch(handleApiErrors);
        } else {
          setModel({
            preReleaseWindowStatus,
          });
        }
      })

      .catch(handleApiErrors);
  }, [releaseId, handleApiErrors, handleManualErrors]);

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
            model.content && (
              <PublicationReleaseContent
                editing={false}
                content={model.content}
                styles={{}}
                onReleaseChange={_ => {}}
                availableDataBlocks={[]}
              />
            )}

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

export default withErrorControl(withRouter(PreReleasePage));
