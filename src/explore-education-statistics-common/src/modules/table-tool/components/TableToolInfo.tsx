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
            table, please contact us:
          </p>
          <h3 className="govuk-heading-s">{contactDetails.teamName}</h3>
          <address className="govuk-!-margin-top-0">
            Email:{' '}
            <a href={`mailto:${contactDetails.teamEmail}`}>
              {contactDetails.teamEmail}
            </a>
            <br />
            Contact name: {contactDetails.contactName}
            {contactDetails.contactTelNo && (
              <>
                <br />
                Telephone: {contactDetails.contactTelNo}
              </>
            )}
          </address>
        </>
      )}
    </>
  );
};

export default TableToolInfo;
