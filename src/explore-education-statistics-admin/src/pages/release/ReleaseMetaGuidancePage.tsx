import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import releaseMetaGuidanceService, {
  ReleaseMetaGuidance,
} from '@admin/services/releaseMetaGuidanceService';
import releaseService, { Release } from '@admin/services/releaseService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import ReleaseMetaGuidancePageContent from '@common/modules/release/components/ReleaseMetaGuidancePageContent';
import { useConfig } from '@admin/contexts/ConfigContext';
import Link from '@admin/components/Link';
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

  const { PublicAppUrl } = useConfig();

  return (
    <Page wide={false} backLink={location.state?.backLink} homePath="">
      <LoadingSpinner loading={isLoading}>
        {model && (
          <>
            <PageTitle
              title={model.release.publicationTitle}
              caption={model.release.title}
            />

            <h2>Data guidance</h2>

            <ReleaseMetaGuidancePageContent
              published={model.release.published}
              metaGuidance={model.metaGuidance.content}
              renderDataCatalogueLink={
                model.release.published ? (
                  <Link
                    to={`${PublicAppUrl}/data-catalogue/${model.release.publicationSlug}/${model.release.slug}`}
                  >
                    data catalogue
                  </Link>
                ) : (
                  'data catalogue'
                )
              }
              subjects={model.metaGuidance.subjects}
            />
          </>
        )}
      </LoadingSpinner>
    </Page>
  );
};

export default ReleaseMetaGuidancePage;
