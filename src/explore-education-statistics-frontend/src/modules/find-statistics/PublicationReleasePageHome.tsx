import SectionBreak from '@common/components/SectionBreak';
import ContactUsSection from '@common/modules/find-statistics/components/ContactUsSectionRedesign';
import {
  PublicationSummaryRedesign,
  ReleaseVersionHomeContent,
} from '@common/services/publicationService';
import React, { Fragment } from 'react';

interface Props {
  homeContent: ReleaseVersionHomeContent;
  publicationSummary: PublicationSummaryRedesign;
}

const PublicationReleasePage = ({ homeContent, publicationSummary }: Props) => {
  const {
    content,
    keyStatistics,
    keyStatisticsSecondarySection,
    summarySection,
  } = homeContent;

  return (
    <>
      <section id="headlines-section" data-page-section>
        <h2>Summary Section</h2>
        {summarySection.content.map(block => (
          <p key={block.id}>{block.type}</p>
        ))}
        <h2>Key Statistics</h2>
        {keyStatistics.map(ks => (
          <p key={ks.id}>{ks.type}</p>
        ))}
        <h2>Key Statistics Secondary</h2>
        {keyStatisticsSecondarySection.content.map(block => (
          <p key={block.id}>{block.type}</p>
        ))}
      </section>
      <SectionBreak size="xl" />
      {content.map(section => (
        <Fragment key={section.id}>
          <section id={section.id} data-page-section>
            <h2>{section.heading}</h2>
            {section.content.map(block => (
              <p key={block.id}>{block.type}</p>
            ))}
          </section>
          <SectionBreak size="xl" />
        </Fragment>
      ))}
      <ContactUsSection
        publicationContact={publicationSummary.contact}
        publicationTitle={publicationSummary.title}
      />
    </>
  );
};

export default PublicationReleasePage;
