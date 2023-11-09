import React, { ReactNode } from 'react';
import { Contact } from '@common/services/publicationService';
import ReleaseTypesModal from '@common/modules/release/components/ReleaseTypesModal';
import { ReleaseType, releaseTypes } from '@common/services/types/releaseType';
import ButtonText from '@common/components/ButtonText';
import InfoIcon from '@common/components/InfoIcon';

interface Props {
  contactDetails?: Contact;
  methodologyLinks?: ReactNode[];
  releaseLink?: ReactNode;
  releaseType?: ReleaseType;
}

const TableToolInfo = ({
  contactDetails,
  methodologyLinks,
  releaseLink,
  releaseType,
}: Props) => {
  return (
    <>
      <h3>Related information</h3>

      <ul className="govuk-list">
        {releaseType && (
          <li>
            Release type: {releaseTypes[releaseType]}{' '}
            <ReleaseTypesModal
              triggerButton={
                <ButtonText>
                  <InfoIcon description="What are release types" />
                </ButtonText>
              }
            />
          </li>
        )}
        {releaseLink && <li>Publication: {releaseLink}</li>}
        {methodologyLinks?.map((methodologyLink, index) => (
          // eslint-disable-next-line react/no-array-index-key
          <li key={index}>Methodology: {methodologyLink}</li>
        ))}
      </ul>

      <p>
        Our statistical practice is regulated by the Office for Statistics
        Regulation (OSR).
      </p>

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
