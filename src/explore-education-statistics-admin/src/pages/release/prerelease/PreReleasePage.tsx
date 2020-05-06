import Page from '@admin/components/Page';
import { useAuthContext } from '@admin/contexts/AuthContext';
import PublicationReleaseContent from '@admin/modules/find-statistics/PublicationReleaseContent';
import { ReleaseProvider } from '@admin/pages/release/edit-release/content/ReleaseContext';
import commonService from '@admin/services/common/service';
import permissionService, {
  PreReleaseWindowStatus,
} from '@admin/services/permissions/permissionService';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import { ManageContentPageViewModel } from '@admin/services/release/edit-release/content/types';
import { useErrorControl } from '@common/contexts/ErrorControlContext';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import { format } from 'date-fns';
import React from 'react';
import { RouteComponentProps } from 'react-router';
import { BasicPublicationDetails } from 'src/services/common/types';

interface Model {
  preReleaseWindowStatus: PreReleaseWindowStatus;
  content: ManageContentPageViewModel;
  publication: BasicPublicationDetails;
}

interface MatchProps {
  releaseId: string;
  publicationId: string;
}

const PreReleasePage = ({ match }: RouteComponentProps<MatchProps>) => {
  const { releaseId, publicationId } = match.params;

  const { handleManualErrors } = useErrorControl();
  const { user } = useAuthContext();

  const { value: model } = useAsyncRetry<Model | undefined>(async () => {
    const preReleaseWindowStatus = await permissionService.getPreReleaseWindowStatus(
      releaseId,
    );
    if (preReleaseWindowStatus.preReleaseAccess === 'NoneSet') {
      handleManualErrors.forbidden();
      return undefined;
    }

    const [publication, content] = await Promise.all([
      commonService.getBasicPublicationDetails(publicationId),
      releaseContentService.getContent(releaseId),
    ]);

    return {
      publication,
      preReleaseWindowStatus,
      content,
    };
  }, [handleManualErrors, publicationId, releaseId]);

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
              <ReleaseProvider
                value={{
                  ...model?.content,
                  canUpdateRelease: false,
                  unresolvedComments: [],
                }}
              >
                <PublicationReleaseContent />
              </ReleaseProvider>
            )}

          {model.preReleaseWindowStatus.preReleaseAccess === 'Before' && (
            <>
              <h1 className="govuk-heading-l">
                Pre Release access is not yet available
              </h1>
              <p className="govuk-body">
                Pre Release access for the{' '}
                <strong>{model.content.release.title}</strong> release of{' '}
                <strong>{model.publication.title}</strong> is not yet available.
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
                Pre Release access for the{' '}
                <strong>{model.content.release.title}</strong> release of{' '}
                <strong>{model.publication.title}</strong> is no longer
                available.
              </p>
            </>
          )}
        </Page>
      )}
    </>
  );
};

export default PreReleasePage;
