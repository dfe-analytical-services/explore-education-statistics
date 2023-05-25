import Link from '@admin/components/Link';
import { useEditingContext } from '@admin/contexts/EditingContext';
import { EditableRelease } from '@admin/services/releaseContentService';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import ContactUsSection from '@common/modules/find-statistics/components/ContactUsSection';
import NationalStatisticsSection from '@common/modules/find-statistics/components/NationalStatisticsSection';
import OfficialStatisticsSection from '@common/modules/find-statistics/components/OfficialStatisticsSection';
import React from 'react';
import AdHocOfficialStatisticsSection from '@common/modules/find-statistics/components/AdHocOfficialStatisticsSection';
import ExperimentalStatisticsSection from '@common/modules/find-statistics/components/ExperimentalStatisticsSection';
import ManagementInformationSection from '@common/modules/find-statistics/components/ManageInformationSection';

interface MethodologyLink {
  key: string;
  title: string;
  url: string;
}

const ReleaseHelpAndSupportSection = ({
  release,
}: {
  release: EditableRelease;
}) => {
  const { editingMode } = useEditingContext();
  const { publication } = release;

  const allMethodologies: MethodologyLink[] = publication.methodologies.map(
    methodology => ({
      key: methodology.id,
      title: methodology.title,
      url: `/methodology/${methodology.id}/summary`,
    }),
  );

  if (publication.externalMethodology) {
    allMethodologies.push({
      key: publication.externalMethodology.url,
      title: publication.externalMethodology.title,
      url: publication.externalMethodology.url,
    });
  }

  return (
    <>
      <h2
        className="govuk-heading-m govuk-!-margin-top-9"
        data-testid="extra-information"
        id="help-and-support"
      >
        Help and support
      </h2>

      <Accordion id="help-and-support-accordion">
        <AccordionSection
          heading="Methodology"
          caption="Find out how and why we collect, process and publish these statistics"
          headingTag="h3"
        >
          {allMethodologies.length ? (
            <ul className="govuk-list govuk-list--spaced">
              {allMethodologies.map(methodology => (
                <li key={methodology.key}>
                  {editingMode === 'edit' ? (
                    <a>{`${methodology.title}`}</a>
                  ) : (
                    <Link to={methodology.url}>{methodology.title}</Link>
                  )}
                </li>
              ))}
            </ul>
          ) : (
            <p>No methodologies added.</p>
          )}
        </AccordionSection>
        {release.type === 'NationalStatistics' && (
          <AccordionSection heading="National statistics" headingTag="h3">
            <NationalStatisticsSection />
          </AccordionSection>
        )}
        {release.type === 'OfficialStatistics' && (
          <AccordionSection heading="Official statistics" headingTag="h3">
            <OfficialStatisticsSection />
          </AccordionSection>
        )}
        {release.type === 'AdHocStatistics' && (
          <AccordionSection
            heading="Ad hoc official statistics"
            headingTag="h3"
          >
            <AdHocOfficialStatisticsSection />
          </AccordionSection>
        )}
        {release.type === 'ExperimentalStatistics' && (
          <AccordionSection heading="Experimental statistics" headingTag="h3">
            <ExperimentalStatisticsSection />
          </AccordionSection>
        )}
        {release.type === 'ManagementInformation' && (
          <AccordionSection heading="Management information" headingTag="h3">
            <ManagementInformationSection />
          </AccordionSection>
        )}
        <AccordionSection
          heading="Contact us"
          headingTag="h3"
          id="contact-us"
          caption="Ask questions and provide feedback"
        >
          <ContactUsSection
            publicationContact={publication.contact}
            publicationTitle={publication.title}
          />
        </AccordionSection>
      </Accordion>
    </>
  );
};

export default ReleaseHelpAndSupportSection;
