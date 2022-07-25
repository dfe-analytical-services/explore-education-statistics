import useReleasePublishingStatus from '@admin/pages/release/hooks/useReleasePublishingStatus';
import ReleasePublishingStatusTag from '@admin/pages/release/components/ReleasePublishingStatusTag';
import ReleasePublishingStages from '@admin/pages/release/components/ReleasePublishingStages';
import React from 'react';

interface ReleasePublishingStatusProps {
  isApproved?: boolean;
  releaseId: string;
}

const ReleasePublishingStatus = ({
  isApproved = false,
  releaseId,
}: ReleasePublishingStatusProps) => {
  const { currentStatus, currentStatusDetail } = useReleasePublishingStatus({
    releaseId,
  });
  return (
    <>
      <ReleasePublishingStatusTag
        currentStatus={currentStatus}
        color={currentStatusDetail.color}
        isApproved={isApproved}
      />
      <ReleasePublishingStages currentStatus={currentStatus} />
    </>
  );
};

export default ReleasePublishingStatus;
