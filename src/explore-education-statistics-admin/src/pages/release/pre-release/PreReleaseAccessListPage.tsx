import glossaryService from '@admin/services/glossaryService';
import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import releaseVersionService from '@admin/services/releaseVersionService';
import FormattedDate from '@common/components/FormattedDate';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ContentHtml from '@common/components/ContentHtml';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { RouteComponentProps, StaticContext } from 'react-router';

interface LocationState {
  backLink: string;
}

const PreReleaseAccessListPage = ({
  match,
  location,
}: RouteComponentProps<ReleaseRouteParams, StaticContext, LocationState>) => {
  const { releaseVersionId } = match.params;

  const { value: release, isLoading } = useAsyncHandledRetry(
    () => releaseVersionService.getReleaseVersion(releaseVersionId),
    [releaseVersionId],
  );

  return (
    <Page wide={false} backLink={location.state?.backLink} homePath="">
      <LoadingSpinner loading={isLoading}>
        {release && (
          <>
            <PageTitle
              title={release.publicationTitle}
              caption={release.title}
            />

            <h2>Pre-release access list</h2>

            {release.published && (
              <p>
                <strong>
                  Published <FormattedDate>{release.published}</FormattedDate>
                </strong>
              </p>
            )}

            <ContentHtml
              html={release?.preReleaseAccessList}
              getGlossaryEntry={glossaryService.getEntry}
            />
          </>
        )}
      </LoadingSpinner>
    </Page>
  );
};

export default PreReleaseAccessListPage;
