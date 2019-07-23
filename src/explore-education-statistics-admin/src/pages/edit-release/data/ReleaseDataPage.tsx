import ReleasePageTemplate from '@admin/pages/edit-release/components/ReleasePageTemplate';
import ReleaseDataUploadsSection from '@admin/pages/edit-release/data/ReleaseDataUploadsSection';
import ReleaseFileUploadsSection from '@admin/pages/edit-release/data/ReleaseFileUploadsSection';
import service from '@admin/services/common/service';
import { IdLabelPair } from '@admin/services/common/types';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';

interface MatchProps {
  releaseId: string;
}

const ReleaseDataPage = ({ match }: RouteComponentProps<MatchProps>) => {
  const { releaseId } = match.params;
  const [publicationDetails, setPublicationDetails] = useState<IdLabelPair>();

  useEffect(() => {
    service
      .getPublicationDetailsForRelease(releaseId)
      .then(setPublicationDetails);
  }, [releaseId]);

  return (
    <>
      {publicationDetails && (
        <ReleasePageTemplate
          publicationTitle={publicationDetails.label}
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
