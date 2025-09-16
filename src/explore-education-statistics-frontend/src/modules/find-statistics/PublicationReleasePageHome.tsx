import {
  PublicationSummaryRedesign,
  ReleaseVersionSummary,
} from '@common/services/publicationService';
import ReleasePageShell from '@frontend/modules/find-statistics/components/ReleasePageShell';
import ReleasePageTabNav from '@frontend/modules/find-statistics/components/ReleasePageTabNav';
import { NextPage } from 'next';
import React from 'react';

interface Props {
  publicationSummary: PublicationSummaryRedesign;
  releaseVersionSummary: ReleaseVersionSummary;
}

const PublicationReleasePage: NextPage<Props> = ({
  publicationSummary,
  releaseVersionSummary,
}) => {
  return (
    <ReleasePageShell
      publicationSummary={publicationSummary}
      releaseVersionSummary={releaseVersionSummary}
    >
      <ReleasePageTabNav
        activePage="home"
        releaseUrlBase={`/find-statistics/${publicationSummary.slug}/${releaseVersionSummary.slug}`}
      />
      <p>Home</p>
    </ReleasePageShell>
  );
};

export default PublicationReleasePage;
