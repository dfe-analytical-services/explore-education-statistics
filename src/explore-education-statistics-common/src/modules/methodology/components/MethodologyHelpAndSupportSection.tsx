import React from 'react';
import ContactUsSection from '@common/modules/find-statistics/components/ContactUsSection';
import { PublicationSummary } from '@common/services/publicationService';
import { MethodologyPublication } from '@common/services/methodologyService';

interface Props {
  owningPublication: PublicationSummary | MethodologyPublication;
}

export default function MethodologyHelpAndSupportSection({
  owningPublication,
}: Props) {
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
}
