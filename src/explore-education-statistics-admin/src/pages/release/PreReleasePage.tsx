import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import { useAuthContext } from '@admin/contexts/AuthContext';
import { useConfig } from '@admin/contexts/ConfigContext';
import ReleaseContent from '@admin/pages/release/content/components/ReleaseContent';
import { ReleaseContentProvider } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import permissionService, {
  PreReleaseWindowStatus,
} from '@admin/services/permissionService';
import preReleaseService, {
  PreReleaseSummary,
} from '@admin/services/preReleaseService';
import releaseContentService, {
  ReleaseContent as ReleaseContentModel,
} from '@admin/services/releaseContentService';
import { useErrorControl } from '@common/contexts/ErrorControlContext';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import { format } from 'date-fns';
import React from 'react';
import { RouteComponentProps } from 'react-router';

interface Model {
  preReleaseWindowStatus: PreReleaseWindowStatus;
  content?: ReleaseContentModel;
  preReleaseSummary?: PreReleaseSummary;
}

interface MatchProps {
  releaseId: string;
}

const PreReleasePage = ({ match }: RouteComponentProps<MatchProps>) => {
  const { releaseId } = match.params;

  const config = useConfig();
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
          ? [{ name: 'Pre-release access' }]
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
          <PageTitle
            caption={content.release.title}
            title={content.release.publication.title}
          />

          <ReleaseContent />
        </ReleaseContentProvider>
      )}

      {preReleaseWindowStatus?.access === 'Before' && (
        <>
          <h1>Pre-release access is not yet available</h1>

          {preReleaseSummary && (
            <>
              <p>
                Pre-release access for the{' '}
                <strong>{preReleaseSummary.releaseTitle}</strong> release of{' '}
                <strong>{preReleaseSummary.publicationTitle}</strong> is not yet
                available.
              </p>
              <p>
                Pre-release access will be available from{' '}
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
          <h1>Pre-release access has ended</h1>

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
