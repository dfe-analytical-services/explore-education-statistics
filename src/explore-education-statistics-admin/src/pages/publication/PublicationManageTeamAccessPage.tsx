import Page from '@admin/components/Page';
import { PublicationRouteParams } from '@admin/routes/routes';
import publicationService from '@admin/services/publicationService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { RouteComponentProps } from 'react-router';
import TabsSection from '@common/components/TabsSection';
import Tabs from '@common/components/Tabs';
import PublicationManageTeamAccessTab from '@admin/pages/publication/components/PublicationManageTeamAccessTab';
import PublicationInviteNewUsersTab from '@admin/pages/publication/components/PublicationInviteNewUsersTab';

const PublicationManageTeamAccessPage = ({
  match,
}: RouteComponentProps<PublicationRouteParams>) => {
  const { publicationId } = match.params;

  const { value: publication, isLoading } = useAsyncHandledRetry(
    () => publicationService.getPublication(publicationId),
    [publicationId],
  );

  if (isLoading) {
    return <LoadingSpinner />;
  }

  if (!publication) {
    return null;
  }

  return (
    <Page
      title="Manage team access"
      caption={publication.title}
      breadcrumbs={[{ name: 'Manage team access' }]}
    >
      <Tabs id="manageTeamAccessTabs">
        <TabsSection id="manageTeamAccessTab" title="Manage team access">
          <PublicationManageTeamAccessTab publication={publication} />
        </TabsSection>
        <TabsSection id="inviteNewUsersTab" title="Invite new users">
          <PublicationInviteNewUsersTab publication={publication} />
        </TabsSection>
      </Tabs>
    </Page>
  );
};

export default PublicationManageTeamAccessPage;
