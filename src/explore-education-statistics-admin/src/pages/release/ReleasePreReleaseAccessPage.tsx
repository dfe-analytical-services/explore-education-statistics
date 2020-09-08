import Link from '@admin/components/Link';
import PreReleaseUserAccessForm from '@admin/pages/release/components/PreReleaseUserAccessForm';
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

  const { value: release, isLoading } = useAsyncHandledRetry(
    () => releaseService.getRelease(releaseId),
    [releaseId],
  );

  return (
    <LoadingSpinner loading={isLoading}>
      {release && (
        <Tabs id="preReleaseAccess">
          <TabsSection id="preReleaseAccess-users" title="Pre-release users">
            <h2>Manage pre-release user access</h2>

            {release.status !== 'Approved' ? (
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
                  release status page
                </Link>
                .
              </WarningMessage>
            ) : (
              <PreReleaseUserAccessForm releaseId={release.id} />
            )}
          </TabsSection>
        </Tabs>
      )}
    </LoadingSpinner>
  );
};

export default ReleasePreReleaseAccessPage;
