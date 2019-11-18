import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import {
  PublicationContact,
  ReleaseType,
} from '@common/services/publicationService';
import AccordionWithAnalytics from '@frontend/components/AccordionWithAnalytics';
import Link from '@frontend/components/Link';
import React, { ReactNode } from 'react';

interface Props {
  includeAnalytics?: boolean;
  accordionId: string;
  publicationTitle: string;
  methodologyUrl: string;
  releaseType?: string;
  themeTitle: string;
  publicationContact: PublicationContact;
}

const HelpAndSupport = ({
  accordionId,
  includeAnalytics = false,
  publicationTitle,
  methodologyUrl,
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
          heading={`${publicationTitle}: methodology`}
          caption="Find out how and why we collect, process and publish these statistics"
          headingTag="h3"
        >
          <p>
            Read our{' '}
            <Link to={methodologyUrl}>
              {`${publicationTitle}: methodology`}
            </Link>{' '}
            guidance.
          </p>
        </AccordionSection>
        {releaseType === ReleaseType.NationalStatistics && (
          <AccordionSection heading="National Statistics" headingTag="h3">
            <p className="govuk-body">
              The{' '}
              <a href="https://www.statisticsauthority.gov.uk/">
                United Kingdom Statistics Authority
              </a>{' '}
              designated these statistics as National Statistics in accordance
              with the{' '}
              <a href="https://www.legislation.gov.uk/ukpga/2007/18/contents">
                Statistics and Registration Service Act 2007
              </a>{' '}
              and signifying compliance with the Code of Practice for
              Statistics.
            </p>
            <p className="govuk-body">
              Designation signifying their compliance with the authority's{' '}
              <a href="https://www.statisticsauthority.gov.uk/code-of-practice/the-code/">
                Code of Practice for Statistics
              </a>{' '}
              which broadly means these statistics are:
            </p>
            <ul className="govuk-list govuk-list--bullet">
              <li>
                managed impartially and objectively in the public interest
              </li>
              <li>meet identified user needs</li>
              <li>produced according to sound methods</li>
              <li>well explained and readily accessible</li>
            </ul>
            <p className="govuk-body">
              Once designated as National Statistics it's a statutory
              requirement for statistics to follow and comply with the Code of
              Practice for Statistics to be observed.
            </p>
            <p className="govuk-body">
              Find out more about the standards we follow to produce these
              statistics through our{' '}
              <a href="https://www.gov.uk/government/publications/standards-for-official-statistics-published-by-the-department-for-education">
                Standards for official statistics published by DfE
              </a>{' '}
              guidance.
            </p>
          </AccordionSection>
        )}
        <AccordionSection heading="Contact us" headingTag="h3">
          <p>
            If you have a specific enquiry about {themeTitle} statistics and
            data:
          </p>
          <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
            {publicationContact.teamName}
          </h4>
          <p className="govuk-!-margin-top-0">
            Email <br />
            <a href={`mailto:${publicationContact.teamEmail}`}>
              {publicationContact.teamEmail}
            </a>
          </p>
          <p>
            Telephone: {publicationContact.contactName} <br />{' '}
            {publicationContact.contactTelNo}
          </p>
          <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
            Press office
          </h4>
          <p className="govuk-!-margin-top-0">If you have a media enquiry:</p>
          <p>
            Telephone <br />
            020 7925 6789
          </p>
          <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
            Public enquiries
          </h4>
          <p className="govuk-!-margin-top-0">
            If you have a general enquiry about the Department for Education
            (DfE) or education:
          </p>
          <p>
            Telephone <br />
            037 0000 2288
          </p>
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
