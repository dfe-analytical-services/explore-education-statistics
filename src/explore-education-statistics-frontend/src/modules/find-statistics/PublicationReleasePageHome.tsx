import SectionBreak from '@common/components/SectionBreak';
import ContactUsSection from '@common/modules/find-statistics/components/ContactUsSectionRedesign';
import { PublicationSummaryRedesign } from '@common/services/publicationService';
import React from 'react';

interface Props {
  publicationSummary: PublicationSummaryRedesign;
}

const PublicationReleasePage = ({ publicationSummary }: Props) => {
  return (
    <>
      <section id="headlines-section" data-page-section>
        <h2>TODO EES-6443- render release content</h2>
      </section>
      <SectionBreak size="xl" />
      <ContactUsSection
        publicationContact={publicationSummary.contact}
        publicationTitle={publicationSummary.title}
      />
    </>
  );
};

export default PublicationReleasePage;
