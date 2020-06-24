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
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
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

  const getFootnoteData = useCallback(() => {
    Promise.all([
      footnotesService.getReleaseFootnoteData(releaseId),
      permissionService.canUpdateRelease(releaseId),
    ]).then(([{ meta, footnotes: footnotesList }, canUpdateRelease]) => {
      setFootnotesData({
        footnoteMeta: meta,
        footnotes: footnotesList,
        footnoteMetaGetters: generateFootnoteMetaMap(meta),
        canUpdateRelease,
      });
    });
  }, [releaseId]);

  useEffect(() => {
    getFootnoteData();
  }, [activeTab, getFootnoteData]);

  return (
    <>
      <Tabs
        onToggle={tab => {
          if (tab.id === 'footnotes') {
            getFootnoteData();
          }
          setActiveTab(tab.id);
        }}
        id="dataUploadTab"
      >
        <TabsSection id="data-upload" title="Data uploads">
          <ReleaseDataUploadsSection
            publicationId={publication.id}
            releaseId={releaseId}
          />
        </TabsSection>
        <TabsSection id="footnotes" title="Footnotes">
          <ReleaseFootnotesSection
            onDelete={getFootnoteData}
            publicationId={publication.id}
            releaseId={releaseId}
            footnotesData={footnotesData}
            onSubmit={(footnotesDataCallback: FootnotesData) => {
              setFootnotesData(footnotesDataCallback);
            }}
          />
        </TabsSection>
        <TabsSection id="file-upload" title="File uploads">
          <ReleaseFileUploadsSection
            publicationId={publication.id}
            releaseId={releaseId}
          />
        </TabsSection>
      </Tabs>
    </>
  );
};

export default ReleaseDataPage;
