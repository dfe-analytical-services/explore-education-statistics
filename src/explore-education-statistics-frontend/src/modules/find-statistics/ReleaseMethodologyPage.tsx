import ContactUsSection from '@common/modules/find-statistics/components/ContactUsSectionRedesign';
import ReleasePageContentSection from '@common/modules/find-statistics/components/ReleasePageContentSection';
import {
  PublicationMethodologiesList,
  PublicationSummaryRedesign,
} from '@common/services/publicationService';
import getUrlAttributes from '@common/utils/url/getUrlAttributes';
import Link from '@frontend/components/Link';
import React from 'react';

interface Props {
  methodologiesSummary: PublicationMethodologiesList;
  publicationSummary: PublicationSummaryRedesign;
}

const ReleaseMethodologyPage = ({
  methodologiesSummary,
  publicationSummary,
}: Props) => {
  const { methodologies, externalMethodology } = methodologiesSummary;

  const hasMethodologies = methodologies.length > 0 || externalMethodology;

  const externalMethodologyAttributes = getUrlAttributes(
    externalMethodology?.url || '',
  );

  return (
    <>
      {hasMethodologies && (
        <ReleasePageContentSection
          heading="Methodology"
          id="methodology-section"
        >
          <p>
            Find out how and why we collect, process and publish these
            statistics.
          </p>
          <ul
            className="govuk-list govuk-list--spaced"
            data-testid="methodologies-list"
          >
            {methodologiesSummary.methodologies.map(methodology => (
              <li key={methodology.methodologyId}>
                <Link to={`/methodology/${methodology.slug}`}>
                  {methodology.title}
                </Link>
              </li>
            ))}
            {externalMethodology && (
              <li>
                <Link
                  to={externalMethodology.url}
                  rel={`noopener noreferrer nofollow ${
                    !externalMethodologyAttributes?.isTrusted ? 'external' : ''
                  }`}
                  target="_blank"
                >
                  {externalMethodology.title} (opens in new tab)
                </Link>
              </li>
            )}
          </ul>
        </ReleasePageContentSection>
      )}
      <ContactUsSection
        publicationContact={publicationSummary.contact}
        publicationTitle={publicationSummary.title}
      />
    </>
  );
};

export default ReleaseMethodologyPage;
