import Link from '@admin/components/Link';
import PreReleaseUserAccessForm from '@admin/pages/release/components/PreReleaseUserAccessForm';
import PublicPreReleaseAccessForm from '@admin/pages/release/components/PublicPreReleaseAccessForm';
import { useManageReleaseContext } from '@admin/pages/release/contexts/ManageReleaseContext';
import {
  ReleaseRouteParams,
  releaseStatusRoute,
} from '@admin/routes/releaseRoutes';
import releaseService from '@admin/services/releaseService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath } from 'react-router';

const ReleasePreReleaseAccessPage = () => {
  const { releaseId } = useManageReleaseContext();

  const {
    value: release,
    isLoading,
    setState: setRelease,
  } = useAsyncHandledRetry(() => releaseService.getRelease(releaseId), [
    releaseId,
  ]);

  return (
    <LoadingSpinner loading={isLoading}>
      {release && (
        <Tabs id="preReleaseAccess">
          <TabsSection id="preReleaseAccess-users" title="Pre-release users">
            <h2>Manage pre-release user access</h2>

            {release.status === 'Approved' ? (
              <PreReleaseUserAccessForm
                releaseId={release.id}
                isReleaseLive={release.live}
              />
            ) : (
              <WarningMessage>
                Before you can invite users for pre-release access, the release
                status must be approved. This can be done in the{' '}
                <Link
                  to={generatePath<ReleaseRouteParams>(
                    releaseStatusRoute.path,
                    {
                      releaseId,
                      publicationId: release.publicationId,
                    },
                  )}
                >
                  release status
                </Link>
                page.
              </WarningMessage>
            )}
          </TabsSection>
          <TabsSection
            id="preReleaseAccess-publicList"
            title="Public access list"
          >
            <h2>Public pre-release access list</h2>

            <PublicPreReleaseAccessForm
              publicationId={release.publicationId}
              publicationSlug={release.publicationSlug}
              releaseId={release.id}
              releaseSlug={release.slug}
              isReleaseLive={release.live}
              preReleaseAccessList={release.preReleaseAccessList}
              onSubmit={async ({ preReleaseAccessList }) => {
                const updatedRelease = await releaseService.updateRelease(
                  release.id,
                  {
                    ...release,
                    typeId: release.type.id,
                    preReleaseAccessList,
                  },
                );

                setRelease({
                  value: updatedRelease,
                });
              }}
            />
          </TabsSection>
        </Tabs>
      )}
    </LoadingSpinner>
  );
};

export default ReleasePreReleaseAccessPage;
