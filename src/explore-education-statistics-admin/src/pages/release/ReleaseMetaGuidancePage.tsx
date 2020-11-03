import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import { useAuthContext } from '@admin/contexts/AuthContext';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import releaseMetaGuidanceService, {
  ReleaseMetaGuidance,
} from '@admin/services/releaseMetaGuidanceService';
import releaseService, { Release } from '@admin/services/releaseService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import ReleaseMetaGuidancePageContent from '@common/modules/release/components/ReleaseMetaGuidancePageContent';
import React from 'react';
import { RouteComponentProps, StaticContext } from 'react-router';

interface LocationState {
  backLink: string;
}

interface Model {
  metaGuidance: ReleaseMetaGuidance;
  release: Release;
}

const ReleaseMetaGuidancePage = ({
  match,
  location,
}: RouteComponentProps<ReleaseRouteParams, StaticContext, LocationState>) => {
  const { releaseId } = match.params;

  const { user } = useAuthContext();

  const { value: model, isLoading } = useAsyncHandledRetry<Model>(async () => {
    const [metaGuidance, release] = await Promise.all([
      releaseMetaGuidanceService.getMetaGuidance(releaseId),
      releaseService.getRelease(releaseId),
    ]);

    return {
      metaGuidance,
      release,
    };
  }, [releaseId]);

  return (
    <Page
      wide
      breadcrumbs={
        user && user.permissions.canAccessAnalystPages
          ? [{ name: 'Metadata guidance document' }]
          : []
      }
      homePath={user?.permissions.canAccessAnalystPages ? '/' : ''}
      backLink={location.state?.backLink}
    >
      <LoadingSpinner loading={isLoading}>
        {model && (
          <>
            <PageTitle
              title={model.release.publicationTitle}
              caption={model.release.title}
            />

            <h2>Metadata guidance document</h2>

            <ReleaseMetaGuidancePageContent
              published={model.release.published}
              metaGuidance={model.metaGuidance.content}
              subjects={model.metaGuidance.subjects}
            />
          </>
        )}
      </LoadingSpinner>
    </Page>
  );
};

export default ReleaseMetaGuidancePage;
