import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import ContactUsSection from '@common/modules/find-statistics/components/ContactUsSection';
import NationalStatisticsSection from '@common/modules/find-statistics/components/NationalStatisticsSection';
import OfficialStatisticsSection from '@common/modules/find-statistics/components/OfficialStatisticsSection';
import { Contact } from '@common/services/publicationService';
import {
  ExternalMethodology,
  MethodologySummary,
} from '@common/services/types/methodology';
import { ReleaseType } from '@common/services/types/releaseType';
import Link from '@frontend/components/Link';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import React, { ReactNode } from 'react';
import AdHocOfficialStatisticsSection from '@common/modules/find-statistics/components/AdHocOfficialStatisticsSection';
import ExperimentalStatisticsSection from '@common/modules/find-statistics/components/ExperimentalStatisticsSection';
import ManagementInformationSection from '@common/modules/find-statistics/components/ManageInformationSection';

interface Props {
  includeAnalytics?: boolean;
  publicationTitle: string;
  methodologies: MethodologySummary[];
  externalMethodology?: ExternalMethodology;
  releaseType?: ReleaseType;
  publicationContact: Contact;
}

const PublicationReleaseHelpAndSupportSection = ({
  includeAnalytics = false,
  publicationTitle,
  methodologies,
  externalMethodology,
  releaseType,
  publicationContact,
}: Props) => {
  return (
    <>
      <h2
        className="govuk-heading-m govuk-!-margin-top-9"
        data-testid="extra-information"
        id="help-and-support"
      >
        Help and support
      </h2>
      <AccordionComponent
        accordionId="help-and-support-accordion"
        includeAnalytics={includeAnalytics}
        publicationTitle={publicationTitle}
      >
        {(methodologies.length || externalMethodology) && (
          <AccordionSection
            heading="Methodology"
            caption="Find out how and why we collect, process and publish these statistics"
            headingTag="h3"
          >
            {(methodologies.length || externalMethodology) && (
              <ul className="govuk-list govuk-list--spaced">
                {methodologies.map(methodology => (
                  <li key={methodology.id}>
                    <Link to={`/methodology/${methodology.slug}`}>
                      {methodology.title}
                    </Link>
                  </li>
                ))}
                {externalMethodology && (
                  <li>
                    <Link to={externalMethodology.url}>
                      {externalMethodology.title}
                    </Link>
                  </li>
                )}
              </ul>
            )}
          </AccordionSection>
        )}
        {releaseType === 'NationalStatistics' && (
          <AccordionSection heading="National statistics" headingTag="h3">
            <NationalStatisticsSection />
          </AccordionSection>
        )}
        {releaseType === 'OfficialStatistics' && (
          <AccordionSection heading="Official statistics" headingTag="h3">
            <OfficialStatisticsSection />
          </AccordionSection>
        )}
        {releaseType === 'AdHocStatistics' && (
          <AccordionSection
            heading="Ad hoc official statistics"
            headingTag="h3"
          >
            <AdHocOfficialStatisticsSection />
          </AccordionSection>
        )}
        {releaseType === 'ExperimentalStatistics' && (
          <AccordionSection heading="Experimental statistics" headingTag="h3">
            <ExperimentalStatisticsSection />
          </AccordionSection>
        )}
        {releaseType === 'ManagementInformation' && (
          <AccordionSection heading="Management information" headingTag="h3">
            <ManagementInformationSection />
          </AccordionSection>
        )}
        <AccordionSection
          heading="Contact us"
          headingTag="h3"
          caption="Ask questions and provide feedback"
          id="contact-us"
        >
          <ContactUsSection
            publicationContact={publicationContact}
            publicationTitle={publicationTitle}
          />
        </AccordionSection>
      </AccordionComponent>
    </>
  );
};

interface AccordionComponentProps {
  accordionId: string;
  includeAnalytics: boolean;
  publicationTitle: string;
  children: ReactNode;
}

const AccordionComponent = ({
  accordionId,
  includeAnalytics,
  publicationTitle,
  children,
}: AccordionComponentProps) => {
  return includeAnalytics ? (
    <Accordion
      toggleAllHiddenText="help and support sections"
      id={accordionId}
      onSectionOpen={accordionSection => {
        logEvent({
          category: `${publicationTitle} help and support`,
          action: 'Accordion opened',
          label: accordionSection.title,
        });
      }}
    >
      {children}
    </Accordion>
  ) : (
    <Accordion id={accordionId} toggleAllHiddenText="help and support sections">
      {children}
    </Accordion>
  );
};

export default PublicationReleaseHelpAndSupportSection;
