import { useManageReleaseContext } from '@admin/pages/release/contexts/ManageReleaseContext';
import ReleaseDataUploadsSection from '@admin/pages/release/data/components/ReleaseDataUploadsSection';
import ReleaseFileUploadsSection from '@admin/pages/release/data/components/ReleaseFileUploadsSection';
import permissionService from '@admin/services/permissionService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';

const ReleaseDataPage = () => {
  const { publication, releaseId } = useManageReleaseContext();

  const {
    value: canUpdateRelease = false,
    isLoading,
  } = useAsyncHandledRetry(
    () => permissionService.canUpdateRelease(releaseId),
    [releaseId],
  );

  return (
    <LoadingSpinner loading={isLoading}>
      <Tabs id="dataUploadTab">
        <TabsSection id="data-uploads" title="Data uploads">
          <ReleaseDataUploadsSection
            publicationId={publication.id}
            releaseId={releaseId}
            canUpdateRelease={canUpdateRelease}
          />
        </TabsSection>
        <TabsSection id="file-uploads" title="File uploads">
          <ReleaseFileUploadsSection
            releaseId={releaseId}
            canUpdateRelease={canUpdateRelease}
          />
        </TabsSection>
      </Tabs>
    </LoadingSpinner>
  );
};

export default ReleaseDataPage;
