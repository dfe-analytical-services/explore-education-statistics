import ManageReleaseContext, {ManageRelease} from '@admin/pages/release/ManageReleaseContext';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import React, {useContext, useEffect} from 'react';
import { RouteComponentProps } from 'react-router';
import ReleaseDataUploadsSection from './ReleaseDataUploadsSection';
import ReleaseFileUploadsSection from './ReleaseFileUploadsSection';

const ReleaseDataPage = () => {

  const {publication, releaseId} = useContext(ManageReleaseContext) as ManageRelease;

  return (
    <>
      {publication && (
        <>
          <h3>Data uploads</h3>

          <Tabs id="dataUploadTab">
            <TabsSection id="data-upload" title="Data uploads">
              <ReleaseDataUploadsSection
                publicationId={publication.id}
                releaseId={releaseId}
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
      )}
    </>
  );
};

export default ReleaseDataPage;
