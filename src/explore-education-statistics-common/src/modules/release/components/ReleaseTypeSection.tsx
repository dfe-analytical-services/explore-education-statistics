import AdHocStatisticsSection from '@common/modules/release/components/AdHocStatisticsSection';
import ExperimentalStatisticsSection from '@common/modules/release/components/ExperimentalStatisticsSection';
import ManagementInformationSection from '@common/modules/release/components/ManageInformationSection';
import AccreditedOfficialStatisticsSection from '@common/modules/release/components/AccreditedOfficialStatisticsSection';
import OfficialStatisticsSection from '@common/modules/release/components/OfficialStatisticsSection';
import OfficialStatisticsInDevelopmentSection from '@common/modules/release/components/OfficialStatisticsInDevelopmentSection';
import { ReleaseType } from '@common/services/types/releaseType';
import React from 'react';
import { Organisation } from '@common/services/types/organisation';

const releaseTypeComponents = {
  AccreditedOfficialStatistics: AccreditedOfficialStatisticsSection,
  OfficialStatistics: OfficialStatisticsSection,
  ExperimentalStatistics: ExperimentalStatisticsSection,
  AdHocStatistics: AdHocStatisticsSection,
  ManagementInformation: ManagementInformationSection,
  OfficialStatisticsInDevelopment: OfficialStatisticsInDevelopmentSection,
};

export interface ReleaseTypeSectionProps {
  publishingOrganisations?: Organisation[];
  showHeading?: boolean;
}

interface Props extends ReleaseTypeSectionProps {
  type: ReleaseType;
}

export default function ReleaseTypeSection({
  publishingOrganisations,
  showHeading = true,
  type,
}: Props) {
  const ReleaseTypeComponent = releaseTypeComponents[type];
  if (ReleaseTypeComponent) {
    return (
      <ReleaseTypeComponent
        publishingOrganisations={publishingOrganisations}
        showHeading={showHeading}
      />
    );
  }
}
