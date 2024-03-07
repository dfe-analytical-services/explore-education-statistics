import React from 'react';
import ContactUsSection from '@common/modules/find-statistics/components/ContactUsSection';
import { PublicationSummary } from '@common/services/publicationService';

interface Props {
  owningPublication: PublicationSummary;
  trackScroll?: boolean;
}

export default function MethodologyHelpAndSupportSection({
  owningPublication,
  trackScroll = false,
}: Props) {
  return (
    <div data-scroll={trackScroll ? true : undefined} id="help-section">
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
    </div>
  );
}
