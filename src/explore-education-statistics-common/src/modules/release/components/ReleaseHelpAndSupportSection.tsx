import ReleaseTypeSection from '@common/modules/release/components/ReleaseTypeSection';
import ContactUsSection from '@common/modules/find-statistics/components/ContactUsSection';
import { Publication } from '@common/services/publicationService';
import {
  ExternalMethodology,
  MethodologySummary,
} from '@common/services/types/methodology';
import { ReleaseType } from '@common/services/types/releaseType';
import React, { ReactNode } from 'react';
import { Organisation } from '@common/services/types/organisation';

interface Props {
  publication: Publication;
  publishingOrganisations?: Organisation[];
  releaseType: ReleaseType;
  renderExternalMethodologyLink: (
    externalMethodology: ExternalMethodology,
  ) => ReactNode;
  renderMethodologyLink: (methodology: MethodologySummary) => ReactNode;
  trackScroll?: boolean;
}

export default function ReleaseHelpAndSupportSection({
  publication,
  publishingOrganisations,
  releaseType,
  renderMethodologyLink,
  renderExternalMethodologyLink,
  trackScroll = false,
}: Props) {
  const { externalMethodology, methodologies, contact, title } = publication;

  return (
    <div data-scroll={trackScroll ? true : undefined} id="help-section">
      <h2
        className="govuk-!-margin-top-9"
        data-testid="extra-information"
        id="help-and-support"
      >
        Help and support
      </h2>

      {(methodologies.length > 0 || externalMethodology) && (
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

      <ReleaseTypeSection
        publishingOrganisations={publishingOrganisations}
        type={releaseType}
      />

      <ContactUsSection
        publicationContact={contact}
        publicationTitle={title}
        publishingOrganisations={publishingOrganisations}
      />
    </div>
  );
}
