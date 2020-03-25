import React, { useContext } from 'react';
import { PublicationContact } from '@common/services/publicationService';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';

const ContactUsSection = ({
  publicationContact,
  themeTitle,
}: {
  publicationContact: PublicationContact;
  themeTitle: string;
}) => {
  const { isEditing } = useContext(EditingContext);
  return (
    <>
      <p>
        If you have a specific enquiry about {themeTitle} statistics and data:
      </p>
      <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
        {publicationContact.teamName}
      </h4>
      <p className="govuk-!-margin-top-0">
        Email <br />
        {isEditing ? (
          <a>{publicationContact.teamEmail}</a>
        ) : (
          <a href={`mailto:${publicationContact.teamEmail}`}>
            {publicationContact.teamEmail}
          </a>
        )}
      </p>
      <p>
        Telephone: {publicationContact.contactName} <br />{' '}
        {publicationContact.contactTelNo}
      </p>
      <h4 className="govuk-heading-s govuk-!-margin-bottom-0">Press office</h4>
      <p className="govuk-!-margin-top-0">If you have a media enquiry:</p>
      <p>
        Telephone <br />
        020 7925 6789
      </p>
      <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
        Public enquiries
      </h4>
      <p className="govuk-!-margin-top-0">
        If you have a general enquiry about the Department for Education (DfE)
        or education:
      </p>
      <p>
        Telephone <br />
        037 0000 2288
      </p>
    </>
  );
};

export default ContactUsSection;
