import React, { FC } from 'react';
import ContactUsSection from '@common/modules/find-statistics/components/ContactUsSection';
import { Methodology } from '@common/services/methodologyService';
import { Contact } from '@common/services/publicationService';

interface Props {
  methodology: Methodology;
  contact: Contact;
}

const MethodologyHelpAndSupportSection: FC<Props> = ({
  methodology,
  contact,
}) => {
  return (
    <>
      <h2
        className="govuk-!-margin-top-9"
        data-testid="extra-information"
        id="help-and-support"
      >
        Help and support
      </h2>

      <ContactUsSection
        publicationContact={contact}
        publicationTitle={methodology.title}
      />
    </>
  );
};

export default MethodologyHelpAndSupportSection;
