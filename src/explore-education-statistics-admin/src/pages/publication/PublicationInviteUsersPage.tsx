import PublicationInviteNewUsersForm from '@admin/pages/publication/components/PublicationInviteNewUsersForm';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import { PublicationManageTeamRouteParams } from '@admin/routes/publicationRoutes';
import publicationService from '@admin/services/publicationService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { useParams } from 'react-router';

const PublicationInviteUsersPage = () => {
  const { publicationId, publication } = usePublicationContext();
  const { releaseId } = useParams<PublicationManageTeamRouteParams>();

  const { value: allReleases = { results: [] }, isLoading } =
    useAsyncHandledRetry(() => publicationService.listReleases(publicationId));

  const { results: releases } = allReleases;

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
