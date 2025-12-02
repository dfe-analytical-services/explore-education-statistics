import ContentHtml from '@common/components/ContentHtml';
import ContactUsSection from '@common/modules/find-statistics/components/ContactUsSectionRedesign';
import ReleasePageContentSection from '@common/modules/find-statistics/components/ReleasePageContentSection';
import ReleaseTypeSection from '@common/modules/release/components/ReleaseTypeSection';
import {
  PreReleaseAccessListSummary,
  PublicationSummaryRedesign,
  RelatedInformationItem,
  ReleaseVersionSummary,
} from '@common/services/publicationService';
import { releaseTypes } from '@common/services/types/releaseType';
import React from 'react';

interface Props {
  praSummary: PreReleaseAccessListSummary;
  publicationSummary: PublicationSummaryRedesign;
  relatedInformationItems: RelatedInformationItem[];
  releaseVersionSummary: ReleaseVersionSummary;
}

const ReleaseHelpPage = ({
  praSummary,
  publicationSummary,
  relatedInformationItems,
  releaseVersionSummary,
}: Props) => {
  const hasPraSummary = !!praSummary.preReleaseAccessList;
  const hasRelatedInformation = relatedInformationItems.length > 0;

  return (
    <>
      <ContactUsSection
        publicationContact={publicationSummary.contact}
        publicationTitle={publicationSummary.title}
        publishingOrganisations={releaseVersionSummary.publishingOrganisations}
        sectionTitle="Get help by contacting us"
        includeSectionBreak
      />

      <ReleasePageContentSection
        heading={releaseTypes[releaseVersionSummary.type]}
        id="release-type-section"
        includeSectionBreak={hasPraSummary || hasRelatedInformation}
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
          <ContentHtml html={praSummary.preReleaseAccessList} />
        </ReleasePageContentSection>
      )}
    </>
  );
};

export default ReleaseHelpPage;
