import React, { FC } from 'react';
import ContactUsSection from '@common/modules/find-statistics/components/ContactUsSection';
import { PublicationSummary } from '@common/services/publicationService';
import { MethodologyPublication } from '@common/services/methodologyService';

interface Props {
  owningPublication: PublicationSummary | MethodologyPublication;
}

const MethodologyHelpAndSupportSection: FC<Props> = ({ owningPublication }) => {
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
        publicationContact={owningPublication.contact}
        publicationTitle={owningPublication.title}
      />
    </>
  );
};

export default MethodologyHelpAndSupportSection;
