import ContentHtml from '@common/components/ContentHtml';
import FeedbackSection from '@common/components/FeedbackSection';
import ContactUsSection from '@common/modules/release/components/ReleaseContactUsSection';
import ReleasePageContentSection from '@common/modules/release/components/ReleasePageContentSection';
import ReleaseTypeSection from '@common/modules/release/components/ReleaseTypeSection';
import {
  PublicationSummary,
  RelatedInformationItem,
  ReleaseVersionSummary,
} from '@common/services/publicationService';
import { releaseTypes } from '@common/services/types/releaseType';
import React from 'react';

interface Props {
  publicationSummary: PublicationSummary;
  relatedInformationItems: RelatedInformationItem[];
  releaseVersionSummary: ReleaseVersionSummary;
}

const ReleaseHelpPage = ({
  publicationSummary,
  relatedInformationItems,
  releaseVersionSummary,
}: Props) => {
  const hasPraSummary = !!releaseVersionSummary.preReleaseAccessList;
  const hasRelatedInformation = relatedInformationItems.length > 0;

  return (
    <>
      <ContactUsSection
        publicationContact={publicationSummary.contact}
        publicationTitle={publicationSummary.title}
        publishingOrganisations={releaseVersionSummary.publishingOrganisations}
        sectionTitle="Get help by contacting us"
      />

      <ReleasePageContentSection
        heading={releaseTypes[releaseVersionSummary.type]}
        id="release-type-section"
      >
        <ReleaseTypeSection
          publishingOrganisations={
            releaseVersionSummary.publishingOrganisations
          }
          type={releaseVersionSummary.type}
          showHeading={false}
        />
      </ReleasePageContentSection>

      {hasRelatedInformation && (
        <ReleasePageContentSection
          heading="Related information"
          id="related-information-section"
        >
          <ul
            className="govuk-list govuk-list--spaced"
            data-testid="related-information-list"
          >
            {relatedInformationItems &&
              relatedInformationItems.map(link => (
                <li key={link.id}>
                  <a href={link.url}>{link.title}</a>
                </li>
              ))}
          </ul>
        </ReleasePageContentSection>
      )}

      {hasPraSummary && (
        <ReleasePageContentSection
          heading="Pre-release access list"
          id="pre-release-access-list-section"
        >
          <ContentHtml html={releaseVersionSummary.preReleaseAccessList} />
        </ReleasePageContentSection>
      )}

      <ReleasePageContentSection
        heading="Provide us with feedback"
        id="feedback-section"
        includeSectionBreak={false}
      >
        <FeedbackSection />
      </ReleasePageContentSection>
    </>
  );
};

export default ReleaseHelpPage;
