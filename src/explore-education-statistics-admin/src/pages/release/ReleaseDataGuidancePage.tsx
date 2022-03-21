import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import releaseDataGuidanceService, {
  ReleaseDataGuidance,
} from '@admin/services/releaseDataGuidanceService';
import releaseService, { Release } from '@admin/services/releaseService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import ReleaseDataGuidancePageContent from '@common/modules/release/components/ReleaseDataGuidancePageContent';
import { useConfig } from '@admin/contexts/ConfigContext';
import Link from '@admin/components/Link';
import React from 'react';
import { RouteComponentProps, StaticContext } from 'react-router';

interface LocationState {
  backLink: string;
}

interface Model {
  dataGuidance: ReleaseDataGuidance;
  release: Release;
}

const ReleaseDataGuidancePage = ({
  match,
  location,
}: RouteComponentProps<ReleaseRouteParams, StaticContext, LocationState>) => {
  const { releaseId } = match.params;

  const { value: model, isLoading } = useAsyncHandledRetry<Model>(async () => {
    const [dataGuidance, release] = await Promise.all([
      releaseDataGuidanceService.getDataGuidance(releaseId),
      releaseService.getRelease(releaseId),
    ]);

    return {
      dataGuidance,
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

            <ReleaseDataGuidancePageContent
              published={model.release.published}
              dataGuidance={model.dataGuidance.content}
              renderDataCatalogueLink={
                model.release.published ? (
                  <Link
                    to={`${PublicAppUrl}/data-catalogue/${model.release.publicationSlug}/${model.release.slug}`}
                  >
                    data catalogue
                  </Link>
                ) : (
                  <Link to="#">
                    data catalogue (link will become live when release is
                    published)
                  </Link>
                )
              }
              subjects={model.dataGuidance.subjects}
            />
          </>
        )}
      </LoadingSpinner>
    </Page>
  );
};

export default ReleaseDataGuidancePage;
