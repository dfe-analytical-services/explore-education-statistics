import ReleaseTypeSection from '@common/modules/release/components/ReleaseTypeSection';
import ContactUsSection from '@common/modules/find-statistics/components/ContactUsSection';
import { Release } from '@common/services/publicationService';
import {
  ExternalMethodology,
  MethodologySummary,
} from '@common/services/types/methodology';
import React, { ReactNode } from 'react';

interface Props {
  release: Release;
  renderExternalMethodologyLink: (
    externalMethodology: ExternalMethodology,
  ) => ReactNode;
  renderMethodologyLink: (methodology: MethodologySummary) => ReactNode;
}

export default function ReleaseHelpAndSupportSection({
  release,
  renderMethodologyLink,
  renderExternalMethodologyLink,
}: Props) {
  const { externalMethodology, methodologies, contact, title } =
    release.publication;

  return (
    <>
      <h2
        className="govuk-!-margin-top-9"
        data-testid="extra-information"
        id="help-and-support"
      >
        Help and support
      </h2>

      {methodologies.length > 0 && (
        <>
          <h3>Methodology</h3>
          <p>
            Find out how and why we collect, process and publish these
            statistics.
          </p>

          <ul className="govuk-list govuk-list--spaced">
            {methodologies.map(methodology => (
              <li key={methodology.id}>{renderMethodologyLink(methodology)}</li>
            ))}
            {externalMethodology && (
              <li>{renderExternalMethodologyLink(externalMethodology)}</li>
            )}
          </ul>
        </>
      )}

      <ReleaseTypeSection type={release.type} />

      <ContactUsSection publicationContact={contact} publicationTitle={title} />
    </>
  );
}
