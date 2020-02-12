import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import React, { useCallback, useContext, useEffect, useState } from 'react';
import footnotesService from '@admin/services/release/edit-release/footnotes/service';
import permissionService from '@admin/services/permissions/service';
import { generateFootnoteMetaMap } from '@admin/services/release/edit-release/footnotes/util';
import {
  Footnote,
  FootnoteMeta,
  FootnoteMetaGetters,
} from '@admin/services/release/edit-release/footnotes/types';
import { ErrorControlProps } from '@admin/validation/withErrorControl';
import ReleaseFootnotesSection from '@admin/pages/release/edit-release/data/ReleaseFootnotesSection';
import ReleaseDataUploadsSection from './ReleaseDataUploadsSection';
import ReleaseFileUploadsSection from './ReleaseFileUploadsSection';

export interface FootnotesData {
  loading: boolean;
  footnoteMeta: FootnoteMeta;
  footnotes: Footnote[];
  footnoteMetaGetters: FootnoteMetaGetters;
  hasSufficientData: boolean;
  canUpdateRelease: boolean;
}

const ReleaseDataPage = ({ handleApiErrors }: ErrorControlProps) => {
  const { publication, releaseId } = useContext(
    ManageReleaseContext,
  ) as ManageRelease;

  const [footnotesData, setFootnotesData] = useState<FootnotesData>();
  const [activeTab, setActiveTab] = useState<string>('');

  const getFootnoteData = useCallback(() => {
    Promise.all([
      footnotesService.getReleaseFootnoteData(releaseId),
      permissionService.canUpdateRelease(releaseId),
    ])
      .then(([{ meta, footnotes: footnotesList }, canUpdateReleaseResult]) => {
        setFootnotesData({
          loading: false,
          footnoteMeta: meta,
          footnotes: footnotesList,
          footnoteMetaGetters: generateFootnoteMetaMap(meta),
          hasSufficientData: !!Object.keys(meta).length,
          canUpdateRelease: canUpdateReleaseResult,
        });
      })
      .catch(handleApiErrors);
  }, [handleApiErrors, releaseId]);

  useEffect(() => {
    if (activeTab === 'footnotes') {
      getFootnoteData();
    }
  }, [activeTab]);

  return (
    <>
      <Tabs
        onToggle={tab => {
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
            getFootnoteData={getFootnoteData}
            publicationId={publication.id}
            releaseId={releaseId}
            footnotesPropData={footnotesData}
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
