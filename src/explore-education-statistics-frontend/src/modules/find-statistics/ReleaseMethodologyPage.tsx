import SectionBreak from '@common/components/SectionBreak';
import ContactUsSection, {
  contactUsNavItem,
} from '@common/modules/find-statistics/components/ContactUsSectionRedesign';
import ReleasePageLayout from '@common/modules/release/components/ReleasePageLayout';
import {
  PublicationMethodologiesList,
  PublicationSummaryRedesign,
} from '@common/services/publicationService';
import getUrlAttributes from '@common/utils/url/getUrlAttributes';
import Link from '@frontend/components/Link';
import React, { useState } from 'react';

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

  const navItems = [
    hasMethodologies && { id: 'methodology-section', text: 'Methodology' },
    contactUsNavItem,
  ].filter(item => !!item);

  const [activeSection, setActiveSection] = useState(navItems[0].id);

  const externalMethodologyAttributes = getUrlAttributes(
    externalMethodology?.url || '',
  );

  const setActiveSectionIfValid = (sectionId: string) => {
    if (navItems.some(item => item.id === sectionId)) {
      setActiveSection(sectionId);
    }
  };

  return (
    <ReleasePageLayout
      activeSection={activeSection}
      navItems={navItems}
      onClickNavItem={setActiveSectionIfValid}
      onChangeSection={setActiveSectionIfValid}
    >
      {hasMethodologies && (
        <>
          <section id="methodology-section" data-page-section>
            <h2>Methodology</h2>
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
                      !externalMethodologyAttributes?.isTrusted
                        ? 'external'
                        : ''
                    }`}
                    target="_blank"
                  >
                    {externalMethodology.title} (opens in new tab)
                  </Link>
                </li>
              )}
            </ul>
          </section>
          <SectionBreak size="xl" />
        </>
      )}
      <ContactUsSection
        publicationContact={publicationSummary.contact}
        publicationTitle={publicationSummary.title}
      />
    </ReleasePageLayout>
  );
};

export default ReleaseMethodologyPage;
