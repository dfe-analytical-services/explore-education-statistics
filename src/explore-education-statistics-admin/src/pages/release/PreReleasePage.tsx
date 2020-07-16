import Page from '@admin/components/Page';
import { useAuthContext } from '@admin/contexts/AuthContext';
import useConfig from '@admin/hooks/useConfig';
import ReleaseContentSection from '@admin/pages/release/content/components/ReleaseContentSection';
import { ReleaseContentProvider } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import permissionService, {
  PreReleaseWindowStatus,
} from '@admin/services/permissionService';
import preReleaseService, {
  PreReleaseSummary,
} from '@admin/services/preReleaseService';
import releaseContentService, {
  ReleaseContent,
} from '@admin/services/releaseContentService';
import { useErrorControl } from '@common/contexts/ErrorControlContext';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import { format } from 'date-fns';
import React from 'react';
import { RouteComponentProps } from 'react-router';

interface Model {
  preReleaseWindowStatus: PreReleaseWindowStatus;
  content?: ReleaseContent;
  preReleaseSummary?: PreReleaseSummary;
}

interface MatchProps {
  releaseId: string;
}

const PreReleasePage = ({ match }: RouteComponentProps<MatchProps>) => {
  const { releaseId } = match.params;

  const { value: config } = useConfig();
  const { handleManualErrors, handleApiErrors } = useErrorControl();
  const { user } = useAuthContext();

  const { value: model } = useAsyncRetry<Model | undefined>(async () => {
    try {
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
    } catch (err) {
      handleApiErrors(err);
      return undefined;
    }
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
      homePath={user?.permissions.canAccessAnalystPages ? '/' : ''}
    >
      {preReleaseWindowStatus?.access === 'Within' && content && (
        <ReleaseContentProvider
          value={{
            ...content,
            canUpdateRelease: false,
            unresolvedComments: [],
          }}
        >
          <ReleaseContentSection />
        </ReleaseContentProvider>
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
