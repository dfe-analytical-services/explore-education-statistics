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

interface Props {
  includeAnalytics?: boolean;
  accordionId: string;
  publicationTitle: string;
  methodologyUrl?: string;
  methodologySummary?: string;
  releaseType?: string;
  publicationContact: PublicationContact;
}

const PublicationReleaseHelpAndSupportSection = ({
  accordionId,
  includeAnalytics = false,
  publicationTitle,
  methodologyUrl = '',
  methodologySummary,
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
        {methodologyUrl !== '' && (
          <AccordionSection
            heading="Methodology"
            caption={
              methodologySummary ||
              'Find out how and why we collect, process and publish these statistics'
            }
            headingTag="h3"
          >
            <p className="govuk-!-margin-bottom-9">
              <Link to="/methodology/[methodology]" as={methodologyUrl}>
                View methodology
              </Link>{' '}
              for {publicationTitle}.
            </p>
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
        logEvent(
          'Accordion',
          `${accordionSection.title} accordion opened`,
          publicationTitle,
        );
      }}
    >
      {children}
    </Accordion>
  ) : (
    <Accordion id={accordionId}>{children}</Accordion>
  );
};

export default PublicationReleaseHelpAndSupportSection;
