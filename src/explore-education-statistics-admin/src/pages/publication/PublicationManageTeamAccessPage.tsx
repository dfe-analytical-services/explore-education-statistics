import Page from '@admin/components/Page';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React, { useState } from 'react';
import { RouteComponentProps } from 'react-router';
import TabsSection from '@common/components/TabsSection';
import Tabs from '@common/components/Tabs';
import PublicationManageTeamAccessTab from '@admin/pages/publication/components/PublicationManageTeamAccessTab';
import PublicationInviteNewUsersTab from '@admin/pages/publication/components/PublicationInviteNewUsersTab';
import { FormSelect } from '@common/components/form';
import publicationService, {
  BasicPublicationDetails,
} from '@admin/services/publicationService';
import { ReleaseSummary } from '@admin/services/releaseService';
import WarningMessage from '@common/components/WarningMessage';
import { publicationManageTeamAccessReleaseRoute } from '@admin/routes/routes';
import { generatePath, useHistory } from 'react-router-dom';

interface Model {
  releases: ReleaseSummary[];
  publication: BasicPublicationDetails;
}

const PublicationManageTeamAccessPage = ({
  match,
}: RouteComponentProps<ReleaseRouteParams>) => {
  const history = useHistory();
  const { publicationId, releaseId } = match.params;
  const [currentReleaseId, setCurrentReleaseId] = useState(releaseId ?? '');

  const { value: model, isLoading } = useAsyncHandledRetry(async () => {
    const [releases, publication] = await Promise.all([
      publicationService.getReleases(publicationId),
      publicationService.getPublication(publicationId),
    ]);
    if (!releaseId && releases.length) {
      setCurrentReleaseId(releases[0].id);
      history.push(
        generatePath<ReleaseRouteParams>(
          publicationManageTeamAccessReleaseRoute.path,
          {
            publicationId,
            releaseId: releases[0].id,
          },
        ),
      );
    }
    return { releases, publication } as Model;
  }, [publicationId]);

  if (!model || isLoading) {
    return <LoadingSpinner />;
  }
  const { releases, publication } = model;

  const release = releases.find(r => r.id === currentReleaseId);

  return (
    <Page
      title="Manage team access"
      caption={publication.title}
      breadcrumbs={[{ name: 'Manage team access' }]}
    >
      {!releases.length ? (
        <WarningMessage>
          Create a release for this publication to manage team access.
        </WarningMessage>
      ) : (
        <>
          <div className="dfe-align--right">
            <FormSelect
              id="currentRelease"
              name="release"
              label="Select release"
              options={releases.map(r => ({
                label: r.title,
                value: r.id,
              }))}
              order={[]}
              value={currentReleaseId}
              onChange={e => {
                setCurrentReleaseId(e.target.value);
                history.push(
                  generatePath<ReleaseRouteParams>(
                    publicationManageTeamAccessReleaseRoute.path,
                    {
                      publicationId,
                      releaseId: e.target.value,
                    },
                  ),
                );
              }}
            />
          </div>

          {release && (
            <Tabs id="manageTeamAccessTabs">
              <TabsSection id="manage-access" title="Manage team access">
                <PublicationManageTeamAccessTab
                  publication={publication}
                  release={release}
                />
              </TabsSection>
              <TabsSection id="invite-users" title="Invite new users">
                <PublicationInviteNewUsersTab />
              </TabsSection>
            </Tabs>
          )}
        </>
      )}
    </Page>
  );
};

export default PublicationManageTeamAccessPage;
