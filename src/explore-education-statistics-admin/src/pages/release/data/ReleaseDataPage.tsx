import { useManageReleaseContext } from '@admin/pages/release/contexts/ManageReleaseContext';
import ReleaseDataUploadsSection from '@admin/pages/release/data/components/ReleaseDataUploadsSection';
import ReleaseFileUploadsSection from '@admin/pages/release/data/components/ReleaseFileUploadsSection';
import ReleaseMetaGuidanceSection from '@admin/pages/release/data/components/ReleaseMetaGuidanceSection';
import permissionService from '@admin/services/permissionService';
import { DataFile } from '@admin/services/releaseDataFileService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React, { useState } from 'react';

const ReleaseDataPage = () => {
  const { publication, releaseId } = useManageReleaseContext();
  const [dataFiles, setDataFiles] = useState<DataFile[]>([]);

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
            onDataFilesChange={setDataFiles}
          />
        </TabsSection>
        <TabsSection id="file-uploads" title="File uploads">
          <ReleaseFileUploadsSection
            releaseId={releaseId}
            canUpdateRelease={canUpdateRelease}
          />
        </TabsSection>
        <TabsSection id="metadata-guidance" title="Metadata guidance" lazy>
          <ReleaseMetaGuidanceSection
            // Track data files so that we can re-render this
            // section automatically whenever there is a change
            key={dataFiles.filter(file => file.status === 'COMPLETE').length}
            releaseId={releaseId}
            canUpdateRelease={canUpdateRelease}
          />
        </TabsSection>
      </Tabs>
    </LoadingSpinner>
  );
};

export default ReleaseDataPage;
