import ReleasePageContentSection from '@common/modules/find-statistics/components/ReleasePageContentSection';
import { Contact } from '@common/services/publicationService';
import { Organisation } from '@common/services/types/organisation';
import React from 'react';

export const contactUsNavItem = {
  id: 'contact-us-section',
  text: 'Contact us',
};

const ContactUsSection = ({
  includeSectionBreak = false,
  publicationContact,
  publicationTitle,
  publishingOrganisations,
  sectionTitle = 'Contact us',
}: {
  includeSectionBreak?: boolean;
  publicationContact: Contact;
  publicationTitle: string;
  publishingOrganisations?: Organisation[];
  sectionTitle?: string;
}) => {
  const isSkillsEngland = !!publishingOrganisations?.find(
    org => org.title === 'Skills England',
  );
  return (
    <ReleasePageContentSection
      heading={sectionTitle}
      id="contact-us-section"
      includeSectionBreak={includeSectionBreak}
    >
      <p>
        If you have a specific enquiry about {publicationTitle} statistics and
        data:
      </p>

      <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
        {publicationContact.teamName}
      </h3>

      <address className="govuk-!-margin-top-0">
        Email:{' '}
        <a href={`mailto:${publicationContact.teamEmail}`}>
          {publicationContact.teamEmail}
        </a>
        <br />
        Contact name: {publicationContact.contactName}
        {publicationContact.contactTelNo && (
          <>
            <br />
            Telephone: {publicationContact.contactTelNo}
          </>
        )}
      </address>

      {isSkillsEngland ? (
        <>
          <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
            Press office
          </h3>

          <p className="govuk-!-margin-top-0 govuk-!-margin-bottom-0">
            If you have a media enquiry:
          </p>
          <p>
            Email:{' '}
            <a href="mailto:skills.england@education.gov.uk">
              skills.england@education.gov.uk
            </a>
          </p>

          <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
            Public enquiries
          </h3>
          <p className="govuk-!-margin-top-0 govuk-!-margin-bottom-0">
            If you have a general enquiry about Skills England:
          </p>
          <p>
            Email:{' '}
            <a href="mailto:skills.england@education.gov.uk">
              skills.england@education.gov.uk
            </a>
          </p>
        </>
      ) : (
        <>
          <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
            Press office
          </h3>
          <p className="govuk-!-margin-top-0 govuk-!-margin-bottom-0">
            If you have a media enquiry:
          </p>
          <p>Telephone: 020 7783 8300</p>
          <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
            Public enquiries
          </h3>
          <p className="govuk-!-margin-top-0 govuk-!-margin-bottom-0">
            If you have a general enquiry about the Department for Education
            (DfE) or education:
          </p>
          <p>Telephone: 037 0000 2288</p>
          <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
            Opening times
          </h3>
          <p className="govuk-!-margin-top-0">
            Monday to Friday from 9.30am to 5pm (excluding bank holidays)
          </p>
        </>
      )}
    </ReleasePageContentSection>
  );
};

export default ContactUsSection;
