import Page from '@admin/components/Page';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { RouteComponentProps } from 'react-router';
import PublicationAddExistingUsersTab from '@admin/pages/publication/components/PublicationAddExistingUsersTab';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import releasePermissionService, {
  ManageAccessPageContributor,
} from '@admin/services/releasePermissionService';
import publicationService, {
  BasicPublicationDetails,
} from '@admin/services/publicationService';
import releaseService, { Release } from '@admin/services/releaseService';

interface Model {
  publication: BasicPublicationDetails;
  release: Release;
  contributors: ManageAccessPageContributor[];
}

const ReleaseManageTeamAccessAddUsersPage = ({
  match,
}: RouteComponentProps<ReleaseRouteParams>) => {
  const { publicationId, releaseId } = match.params;

  const { value: model, isLoading } = useAsyncHandledRetry(async () => {
    const [publication, release, contributors] = await Promise.all([
      publicationService.getPublication(publicationId),
      releaseService.getRelease(releaseId),
      releasePermissionService.getAllPublicationContributors(releaseId),
    ]);
    return { publication, release, contributors } as Model;
  }, [publicationId, releaseId]);

  if (!model || isLoading) {
    return <LoadingSpinner />;
  }

  const { publication, release, contributors } = model;

  return (
    <LoadingSpinner loading={!contributors && isLoading}>
      <Page
        title="Add or remove existing users"
        caption={publication.title}
        breadcrumbs={[{ name: 'Add or remove existing users' }]}
      >
        <PublicationAddExistingUsersTab
          publicationId={publicationId}
          release={release}
          contributors={contributors}
        />
      </Page>
    </LoadingSpinner>
  );
};

export default ReleaseManageTeamAccessAddUsersPage;
