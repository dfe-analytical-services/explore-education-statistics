import StatusBlock, { StatusBlockColors } from '@admin/components/StatusBlock';
import { ReleaseStageStatuses } from '@admin/services/releaseService';
import Tag from '@common/components/Tag';
import React from 'react';

const approvedStatuses = ['Complete', 'Scheduled'];

interface Props {
  color?: StatusBlockColors;
  currentStatus?: ReleaseStageStatuses;
  isApproved?: boolean;
}

const ReleasePublishingStatusTag = ({
  color,
  currentStatus,
  isApproved = false,
}: Props) => {
  if (!currentStatus) {
    return null;
  }

  return (
    <>
      {isApproved && !approvedStatuses.includes(currentStatus.overallStage) && (
        <Tag>Approved</Tag>
      )}
      <StatusBlock
        color={color}
        text={
          currentStatus
            ? (currentStatus.overallStage === 'Complete' &&
                isApproved &&
                'Published') ||
              currentStatus.overallStage
            : 'Waiting to be scheduled...'
        }
        id={
          currentStatus
            ? `release-process-status-${currentStatus.overallStage}`
            : 'release-process-status-WaitingToBeScheduled'
        }
      />
    </>
  );
};

export default ReleasePublishingStatusTag;
