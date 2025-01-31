import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import React from 'react';
import PrototypePublicationSubjects from './PrototypePublicationSubjects';

export const releaseDataPageTabIds = {
  dataUploads: 'data-uploads',
  fileUploads: 'file-uploads',
  dataGuidance: 'data-guidance',
  reordering: 'reordering',
  subjects: 'subjects',
};

const ReleaseDataPage = () => {
  return (
    <Tabs id="dataUploadTab">
      <TabsSection id={releaseDataPageTabIds.dataUploads} title="Data uploads">
        <p>Data uploads</p>
      </TabsSection>
      <TabsSection
        id={releaseDataPageTabIds.fileUploads}
        title="Ancillary file uploads"
      >
        <p>Ancillary file uploads</p>
      </TabsSection>
      <TabsSection
        id={releaseDataPageTabIds.dataGuidance}
        title="Data guidance"
        lazy
      >
        <p>Data guidance</p>
      </TabsSection>
      <TabsSection
        id={releaseDataPageTabIds.reordering}
        title="Reorder filters and indicators"
        lazy
      >
        <p>Reorder filters and indicators</p>
      </TabsSection>
      <TabsSection
        id={releaseDataPageTabIds.subjects}
        title="API data sets"
        lazy
      >
        <PrototypePublicationSubjects />
      </TabsSection>
    </Tabs>
  );
};

export default ReleaseDataPage;
