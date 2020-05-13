import Page from '@admin/components/Page';
import { useAuthContext } from '@admin/contexts/AuthContext';
import useConfig from '@admin/hooks/useConfig';
import PublicationReleaseContent from '@admin/modules/find-statistics/PublicationReleaseContent';
import { ReleaseProvider } from '@admin/pages/release/edit-release/content/ReleaseContext';
import permissionService, {
  PreReleaseWindowStatus,
} from '@admin/services/permissions/permissionService';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import preReleaseService, {
  PreReleaseSummary,
} from '@admin/services/pre-release/preReleaseService';
import { ManageContentPageViewModel } from '@admin/services/release/edit-release/content/types';
import { useErrorControl } from '@common/contexts/ErrorControlContext';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import { format } from 'date-fns';
import React from 'react';
import { RouteComponentProps } from 'react-router';

interface Model {
  preReleaseWindowStatus: PreReleaseWindowStatus;
  content?: ManageContentPageViewModel;
  preReleaseSummary?: PreReleaseSummary;
}

interface MatchProps {
  releaseId: string;
}

const PreReleasePage = ({ match }: RouteComponentProps<MatchProps>) => {
  const { releaseId } = match.params;

  const { value: config } = useConfig();
  const { handleManualErrors } = useErrorControl();
  const { user } = useAuthContext();

  const { value: model } = useAsyncRetry<Model | undefined>(async () => {
    const preReleaseWindowStatus = await permissionService.getPreReleaseWindowStatus(
      releaseId,
    );

    if (preReleaseWindowStatus.access === 'NoneSet') {
      handleManualErrors.forbidden();
      return undefined;
    }

    if (preReleaseWindowStatus.access === 'Within') {
      const content = await releaseContentService.getContent(releaseId);

      return {
        preReleaseWindowStatus,
        content,
      };
    }

    const preReleaseSummary = await preReleaseService.getPreReleaseSummary(
      releaseId,
    );

    return {
      preReleaseWindowStatus,
      preReleaseSummary,
    };
  }, [handleManualErrors, releaseId]);

  const { content, preReleaseSummary, preReleaseWindowStatus } = model ?? {};

  return (
    <Page
      wide
      breadcrumbs={
        user && user.permissions.canAccessAnalystPages
          ? [{ name: 'Pre Release access' }]
          : []
      }
      includeHomeBreadcrumb={user && user.permissions.canAccessAnalystPages}
    >
      {preReleaseWindowStatus?.access === 'Within' && content && (
        <ReleaseProvider
          value={{
            ...content,
            canUpdateRelease: false,
            unresolvedComments: [],
          }}
        >
          <PublicationReleaseContent />
        </ReleaseProvider>
      )}

      {preReleaseWindowStatus?.access === 'Before' && (
        <>
          <h1>Pre Release access is not yet available</h1>

          {preReleaseSummary && (
            <>
              <p>
                Pre Release access for the{' '}
                <strong>{preReleaseSummary.releaseTitle}</strong> release of{' '}
                <strong>{preReleaseSummary.publicationTitle}</strong> is not yet
                available.
              </p>
              <p>
                Pre Release access will be available from{' '}
                {format(preReleaseWindowStatus.start, 'd MMMM yyyy')}
                {' at '}
                {format(preReleaseWindowStatus.start, 'HH:mm')} until{' '}
                {format(preReleaseWindowStatus.end, 'd MMMM yyyy')}
                {' at '}
                {format(preReleaseWindowStatus.end, 'HH:mm')}.
              </p>
              <p>
                If you believe that this release should be available and you are
                having problems accessing please contact the{' '}
                <a href={`mailto:${preReleaseSummary.contactEmail}`}>
                  production team
                </a>
                .
              </p>
            </>
          )}
        </>
      )}

      {preReleaseWindowStatus?.access === 'After' && (
        <>
          <h1>Pre Release access has ended</h1>

          {preReleaseSummary && (
            <>
              <p>
                The <strong>{preReleaseSummary.releaseTitle}</strong> release of{' '}
                <strong>{preReleaseSummary.publicationTitle}</strong> has now
                been published on the Explore Education Statistics service.
              </p>

              {config?.PublicAppUrl && (
                <p>
                  View this{' '}
                  <a
                    href={`${config.PublicAppUrl}/find-statistics/${preReleaseSummary.publicationSlug}/${preReleaseSummary.releaseSlug}`}
                    rel="noopener noreferrer"
                  >
                    release
                  </a>
                  .
                </p>
              )}
            </>
          )}
        </>
      )}
    </Page>
  );
};

export default PreReleasePage;
