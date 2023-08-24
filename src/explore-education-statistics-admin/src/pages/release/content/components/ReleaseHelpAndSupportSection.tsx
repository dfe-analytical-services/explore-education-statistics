import Link from '@admin/components/Link';
import { useEditingContext } from '@admin/contexts/EditingContext';
import { EditableRelease } from '@admin/services/releaseContentService';
import ContactUsSection from '@common/modules/find-statistics/components/ContactUsSection';
import NationalStatisticsSection from '@common/modules/find-statistics/components/NationalStatisticsSection';
import OfficialStatisticsSection from '@common/modules/find-statistics/components/OfficialStatisticsSection';
import React from 'react';
import AdHocOfficialStatisticsSection from '@common/modules/find-statistics/components/AdHocOfficialStatisticsSection';
import ExperimentalStatisticsSection from '@common/modules/find-statistics/components/ExperimentalStatisticsSection';
import ManagementInformationSection from '@common/modules/find-statistics/components/ManageInformationSection';

interface MethodologyLink {
  key: string;
  title: string;
  url: string;
}

const ReleaseHelpAndSupportSection = ({
  release,
}: {
  release: EditableRelease;
}) => {
  const { editingMode } = useEditingContext();
  const { publication } = release;

  const allMethodologies: MethodologyLink[] = publication.methodologies.map(
    methodology => ({
      key: methodology.id,
      title: methodology.title,
      url: `/methodology/${methodology.id}/summary`,
    }),
  );

  if (publication.externalMethodology) {
    allMethodologies.push({
      key: publication.externalMethodology.url,
      title: publication.externalMethodology.title,
      url: publication.externalMethodology.url,
    });
  }

  return (
    <>
      <h2
        className="govuk-!-margin-top-9"
        data-testid="extra-information"
        id="help-and-support"
      >
        Help and support
      </h2>

      <h3>Methodology</h3>

      {allMethodologies.length ? (
        <ul className="govuk-list govuk-list--spaced">
          {allMethodologies.map(methodology => (
            <li key={methodology.key}>
              {editingMode === 'edit' ? (
                <a>{`${methodology.title}`}</a>
              ) : (
                <Link to={methodology.url}>{methodology.title}</Link>
              )}
            </li>
          ))}
        </ul>
      ) : (
        <p>No methodologies added.</p>
      )}

      {release.type === 'NationalStatistics' && <NationalStatisticsSection />}
      {release.type === 'OfficialStatistics' && <OfficialStatisticsSection />}
      {release.type === 'AdHocStatistics' && <AdHocOfficialStatisticsSection />}
      {release.type === 'ExperimentalStatistics' && (
        <ExperimentalStatisticsSection />
      )}
      {release.type === 'ManagementInformation' && (
        <ManagementInformationSection />
      )}

      <ContactUsSection
        publicationContact={publication.contact}
        publicationTitle={publication.title}
      />
    </>
  );
};

export default ReleaseHelpAndSupportSection;
