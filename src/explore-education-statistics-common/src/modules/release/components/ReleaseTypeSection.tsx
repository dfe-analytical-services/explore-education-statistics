import AdHocStatisticsSection from '@common/modules/release/components/AdHocStatisticsSection';
import ExperimentalStatisticsSection from '@common/modules/release/components/ExperimentalStatisticsSection';
import ManagementInformationSection from '@common/modules/release/components/ManageInformationSection';
import NationalStatisticsSection from '@common/modules/release/components/NationalStatisticsSection';
import OfficialStatisticsSection from '@common/modules/release/components/OfficialStatisticsSection';
import { ReleaseType } from '@common/services/types/releaseType';
import React from 'react';

const releaseTypeComponents = {
  NationalStatistics: NationalStatisticsSection,
  OfficialStatistics: OfficialStatisticsSection,
  ExperimentalStatistics: ExperimentalStatisticsSection,
  AdHocStatistics: AdHocStatisticsSection,
  ManagementInformation: ManagementInformationSection,
};

interface Props {
  showHeading?: boolean;
  type: ReleaseType;
}

export default function ReleaseTypeSection({
  showHeading = true,
  type,
}: Props) {
  const ReleaseTypeComponent = releaseTypeComponents[type];

  return <ReleaseTypeComponent showHeading={showHeading} />;
}
