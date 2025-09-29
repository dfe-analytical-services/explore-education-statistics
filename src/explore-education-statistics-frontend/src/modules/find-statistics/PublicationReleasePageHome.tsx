import SectionBreak from '@common/components/SectionBreak';
import ContactUsSection, {
  contactUsNavItem,
} from '@common/modules/find-statistics/components/ContactUsSectionRedesign';
import ReleasePageLayout from '@common/modules/release/components/ReleasePageLayout';
import {
  PublicationSummaryRedesign,
  ReleaseVersionSummary,
} from '@common/services/publicationService';
import ReleasePageShell from '@frontend/modules/find-statistics/components/ReleasePageShell';
import { NextPage } from 'next';
import React, { useState } from 'react';

const pageSectionIds = ['headlines-section', 'contact-us-section'] as const;
type PageSectionId = (typeof pageSectionIds)[number];

function isValidPageSectionId(value: string): value is PageSectionId {
  return pageSectionIds.includes(value as PageSectionId);
}

interface Props {
  publicationSummary: PublicationSummaryRedesign;
  releaseVersionSummary: ReleaseVersionSummary;
}

const PublicationReleasePage: NextPage<Props> = ({
  publicationSummary,
  releaseVersionSummary,
}) => {
  const [activeSection, setActiveSection] =
    useState<PageSectionId>('headlines-section');

  const navItems = [
    { id: 'headlines-section', text: 'Headlines facts and figures' },
    contactUsNavItem,
  ];

  const setActiveSectionIfValid = (sectionId: string) => {
    if (isValidPageSectionId(sectionId)) {
      setActiveSection(sectionId);
    }
  };

  return (
    <ReleasePageShell
      activePage="home"
      publicationSummary={publicationSummary}
      releaseVersionSummary={releaseVersionSummary}
    >
      <ReleasePageLayout
        activeSection={activeSection}
        navItems={navItems}
        onClickNavItem={setActiveSectionIfValid}
        onChangeSection={setActiveSectionIfValid}
      >
        <section id="headlines-section" data-page-section>
          <h2>TODO EES-6443- render release content</h2>
        </section>
        <SectionBreak size="xl" />
        <ContactUsSection
          publicationContact={publicationSummary.contact}
          publicationTitle={publicationSummary.title}
        />
      </ReleasePageLayout>
    </ReleasePageShell>
  );
};

export default PublicationReleasePage;
