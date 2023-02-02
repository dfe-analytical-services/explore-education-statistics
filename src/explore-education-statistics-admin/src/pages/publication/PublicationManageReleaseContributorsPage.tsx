import PublicationReleaseContributorsForm from '@admin/pages/publication/components/PublicationReleaseContributorsForm';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import { PublicationManageTeamRouteParams } from '@admin/routes/publicationRoutes';
import releasePermissionService, {
  UserReleaseRole,
} from '@admin/services/releasePermissionService';
import releaseService, { Release } from '@admin/services/releaseService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { RouteComponentProps } from 'react-router';

interface Model {
  release: Release;
  publicationContributors: UserReleaseRole[];
  releaseContributors: UserReleaseRole[];
}

const PublicationManageReleaseContributorsPage = ({
  match,
}: RouteComponentProps<PublicationManageTeamRouteParams>) => {
  const { publicationId } = usePublicationContext();

  const { releaseId } = match.params;

  const { value, isLoading } = useAsyncHandledRetry<Model>(async () => {
    const [release, publicationContributors, releaseRoles] = await Promise.all([
      releaseService.getRelease(releaseId),
      releasePermissionService.listPublicationContributors(publicationId),
      releasePermissionService.listRoles(releaseId),
    ]);
    return {
      release,
      publicationContributors,
      releaseContributors: releaseRoles.filter(
        role => role.role === 'Contributor',
      ),
    };
  }, [publicationId, releaseId]);

  const { release, publicationContributors = [], releaseContributors = [] } =
    value ?? {};

  return (
    <LoadingSpinner loading={isLoading}>
      <h2>{`Manage release contributors (${release?.title})`}</h2>

      <PublicationReleaseContributorsForm
        publicationId={publicationId}
        releaseId={releaseId}
        publicationContributors={publicationContributors}
        releaseContributors={releaseContributors}
      />
    </LoadingSpinner>
  );
};

export default PublicationManageReleaseContributorsPage;
