import Page from '@admin/components/Page';
import { PublicationRouteParams } from '@admin/routes/routes';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { RouteComponentProps } from 'react-router';
import TabsSection from '@common/components/TabsSection';
import Tabs from '@common/components/Tabs';
import PublicationManageTeamAccessTab from '@admin/pages/publication/components/PublicationManageTeamAccessTab';
import PublicationInviteNewUsersTab from '@admin/pages/publication/components/PublicationInviteNewUsersTab';
import releasePermissionService, {
  ManageAccessPageRelease,
} from '@admin/services/releasePermissionService';

const PublicationManageTeamAccessPage = ({
  match,
}: RouteComponentProps<PublicationRouteParams>) => {
  const { publicationId } = match.params;

  const {
    value: viewModel,
    isLoading,
    setState: setViewModel,
  } = useAsyncHandledRetry(
    () => releasePermissionService.getPublicationContributors(publicationId),
    [publicationId],
  );

  const handleChange = (
    release: ManageAccessPageRelease,
    addUser?: string,
    removeUser?: string,
    totalRemoval?: boolean,
  ) => {
    if (!viewModel) {
      return;
    }

    if (removeUser && !totalRemoval) {
      viewModel.releases = viewModel.releases.map(r => {
        const newRelease: ManageAccessPageRelease = {
          releaseId: r.releaseId,
          releaseTitle: r.releaseTitle,
          userList: r.userList.filter(u => u.userId === removeUser),
        };
        return newRelease;
      });
    }

    viewModel.releases = viewModel.releases.map(r =>
      r.releaseId !== release.releaseId ? r : release,
    );
    setViewModel({ value: viewModel });
  };

  if (!viewModel) {
    return null;
  }

  return (
    <LoadingSpinner loading={isLoading}>
      <Page
        title="Manage team access"
        caption={viewModel.publicationTitle}
        breadcrumbs={[{ name: 'Manage team access' }]}
      >
        <Tabs id="manageTeamAccessTabs">
          <TabsSection id="manage-access" title="Manage team access">
            <PublicationManageTeamAccessTab
              releases={viewModel.releases}
              onChange={handleChange}
            />
          </TabsSection>
          <TabsSection id="invite-users" title="Invite new users">
            <PublicationInviteNewUsersTab releases={viewModel.releases} />
          </TabsSection>
        </Tabs>
      </Page>
    </LoadingSpinner>
  );
};

export default PublicationManageTeamAccessPage;
