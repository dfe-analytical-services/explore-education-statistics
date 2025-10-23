import ContentHtml from '@common/components/ContentHtml';
import ContactUsSection from '@common/modules/find-statistics/components/ContactUsSectionRedesign';
import ReleasePageContentSection from '@common/modules/find-statistics/components/ReleasePageContentSection';
import {
  PreReleaseAccessListSummary,
  PublicationSummaryRedesign,
  RelatedInformationItem,
} from '@common/services/publicationService';
import glossaryService from '@frontend/services/glossaryService';
import React from 'react';

interface Props {
  praSummary: PreReleaseAccessListSummary;
  publicationSummary: PublicationSummaryRedesign;
  relatedInformationItems: RelatedInformationItem[];
}

const ReleaseHelpPage = ({
  praSummary,
  publicationSummary,
  relatedInformationItems,
}: Props) => {
  const hasPraSummary = !!praSummary.preReleaseAccessList;
  const hasRelatedInformation = relatedInformationItems.length > 0;

  return (
    <>
      <ContactUsSection
        publicationContact={publicationSummary.contact}
        publicationTitle={publicationSummary.title}
        sectionTitle="Get help by contacting us"
        includeSectionBreak={hasPraSummary || hasRelatedInformation}
      />

      {hasRelatedInformation && (
        <ReleasePageContentSection
          heading="Related information"
          id="related-information-section"
          includeSectionBreak={hasPraSummary}
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
          includeSectionBreak={false}
        >
          <ContentHtml
            html={praSummary.preReleaseAccessList}
            getGlossaryEntry={glossaryService.getEntry}
          />
        </ReleasePageContentSection>
      )}
    </>
  );
};

export default ReleaseHelpPage;
