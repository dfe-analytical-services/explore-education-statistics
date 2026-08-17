import PageMetaTitle from '@admin/components/PageMetaTitle';
import { useAuthContext } from '@admin/contexts/AuthContext';
import { useReleaseVersionContext } from '@admin/pages/release/contexts/ReleaseVersionContext';
import ReleaseApiDataSetsSection from '@admin/pages/release/data/components/ReleaseApiDataSetsSection';
import ReleaseDataGuidanceSection from '@admin/pages/release/data/components/ReleaseDataGuidanceSection';
import ReleaseDataReorderSection from '@admin/pages/release/data/components/ReleaseDataReorderSection';
import ReleaseDataUploadsSection from '@admin/pages/release/data/components/ReleaseDataUploadsSection';
import ReleaseFileUploadsSection from '@admin/pages/release/data/components/ReleaseFileUploadsSection';
import releaseDataPageTabs from '@admin/pages/release/data/utils/releaseDataPageTabs';
import releaseDataFileQueries from '@admin/queries/releaseDataFileQueries';
import permissionService from '@admin/services/permissionService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import { useQuery } from '@tanstack/react-query';
import React, { useState } from 'react';
import { useLocation } from 'react-router';

const ReleaseDataPage = () => {
  const { releaseVersion, releaseVersionId } = useReleaseVersionContext();
  const { user } = useAuthContext();
  const { hash } = useLocation();

  const tabTitleFromHash = hash
    ? Object.values(releaseDataPageTabs).find(
        tab => tab.id === hash.replace('#', ''),
      )?.title
    : undefined;
  const defaultTabTitle = releaseDataPageTabs.dataUploads.title;

  const [tabTitle, setTabTitle] = useState<string>(
    tabTitleFromHash ?? defaultTabTitle,
  );

  const { value: canUpdateRelease = false, isLoading } = useAsyncHandledRetry(
    () => permissionService.canUpdateRelease(releaseVersionId),
    [releaseVersionId],
  );

  // Shares its query key with the data uploads section below, so this is served
  // from the cache rather than making a request of its own.
  const { data: dataFiles = [] } = useQuery(
    releaseDataFileQueries.list(releaseVersionId),
  );

  const completeDataFilesCount = dataFiles.filter(
    file => file.status === 'COMPLETE',
  ).length;

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
              key={completeDataFilesCount}
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
              key={completeDataFilesCount}
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
