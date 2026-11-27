import { useTenant } from '@common/contexts/TenantContext';
import ReleasePageContentSection from '@common/modules/release/components/ReleasePageContentSection';
import { Contact } from '@common/services/publicationService';
import { Organisation } from '@common/services/types/organisation';
import React from 'react';

export const contactUsNavItem = {
  id: 'contact-us-section',
  text: 'Contact us',
};

const ReleaseContactUsSection = ({
  includeSectionBreak = false,
  publicationContact,
  publicationTitle,
  publishingOrganisations,
  sectionTitle = 'Contact us',
}: {
  includeSectionBreak?: boolean;
  publicationContact: Contact;
  publicationTitle: string;
  publishingOrganisations: Organisation[];
  sectionTitle?: string;
}) => {
  const { organisation, pressOffice } = useTenant();
  const isSkillsEngland = !!publishingOrganisations.find(
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
          {pressOffice && (
            <>
              <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                Press office
              </h3>
              <p className="govuk-!-margin-top-0 govuk-!-margin-bottom-0">
                If you have a media enquiry:
              </p>
              <p>{pressOffice.telephone}</p>
            </>
          )}
          {organisation.publicEnquiries && (
            <>
              <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                Public enquiries
              </h3>
              <p className="govuk-!-margin-top-0 govuk-!-margin-bottom-0">
                {organisation.publicEnquiries.description}
              </p>
              <p>{organisation.publicEnquiries.telephone}</p>
              <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                Opening times
              </h3>
              <p className="govuk-!-margin-top-0">
                {organisation.publicEnquiries.openingHours}
              </p>
            </>
          )}
        </>
      )}
    </ReleasePageContentSection>
  );
};

export default ReleaseContactUsSection;
