import ContactUsSection from '@common/modules/find-statistics/components/ContactUsSectionRedesign';
import ReleasePageLayout from '@common/modules/release/components/ReleasePageLayout';
import {
  PublicationSummaryRedesign,
  ReleaseVersionSummary,
} from '@common/services/publicationService';
import ReleasePageShell from '@frontend/modules/find-statistics/components/ReleasePageShell';
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
      activePage="home"
      publicationSummary={publicationSummary}
      releaseVersionSummary={releaseVersionSummary}
    >
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
