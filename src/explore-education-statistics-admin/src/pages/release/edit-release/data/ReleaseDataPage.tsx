import PublicationContext from "@admin/pages/release/PublicationContext";
import service from '@admin/services/common/service';
import {IdTitlePair} from '@admin/services/common/types';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import React, {useContext, useEffect, useState} from 'react';
import {RouteComponentProps} from 'react-router';
import ReleaseDataUploadsSection from './ReleaseDataUploadsSection';
import ReleaseFileUploadsSection from './ReleaseFileUploadsSection';

interface MatchProps {
  releaseId: string;
}

const ReleaseDataPage = ({ match }: RouteComponentProps<MatchProps>) => {
  const { releaseId } = match.params;

  const {publication} = useContext(PublicationContext);

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
