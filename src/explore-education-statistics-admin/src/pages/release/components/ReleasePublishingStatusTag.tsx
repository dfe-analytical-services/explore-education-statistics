import StatusBlock, { StatusBlockColors } from '@admin/components/StatusBlock';
import { ReleaseStageStatus } from '@admin/services/releaseService';
import React from 'react';
import Tag from '@common/components/Tag';

const approvedStatuses = ['Complete', 'Scheduled'];

interface Props {
  color?: StatusBlockColors;
  currentStatus?: ReleaseStageStatus;
  isApproved?: boolean;
}

export default function ReleasePublishingStatusTag({
  color,
  currentStatus,
  isApproved = false,
}: Props) {
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
}
