import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import {
  PublicationContact,
  ReleaseType,
} from '@common/services/publicationService';
import Link from '@frontend/components/Link';
import React, { ReactNode } from 'react';
import ContactUsSection from '@common/modules/find-statistics/components/ContactUsSection';
import NationalStatisticsSection from '@common/modules/find-statistics/components/NationalStatisticsSection';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import {
  ExternalMethodology,
  MethodologySummary,
} from '@common/services/types/methodology';

interface Props {
  includeAnalytics?: boolean;
  accordionId: string;
  publicationTitle: string;
  methodologies: MethodologySummary[];
  externalMethodology?: ExternalMethodology;
  releaseType?: string;
  publicationContact: PublicationContact;
}

const PublicationReleaseHelpAndSupportSection = ({
  accordionId,
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
      >
        Help and support
      </h2>
      <AccordionComponent
        accordionId={accordionId}
        includeAnalytics={includeAnalytics}
        publicationTitle={publicationTitle}
      >
        {(methodologies.length || externalMethodology) && (
          <AccordionSection
            heading="Methodology"
            caption="Find out how and why we collect, process and publish these statistics"
            headingTag="h3"
          >
            {methodologies.map(methodology => (
              <p key={methodology.id} className="govuk-!-margin-bottom-9">
                <Link
                  to="/methodology/[methodology]"
                  as={`/methodology/${methodology.slug}`}
                >
                  {methodology.title}
                </Link>
              </p>
            ))}
            {externalMethodology && (
              <p className="govuk-!-margin-bottom-9">
                <Link
                  to="/methodology/[methodology]"
                  as={externalMethodology.url}
                >
                  {externalMethodology.title}
                </Link>
              </p>
            )}
          </AccordionSection>
        )}
        {releaseType === ReleaseType.NationalStatistics && (
          <AccordionSection heading="National Statistics" headingTag="h3">
            <NationalStatisticsSection />
          </AccordionSection>
        )}
        <AccordionSection heading="Contact us" headingTag="h3">
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
    <Accordion id={accordionId}>{children}</Accordion>
  );
};

export default PublicationReleaseHelpAndSupportSection;
