import { useTenant } from '@common/contexts/TenantContext';
import { Contact } from '@common/services/publicationService';
import { Organisation } from '@common/services/types/organisation';
import React from 'react';

const ContactUsSection = ({
  publicationContact,
  publicationTitle,
  publishingOrganisations,
}: {
  publicationContact: Contact;
  publicationTitle: string;
  publishingOrganisations: Organisation[];
}) => {
  const { organisation, pressOffice } = useTenant();
  const isSkillsEngland = !!publishingOrganisations.find(
    org => org.title === 'Skills England',
  );
  return (
    <>
      <h3 id="contact-us">Contact us</h3>

      <p>
        If you have a specific enquiry about {publicationTitle} statistics and
        data:
      </p>

      <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
        {publicationContact.teamName}
      </h4>

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
          <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
            Press office
          </h4>

          <p className="govuk-!-margin-top-0">If you have a media enquiry:</p>
          <p>
            Email:{' '}
            <a href="mailto:skills.england@education.gov.uk">
              skills.england@education.gov.uk
            </a>
          </p>

          <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
            Public enquiries
          </h4>
          <p className="govuk-!-margin-top-0">
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
              <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
                Press office
              </h4>

              <p className="govuk-!-margin-top-0">
                If you have a media enquiry:
              </p>
              <p>{pressOffice.telephone}</p>
            </>
          )}

          {organisation.publicEnquiries && (
            <>
              <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
                Public enquiries
              </h4>
              <p className="govuk-!-margin-top-0">
                {organisation.publicEnquiries.description}
              </p>
              <p>{organisation.publicEnquiries.telephoneCallToAction}</p>
              <p>{organisation.publicEnquiries.telephone}</p>
              <p>
                Opening times: <br />
                {organisation.publicEnquiries.openingHours}
              </p>
            </>
          )}
        </>
      )}
    </>
  );
};

export default ContactUsSection;
