import { Contact } from '@common/services/publicationService';
import DataSetFilePageSection from '@frontend/modules/data-catalogue/components/DataSetFilePageSection';
import { pageSections } from '@frontend/modules/data-catalogue/DataSetFilePage';
import React from 'react';

interface Props {
  contact: Contact;
  dataSetTitle: string;
}

export default function DataSetFileContact({ contact, dataSetTitle }: Props) {
  return (
    <DataSetFilePageSection
      heading={pageSections.dataSetContact}
      id="dataSetContact"
    >
      <p>
        If you have a specific enquiry about {dataSetTitle} statistics and data:
      </p>

      <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
        {contact.teamName}
      </h3>

      <address className="govuk-!-margin-top-0">
        Email: <a href={`mailto:${contact.teamEmail}`}>{contact.teamEmail}</a>
        <br />
        Contact name: {contact.contactName}
        {contact.contactTelNo && (
          <>
            <br />
            Telephone: {contact.contactTelNo}
          </>
        )}
      </address>
    </DataSetFilePageSection>
  );
}
