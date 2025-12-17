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
  publishingOrganisations?: Organisation[];
}) => {
  const isSkillsEngland = !!publishingOrganisations?.find(
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
          <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
            Press office
          </h4>

          <p className="govuk-!-margin-top-0">If you have a media enquiry:</p>
          <p>Telephone: 020 7783 8300</p>

          <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
            Public enquiries
          </h4>
          <p className="govuk-!-margin-top-0">
            If you have a general enquiry about the Department for Education
            (DfE) or education:
          </p>
          <p>Telephone: 037 0000 2288</p>
          <p>
            Opening times: <br />
            Monday to Friday from 9.30am to 5pm (excluding bank holidays)
          </p>
        </>
      )}
    </>
  );
};

export default ContactUsSection;
