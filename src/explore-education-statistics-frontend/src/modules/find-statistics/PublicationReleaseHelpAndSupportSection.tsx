import ContactUsSection from '@common/modules/find-statistics/components/ContactUsSection';
import NationalStatisticsSection from '@common/modules/find-statistics/components/NationalStatisticsSection';
import OfficialStatisticsSection from '@common/modules/find-statistics/components/OfficialStatisticsSection';
import AdHocOfficialStatisticsSection from '@common/modules/find-statistics/components/AdHocOfficialStatisticsSection';
import ExperimentalStatisticsSection from '@common/modules/find-statistics/components/ExperimentalStatisticsSection';
import ManagementInformationSection from '@common/modules/find-statistics/components/ManageInformationSection';
import { Contact } from '@common/services/publicationService';
import {
  ExternalMethodology,
  MethodologySummary,
} from '@common/services/types/methodology';
import { ReleaseType } from '@common/services/types/releaseType';
import Link from '@frontend/components/Link';
import React from 'react';

interface Props {
  publicationTitle: string;
  methodologies: MethodologySummary[];
  externalMethodology?: ExternalMethodology;
  releaseType?: ReleaseType;
  publicationContact: Contact;
}

const PublicationReleaseHelpAndSupportSection = ({
  publicationTitle,
  methodologies,
  externalMethodology,
  releaseType,
  publicationContact,
}: Props) => {
  return (
    <>
      <h2
        className="govuk-!-margin-top-9"
        data-testid="extra-information"
        id="help-and-support"
      >
        Help and support
      </h2>

      {(methodologies.length || externalMethodology) && (
        <>
          <h3>Methodology</h3>
          <p>
            Find out how and why we collect, process and publish these
            statistics.
          </p>

          <ul className="govuk-list govuk-list--spaced">
            {methodologies.map(methodology => (
              <li key={methodology.id}>
                <Link to={`/methodology/${methodology.slug}`}>
                  {methodology.title}
                </Link>
              </li>
            ))}
            {externalMethodology && (
              <li>
                <Link to={externalMethodology.url}>
                  {externalMethodology.title}
                </Link>
              </li>
            )}
          </ul>
        </>
      )}
      {releaseType === 'NationalStatistics' && <NationalStatisticsSection />}
      {releaseType === 'OfficialStatistics' && <OfficialStatisticsSection />}
      {releaseType === 'AdHocStatistics' && <AdHocOfficialStatisticsSection />}
      {releaseType === 'ExperimentalStatistics' && (
        <ExperimentalStatisticsSection />
      )}
      {releaseType === 'ManagementInformation' && (
        <ManagementInformationSection />
      )}

      <ContactUsSection
        publicationContact={publicationContact}
        publicationTitle={publicationTitle}
      />
    </>
  );
};

export default PublicationReleaseHelpAndSupportSection;
