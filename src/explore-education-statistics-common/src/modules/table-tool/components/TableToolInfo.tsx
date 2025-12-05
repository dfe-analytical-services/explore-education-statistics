import React, { ReactNode } from 'react';
import { Contact } from '@common/services/publicationService';
import { ReleaseType, releaseTypes } from '@common/services/types/releaseType';
import ButtonText from '@common/components/ButtonText';
import InfoIcon from '@common/components/InfoIcon';
import ReleaseTypeSection from '@common/modules/release/components/ReleaseTypeSection';
import Modal from '@common/components/Modal';
import { Organisation } from '@common/services/types/organisation';

interface Props {
  contactDetails?: Contact;
  methodologyLinks?: ReactNode[];
  publishingOrganisations?: Organisation[];
  releaseLink?: ReactNode;
  releaseType?: ReleaseType;
}

const TableToolInfo = ({
  contactDetails,
  methodologyLinks,
  publishingOrganisations,
  releaseLink,
  releaseType,
}: Props) => {
  return (
    <>
      <h2 className="govuk-heading-m">Related information</h2>

      <ul className="govuk-list">
        {releaseType && (
          <li>
            Release type:{' '}
            <Modal
              showClose
              title={releaseTypes[releaseType]}
              triggerButton={
                <ButtonText>
                  {releaseTypes[releaseType]} <InfoIcon />
                </ButtonText>
              }
            >
              <ReleaseTypeSection
                publishingOrganisations={publishingOrganisations}
                showHeading={false}
                type={releaseType}
              />
            </Modal>
          </li>
        )}
        {releaseLink && <li>Publication: {releaseLink}</li>}
        {methodologyLinks?.map((methodologyLink, index) => (
          // eslint-disable-next-line react/no-array-index-key
          <li key={index}>Methodology: {methodologyLink}</li>
        ))}
      </ul>

      <p>
        Our statistical practice is regulated by the{' '}
        <a href="https://osr.statisticsauthority.gov.uk/what-we-do/">
          Office for Statistics Regulation
        </a>{' '}
        (OSR)
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
