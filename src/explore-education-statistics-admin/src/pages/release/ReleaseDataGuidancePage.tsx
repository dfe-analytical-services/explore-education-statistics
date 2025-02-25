import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import releaseDataGuidanceService, {
  ReleaseDataGuidance,
} from '@admin/services/releaseDataGuidanceService';
import releaseVersionService, {
  ReleaseVersion,
} from '@admin/services/releaseVersionService';
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
  releaseVersion: ReleaseVersion;
}

const ReleaseDataGuidancePage = ({
  match,
  location,
}: RouteComponentProps<ReleaseRouteParams, StaticContext, LocationState>) => {
  const { releaseVersionId } = match.params;

  const { value: model, isLoading } = useAsyncHandledRetry<Model>(async () => {
    const [dataGuidance, releaseVersion] = await Promise.all([
      releaseDataGuidanceService.getDataGuidance(releaseVersionId),
      releaseVersionService.getReleaseVersion(releaseVersionId),
    ]);

    return {
      dataGuidance,
      releaseVersion,
    };
  }, [releaseVersionId]);

  const { publicAppUrl } = useConfig();

  return (
    <Page wide={false} backLink={location.state?.backLink} homePath="">
      <LoadingSpinner loading={isLoading}>
        {model && (
          <>
            <PageTitle
              title={model.releaseVersion.publicationTitle}
              caption={model.releaseVersion.title}
            />

            <h2>Data guidance</h2>

            <ReleaseDataGuidancePageContent
              published={model.releaseVersion.published}
              dataGuidance={model.dataGuidance.content}
              renderDataCatalogueLink={
                model.releaseVersion.published ? (
                  <Link
                    to={`${publicAppUrl}/data-catalogue?publicationId=${model.releaseVersion.publicationId}&releaseVersionId=${model.releaseVersion.id}`}
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
              dataSets={model.dataGuidance.dataSets}
            />
          </>
        )}
      </LoadingSpinner>
    </Page>
  );
};

export default ReleaseDataGuidancePage;
