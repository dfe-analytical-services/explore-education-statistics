import PageMetaTitle from '@admin/components/PageMetaTitle';
import { useAuthContext } from '@admin/contexts/AuthContext';
import { useReleaseVersionContext } from '@admin/pages/release/contexts/ReleaseVersionContext';
import ReleaseApiDataSetsSection from '@admin/pages/release/data/components/ReleaseApiDataSetsSection';
import ReleaseDataGuidanceSection from '@admin/pages/release/data/components/ReleaseDataGuidanceSection';
import ReleaseDataReorderSection from '@admin/pages/release/data/components/ReleaseDataReorderSection';
import ReleaseDataUploadsSection from '@admin/pages/release/data/components/ReleaseDataUploadsSection';
import ReleaseFileUploadsSection from '@admin/pages/release/data/components/ReleaseFileUploadsSection';
import releaseDataPageTabs from '@admin/pages/release/data/utils/releaseDataPageTabs';
import permissionService from '@admin/services/permissionService';
import { DataFile } from '@admin/services/releaseDataFileService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React, { useState } from 'react';
import { useLocation } from 'react-router';

const ReleaseDataPage = () => {
  const { releaseVersion, releaseVersionId } = useReleaseVersionContext();
  const { user } = useAuthContext();
  const { hash } = useLocation();

  const initialTabTitle = hash
    ? Object.values(releaseDataPageTabs).find(
        tab => tab.id === hash.replace('#', ''),
      )?.title
    : releaseDataPageTabs.dataUploads.title;

  const [dataFiles, setDataFiles] = useState<DataFile[]>([]);
  const [tabTitle, setTabTitle] = useState<string | undefined>(initialTabTitle);

  const { value: canUpdateRelease = false, isLoading } = useAsyncHandledRetry(
    () => permissionService.canUpdateRelease(releaseVersionId),
    [releaseVersionId],
  );

  return (
    <>
      <PageMetaTitle
        title={`${tabTitle} - ${releaseVersion.publicationTitle}`}
      />
      <LoadingSpinner loading={isLoading}>
        <Tabs
          id="data-and-files-tabs"
          onToggle={section => {
            setTabTitle(section.title);
          }}
        >
          <TabsSection
            id={releaseDataPageTabs.dataUploads.id}
            title={releaseDataPageTabs.dataUploads.title}
          >
            <ReleaseDataUploadsSection
              publicationId={releaseVersion.publicationId}
              releaseVersionId={releaseVersionId}
              canUpdateRelease={canUpdateRelease}
              onDataFilesChange={setDataFiles}
            />
          </TabsSection>
          <TabsSection
            id={releaseDataPageTabs.fileUploads.id}
            title={releaseDataPageTabs.fileUploads.title}
          >
            <ReleaseFileUploadsSection
              publicationId={releaseVersion.publicationId}
              releaseVersionId={releaseVersionId}
              canUpdateRelease={canUpdateRelease}
            />
          </TabsSection>
          <TabsSection
            id={releaseDataPageTabs.dataGuidance.id}
            title={releaseDataPageTabs.dataGuidance.title}
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
            id={releaseDataPageTabs.reordering.id}
            title={releaseDataPageTabs.reordering.title}
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
              id={releaseDataPageTabs.apiDataSets.id}
              title={releaseDataPageTabs.apiDataSets.title}
              lazy
            >
              <ReleaseApiDataSetsSection />
            </TabsSection>
          )}
        </Tabs>
      </LoadingSpinner>
    </>
  );
};

export default ReleaseDataPage;
