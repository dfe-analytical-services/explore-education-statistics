import ContactUsSection from '@common/modules/find-statistics/components/ContactUsSectionRedesign';
import ReleasePageLayout from '@common/modules/release/components/ReleasePageLayout';
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
      <ReleasePageLayout>
        <h2>TODO EES-6443- render release content</h2>
        <ContactUsSection
          publicationContact={publicationSummary.contact}
          publicationTitle={publicationSummary.title}
        />
      </ReleasePageLayout>
    </ReleasePageShell>
  );
};

export default PublicationReleasePage;
