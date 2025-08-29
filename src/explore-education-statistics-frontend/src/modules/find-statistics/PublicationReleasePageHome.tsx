import { ReleaseVersion } from '@common/services/publicationService';
import ReleasePageShell from '@frontend/modules/find-statistics/components/ReleasePageShell';
import { NextPage } from 'next';
import React from 'react';

interface Props {
  releaseVersion: ReleaseVersion;
}

const PublicationReleasePage: NextPage<Props> = ({ releaseVersion }) => {
  return (
    <ReleasePageShell releaseVersion={releaseVersion}>
      <p>Home</p>
    </ReleasePageShell>
  );
};

export default PublicationReleasePage;
