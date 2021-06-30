import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import React, { ReactNode } from 'react';
import { BasicPublicationContact } from '@common/services/publicationService';

interface Props {
  contactDetails?: BasicPublicationContact;
  methodologyLink?: ReactNode;
  releaseLink?: ReactNode;
}

const TableToolInfo = ({
  contactDetails,
  methodologyLink,
  releaseLink,
}: Props) => {
  return (
    <Accordion id="TableToolInfo">
      <AccordionSection heading="Related information">
        <ul className="govuk-list">
          {releaseLink && <li>Publication: {releaseLink}</li>}
          {methodologyLink && <li>Methodology: {methodologyLink}</li>}
        </ul>
      </AccordionSection>
      {contactDetails && (
        <AccordionSection heading="Contact us">
          <p>
            If you have a question about the data or methods used to create this
            table contact the named statistician:
          </p>
          <h4 className="govuk-heading-s govuk-!-font-weight-bold">
            {contactDetails.teamName}
          </h4>
          <p>Named statistician: {contactDetails.contactName}</p>
          <p>
            Email:{' '}
            <a href={`mailto:${contactDetails.teamEmail}`}>
              {contactDetails.teamEmail}
            </a>
          </p>
          <p>Telephone: {contactDetails.contactTelNo}</p>
        </AccordionSection>
      )}
    </Accordion>
  );
};

export default TableToolInfo;
