import useReleasePublishingStatus from '@admin/pages/release/hooks/useReleasePublishingStatus';
import ReleasePublishingStatusTag from '@admin/pages/release/components/ReleasePublishingStatusTag';
import ReleasePublishingStages from '@admin/pages/release/components/ReleasePublishingStages';
import { ReleaseStageStatus } from '@admin/services/releaseService';
import React from 'react';

interface ReleasePublishingStatusProps {
  releaseVersionId: string;
  refreshPeriod?: number;
  onChange?: (status: ReleaseStageStatus) => void;
}

export default function ReleasePublishingStatus({
  releaseVersionId,
  refreshPeriod,
  onChange,
}: ReleasePublishingStatusProps) {
  const { currentStatus, currentStatusDetail } = useReleasePublishingStatus({
    releaseVersionId,
    refreshPeriod,
    onChange,
  });

  return (
    <>
      <ReleasePublishingStatusTag
        currentStatus={currentStatus}
        color={currentStatusDetail.color}
      />
      <ReleasePublishingStages currentStatus={currentStatus} />
    </>
  );
}
