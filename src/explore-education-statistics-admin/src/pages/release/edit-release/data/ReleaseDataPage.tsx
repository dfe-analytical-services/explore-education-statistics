import service from '@admin/services/common/service';
import { IdTitlePair } from '@admin/services/common/types';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import ReleasePageTemplate from '../components/ReleasePageTemplate';
import ReleaseDataUploadsSection from './ReleaseDataUploadsSection';
import ReleaseFileUploadsSection from './ReleaseFileUploadsSection';

interface MatchProps {
  releaseId: string;
}

const ReleaseDataPage = ({ match }: RouteComponentProps<MatchProps>) => {
  const { releaseId } = match.params;
  const [publicationDetails, setPublicationDetails] = useState<IdTitlePair>();

  useEffect(() => {
    service
      .getPublicationDetailsForRelease(releaseId)
      .then(setPublicationDetails);
  }, [releaseId]);

  return (
    <>
      {publicationDetails && (
        <ReleasePageTemplate
          publicationTitle={publicationDetails.title}
          releaseId={releaseId}
        >
          <h3>Data uploads</h3>

          <Tabs id="dataUploadTab">
            <TabsSection id="data-upload" title="Data uploads">
              <ReleaseDataUploadsSection releaseId={releaseId} />
            </TabsSection>
            <TabsSection id="file-upload" title="File uploads">
              <ReleaseFileUploadsSection releaseId={releaseId} />
            </TabsSection>
          </Tabs>
        </ReleasePageTemplate>
      )}
    </>
  );
};

export default ReleaseDataPage;
