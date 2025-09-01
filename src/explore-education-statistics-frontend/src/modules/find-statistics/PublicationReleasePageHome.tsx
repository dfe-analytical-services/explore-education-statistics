import { ReleaseVersion } from '@common/services/publicationService';
import ReleasePageShell from '@frontend/modules/find-statistics/components/ReleasePageShell';
import ReleasePageTabNav from '@frontend/modules/find-statistics/components/ReleasePageTabNav';
import { NextPage } from 'next';
import React from 'react';

interface Props {
  releaseVersion: ReleaseVersion;
}

const PublicationReleasePage: NextPage<Props> = ({ releaseVersion }) => {
  return (
    <ReleasePageShell releaseVersion={releaseVersion}>
      <ReleasePageTabNav
        activePage="home"
        releaseUrlBase={`/find-statistics/${releaseVersion.publication.slug}/${releaseVersion.slug}/`}
      />
      <p>Home</p>
    </ReleasePageShell>
  );
};

export default PublicationReleasePage;
