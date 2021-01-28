import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import { useAuthContext } from '@admin/contexts/AuthContext';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import releaseService from '@admin/services/releaseService';
import FormattedDate from '@common/components/FormattedDate';
import LoadingSpinner from '@common/components/LoadingSpinner';
import SanitizeHtml from '@common/components/SanitizeHtml';
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
  const { releaseId } = match.params;

  const { user } = useAuthContext();

  const { value: release, isLoading } = useAsyncHandledRetry(
    () => releaseService.getRelease(releaseId),
    [releaseId],
  );

  return (
    <Page
      wide={false}
      breadcrumbs={
        user && user.permissions.canAccessAnalystPages
          ? [{ name: 'Pre-release access list' }]
          : []
      }
      homePath={user?.permissions.canAccessAnalystPages ? '/' : ''}
      backLink={location.state?.backLink}
    >
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

            <SanitizeHtml dirtyHtml={release?.preReleaseAccessList} />
          </>
        )}
      </LoadingSpinner>
    </Page>
  );
};

export default PreReleaseAccessListPage;
