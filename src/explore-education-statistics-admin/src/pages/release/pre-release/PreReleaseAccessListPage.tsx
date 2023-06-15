import glossaryService from '@admin/services/glossaryService';
import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import releaseService from '@admin/services/releaseService';
import FormattedDate from '@common/components/FormattedDate';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ContentHtml from '@common/components/ContentHtml';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import { useLocation, useParams } from 'react-router';
import React from 'react';

interface LocationState {
  backLink: string;
}

const PreReleaseAccessListPage = () => {
  const { releaseId } = useParams<ReleaseRouteParams>();
  const location = useLocation<LocationState>();

  const { value: release, isLoading } = useAsyncHandledRetry(
    () => releaseService.getRelease(releaseId),
    [releaseId],
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
