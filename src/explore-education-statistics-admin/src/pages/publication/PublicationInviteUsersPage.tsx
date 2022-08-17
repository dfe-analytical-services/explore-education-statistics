import PublicationInviteNewUsersForm from '@admin/pages/publication/components/PublicationInviteNewUsersForm';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import { PublicationManageTeamRouteParams } from '@admin/routes/publicationRoutes';
import publicationService from '@admin/services/publicationService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { RouteComponentProps } from 'react-router';

const PublicationInviteUsersPage = ({
  match,
}: RouteComponentProps<PublicationManageTeamRouteParams>) => {
  const { publicationId, publication } = usePublicationContext();

  const { releaseId } = match.params;

  const {
    value: allReleases = { results: [] },
    isLoading,
  } = useAsyncHandledRetry(() =>
    publicationService.listReleases(publicationId),
  );

  const releases = allReleases.results;

  return (
    <LoadingSpinner loading={isLoading}>
      <PublicationInviteNewUsersForm
        publication={publication}
        releases={releases}
        releaseId={releaseId}
      />
    </LoadingSpinner>
  );
};

export default PublicationInviteUsersPage;
