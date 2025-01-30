import { useAuthContext } from '@admin/contexts/AuthContext';
import { useReleaseVersionContext } from '@admin/pages/release/contexts/ReleaseVersionContext';
import ReleaseApiDataSetsSection from '@admin/pages/release/data/components/ReleaseApiDataSetsSection';
import ReleaseDataGuidanceSection from '@admin/pages/release/data/components/ReleaseDataGuidanceSection';
import ReleaseDataReorderSection from '@admin/pages/release/data/components/ReleaseDataReorderSection';
import ReleaseDataUploadsSection from '@admin/pages/release/data/components/ReleaseDataUploadsSection';
import ReleaseFileUploadsSection from '@admin/pages/release/data/components/ReleaseFileUploadsSection';
import releaseDataPageTabIds from '@admin/pages/release/data/utils/releaseDataPageTabIds';
import permissionService from '@admin/services/permissionService';
import { DataFile } from '@admin/services/releaseDataFileService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React, { useState } from 'react';

const ReleaseDataPage = () => {
  const { releaseVersion, releaseVersionId } = useReleaseVersionContext();
  const { user } = useAuthContext();

  const [dataFiles, setDataFiles] = useState<DataFile[]>([]);

  const { value: canUpdateRelease = false, isLoading } = useAsyncHandledRetry(
    () => permissionService.canUpdateRelease(releaseVersionId),
    [releaseVersionId],
  );

  return (
    <LoadingSpinner loading={isLoading}>
      <Tabs id="data-and-files-tabs">
        <TabsSection
          id={releaseDataPageTabIds.dataUploads}
          title="Data uploads"
        >
          <ReleaseDataUploadsSection
            publicationId={releaseVersion.publicationId}
            releaseVersionId={releaseVersionId}
            canUpdateRelease={canUpdateRelease}
            onDataFilesChange={setDataFiles}
          />
        </TabsSection>
        <TabsSection
          id={releaseDataPageTabIds.fileUploads}
          title="Supporting file uploads"
        >
          <ReleaseFileUploadsSection
            publicationId={releaseVersion.publicationId}
            releaseVersionId={releaseVersionId}
            canUpdateRelease={canUpdateRelease}
          />
        </TabsSection>
        <TabsSection
          id={releaseDataPageTabIds.dataGuidance}
          title="Data guidance"
          lazy
        >
          <ReleaseDataGuidanceSection
            // Track data files so that we can re-render this
            // section automatically whenever there is a change
            key={dataFiles.filter(file => file.status === 'COMPLETE').length}
            releaseVersionId={releaseVersionId}
            canUpdateRelease={canUpdateRelease}
          />
        </TabsSection>
        <TabsSection
          id={releaseDataPageTabIds.reordering}
          title="Reorder filters and indicators"
          lazy
        >
          <ReleaseDataReorderSection
            key={dataFiles.filter(file => file.status === 'COMPLETE').length}
            releaseVersionId={releaseVersionId}
            canUpdateRelease={canUpdateRelease}
          />
        </TabsSection>
        {user?.permissions.isBauUser && (
          <TabsSection
            id={releaseDataPageTabIds.apiDataSets}
            title="API data sets"
            lazy
          >
            <ReleaseApiDataSetsSection />
          </TabsSection>
        )}
      </Tabs>
    </LoadingSpinner>
  );
};

export default ReleaseDataPage;
