import { useManageReleaseContext } from '@admin/pages/release/contexts/ManageReleaseContext';
import ReleaseDataUploadsSection from '@admin/pages/release/data/components/ReleaseDataUploadsSection';
import ReleaseFileUploadsSection from '@admin/pages/release/data/components/ReleaseFileUploadsSection';
import ReleaseFootnotesSection from '@admin/pages/release/data/components/ReleaseFootnotesSection';
import generateFootnoteMetaMap, {
  FootnoteMetaGetters,
} from '@admin/pages/release/data/utils/generateFootnoteMetaMap';
import footnotesService, {
  Footnote,
  FootnoteMeta,
} from '@admin/services/footnoteService';
import permissionService from '@admin/services/permissionService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React, { useCallback, useEffect, useState } from 'react';

export interface FootnotesData {
  footnoteMeta: FootnoteMeta;
  footnotes: Footnote[];
  footnoteMetaGetters: FootnoteMetaGetters;
  canUpdateRelease: boolean;
}

const ReleaseDataPage = () => {
  const { publication, releaseId } = useManageReleaseContext();

  const [footnotesData, setFootnotesData] = useState<FootnotesData>();
  const [activeTab, setActiveTab] = useState<string>('');

  const {
    value: canUpdateRelease = false,
    isLoading,
  } = useAsyncHandledRetry(
    () => permissionService.canUpdateRelease(releaseId),
    [releaseId],
  );

  const getFootnoteData = useCallback(() => {
    Promise.all([footnotesService.getReleaseFootnoteData(releaseId)]).then(
      ([{ meta, footnotes: footnotesList }]) => {
        setFootnotesData({
          footnoteMeta: meta,
          footnotes: footnotesList,
          footnoteMetaGetters: generateFootnoteMetaMap(meta),
          canUpdateRelease,
        });
      },
    );
  }, [canUpdateRelease, releaseId]);

  useEffect(() => {
    getFootnoteData();
  }, [activeTab, getFootnoteData]);

  return (
    <LoadingSpinner loading={isLoading}>
      <Tabs
        onToggle={tab => {
          if (tab.id === 'footnotes') {
            getFootnoteData();
          }
          setActiveTab(tab.id);
        }}
        id="dataUploadTab"
      >
        <TabsSection id="data-uploads" title="Data uploads">
          <ReleaseDataUploadsSection
            publicationId={publication.id}
            releaseId={releaseId}
            canUpdateRelease={canUpdateRelease}
          />
        </TabsSection>
        <TabsSection id="footnotes" title="Footnotes">
          <ReleaseFootnotesSection
            publicationId={publication.id}
            releaseId={releaseId}
            footnotesData={footnotesData}
            onDelete={getFootnoteData}
            onSubmit={(footnotesDataCallback: FootnotesData) => {
              setFootnotesData(footnotesDataCallback);
            }}
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
