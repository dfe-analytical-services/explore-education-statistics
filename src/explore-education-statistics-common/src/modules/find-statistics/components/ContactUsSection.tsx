import { Contact } from '@common/services/publicationService';
import React from 'react';

const ContactUsSection = ({
  publicationContact,
  publicationTitle,
}: {
  publicationContact: Contact;
  publicationTitle: string;
}) => {
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
            Telephone:{' '}
            <a href={`tel:${publicationContact.contactTelNo}`}>
              {publicationContact.contactTelNo}
            </a>
          </>
        )}
      </address>
      <h4 className="govuk-heading-s govuk-!-margin-bottom-0">Press office</h4>
      <p className="govuk-!-margin-top-0">If you have a media enquiry:</p>
      <p>
        Telephone: <a href="tel:020 7783 8300">020 7783 8300</a>
      </p>
      <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
        Public enquiries
      </h4>
      <p className="govuk-!-margin-top-0">
        If you have a general enquiry about the Department for Education (DfE)
        or education:
      </p>
      <p>
        Telephone: <a href="tel:037 0000 2288">037 0000 2288</a>
      </p>
      <p>
        Opening times: <br />
        Monday to Friday from 9.30am to 5pm (excluding bank holidays)
      </p>
    </>
  );
};

export default ContactUsSection;
