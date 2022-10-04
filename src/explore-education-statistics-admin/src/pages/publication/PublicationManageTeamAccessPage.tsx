import Page from '@admin/components/Page';
import PublicationManageTeamAccess from '@admin/pages/publication/components/PublicationManageTeamAccess';
import PublicationInviteNewUsersForm from '@admin/pages/publication/components/PublicationInviteNewUsersForm';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import { publicationManageTeamAccessReleaseRoute } from '@admin/routes/routes';
import publicationService, {
  Publication,
} from '@admin/services/publicationService';
import { ReleaseSummary } from '@admin/services/releaseService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import TabsSection from '@common/components/TabsSection';
import Tabs from '@common/components/Tabs';
import { FormSelect } from '@common/components/form';
import WarningMessage from '@common/components/WarningMessage';
import React, { useState } from 'react';
import { RouteComponentProps } from 'react-router';
import { generatePath, useHistory } from 'react-router-dom';

interface Model {
  releases: ReleaseSummary[];
  publication: Publication;
}

const PublicationManageTeamAccessPage = ({
  match,
}: RouteComponentProps<ReleaseRouteParams>) => {
  const history = useHistory();
  const { publicationId, releaseId } = match.params;
  const [currentReleaseId, setCurrentReleaseId] = useState(releaseId ?? '');

  const { value: model, isLoading } = useAsyncHandledRetry<Model>(async () => {
    const [allReleases, publication] = await Promise.all([
      publicationService.listReleases(publicationId),
      publicationService.getPublication(publicationId),
    ]);
    const releases = allReleases.results;
    if (!releaseId && releases.length) {
      setCurrentReleaseId(releases[0].id);
      history.replace(
        generatePath<ReleaseRouteParams>(
          publicationManageTeamAccessReleaseRoute.path,
          {
            publicationId,
            releaseId: releases[0].id,
          },
        ),
      );
    }
    return { releases, publication };
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
                history.replace(
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
                <PublicationManageTeamAccess
                  publicationId={publication.id}
                  release={release}
                />
              </TabsSection>
              <TabsSection id="invite-users" title="Invite new users">
                <PublicationInviteNewUsersForm
                  publication={publication}
                  releases={releases}
                  releaseId={releaseId}
                />
              </TabsSection>
            </Tabs>
          )}
        </>
      )}
    </Page>
  );
};

export default PublicationManageTeamAccessPage;
