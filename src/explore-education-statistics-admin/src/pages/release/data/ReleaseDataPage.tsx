import { useReleaseContext } from '@admin/pages/release/contexts/ReleaseContext';
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

export const releaseDataPageTabIds = {
  dataUploads: 'data-uploads',
  fileUploads: 'file-uploads',
  metaGuidance: 'metadata-guidance',
};

const ReleaseDataPage = () => {
  const { release, releaseId } = useReleaseContext();
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
        <TabsSection
          id={releaseDataPageTabIds.dataUploads}
          title="Data uploads"
        >
          <ReleaseDataUploadsSection
            publicationId={release.publicationId}
            releaseId={releaseId}
            canUpdateRelease={canUpdateRelease}
            onDataFilesChange={setDataFiles}
          />
        </TabsSection>
        <TabsSection
          id={releaseDataPageTabIds.fileUploads}
          title="Ancillary file uploads"
        >
          <ReleaseFileUploadsSection
            publicationId={release.publicationId}
            releaseId={releaseId}
            canUpdateRelease={canUpdateRelease}
          />
        </TabsSection>
        <TabsSection
          id={releaseDataPageTabIds.metaGuidance}
          title="Data guidance"
          lazy
        >
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
