import React, { ReactNode } from 'react';
import { Contact } from '@common/services/publicationService';

interface Props {
  contactDetails?: Contact;
  methodologyLinks?: ReactNode[];
  releaseLink?: ReactNode;
}

const TableToolInfo = ({
  contactDetails,
  methodologyLinks,
  releaseLink,
}: Props) => {
  return (
    <>
      <h3>Related information</h3>
      <ul className="govuk-list">
        {releaseLink && <li>Publication: {releaseLink}</li>}
        {methodologyLinks?.map((methodologyLink, index) => (
          // eslint-disable-next-line react/no-array-index-key
          <li key={index}>Methodology: {methodologyLink}</li>
        ))}
      </ul>
      {contactDetails && (
        <>
          <h3>Contact us</h3>
          <p>
            If you have a question about the data or methods used to create this
            table contact the named statistician:
          </p>
          <h3 className="govuk-heading-s">{contactDetails.teamName}</h3>
          <p>Named statistician: {contactDetails.contactName}</p>
          <p>
            Email:{' '}
            <a href={`mailto:${contactDetails.teamEmail}`}>
              {contactDetails.teamEmail}
            </a>
          </p>
          <p>Telephone: {contactDetails.contactTelNo}</p>
        </>
      )}
    </>
  );
};

export default TableToolInfo;
