import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import {
  PublicationContact,
  ReleaseType,
} from '@common/services/publicationService';
import AccordionWithAnalytics from '@frontend/components/AccordionWithAnalytics';
import Link from '@frontend/components/Link';
import React, { ReactNode } from 'react';
import ContactUsSection from '@common/modules/find-statistics/components/ContactUsSection';
import NationalStatisticsSection from '@common/modules/find-statistics/components/NationalStatisticsSection';

interface Props {
  includeAnalytics?: boolean;
  accordionId: string;
  publicationTitle: string;
  methodologyUrl?: string;
  methodologySummary?: string;
  releaseType?: string;
  themeTitle: string;
  publicationContact: PublicationContact;
}

const HelpAndSupport = ({
  accordionId,
  includeAnalytics = false,
  publicationTitle,
  methodologyUrl,
  methodologySummary,
  releaseType,
  themeTitle,
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
        <AccordionSection
          heading="Methodology"
          caption={
            methodologySummary ||
            'Find out how and why we collect, process and publish these statistics'
          }
          headingTag="h3"
        >
          <p className="govuk-!-margin-bottom-9">
            <Link to={methodologyUrl}>View methodology</Link> for{' '}
            {publicationTitle}.
          </p>
        </AccordionSection>
        {releaseType === ReleaseType.NationalStatistics && (
          <AccordionSection heading="National Statistics" headingTag="h3">
            <NationalStatisticsSection />
          </AccordionSection>
        )}
        <AccordionSection heading="Contact us" headingTag="h3">
          <ContactUsSection
            publicationContact={publicationContact}
            themeTitle={themeTitle}
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
    <AccordionWithAnalytics
      id={accordionId}
      publicationTitle={publicationTitle}
    >
      {children}
    </AccordionWithAnalytics>
  ) : (
    <Accordion id={accordionId}>{children}</Accordion>
  );
};

export default HelpAndSupport;
