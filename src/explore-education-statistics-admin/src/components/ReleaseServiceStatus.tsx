import StatusBlock, { StatusBlockColors } from '@admin/components/StatusBlock';
import useReleaseServiceStatus, {
  StatusDetail,
} from '@admin/hooks/useReleaseServiceStatus';
import { ReleaseStageStatuses } from '@admin/services/releaseService';
import Details from '@common/components/Details';
import React from 'react';
import Tag from '@common/components/Tag';

const approvedStatuses = ['Complete', 'Scheduled'];
const notStartedStatuses = ['Validating', 'Invalid'];

export const getStatusDetail = (status: string): StatusDetail => {
  if (!status) {
    return { color: 'orange', text: 'Requesting status' };
  }
  switch (status) {
    case 'NotStarted':
      return { color: 'blue', text: 'Not Started' };
    case 'Scheduled':
      return { color: 'blue', text: status };
    case 'Failed':
    case 'Cancelled':
    case 'Superseded':
      return { color: 'red', text: status };
    case 'Validating':
    case 'Queued':
    case 'Started':
      return { color: 'orange', text: status };
    case 'Complete':
      return { color: 'green', text: status };
    default:
      return { color: 'red', text: 'Error' };
  }
};

interface CurrentStatusBlockProps {
  color?: StatusBlockColors;
  currentStatus?: ReleaseStageStatuses;
  isApproved?: boolean;
}

export const CurrentStatusBlock = ({
  color,
  currentStatus,
  isApproved = false,
}: CurrentStatusBlockProps) => {
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

interface ReleaseStagesProps {
  checklistStyle?: boolean;
  currentStatus?: ReleaseStageStatuses;
  includeScheduled?: boolean;
}

export const ReleaseStages = ({
  checklistStyle = false,
  currentStatus,
  includeScheduled = false,
}: ReleaseStagesProps) => {
  if (
    !currentStatus ||
    (includeScheduled
      ? notStartedStatuses.includes(currentStatus.overallStage)
      : [...notStartedStatuses, 'Scheduled'].includes(
          currentStatus.overallStage,
        ))
  ) {
    return null;
  }
  return (
    <Details
      className="govuk-!-margin-bottom-0 govuk-!-margin-top-1"
      summary="View stages"
    >
      {checklistStyle &&
        ![...notStartedStatuses, 'Scheduled'].includes(
          currentStatus.overallStage,
        ) && (
          <p className="govuk-!-font-weight-bold">Release process started</p>
        )}
      <ul className="govuk-list">
        {Object.entries(currentStatus).map(([key, val]) => {
          if (['overallStage', 'releaseId', 'lastUpdated'].includes(key)) {
            return null;
          }
          const { color, text } = getStatusDetail(val);

          if (!color) {
            return null;
          }

          return (
            <li key={key}>
              <StatusBlock
                checklistStyle={checklistStyle}
                color={color}
                text={
                  checklistStyle
                    ? `${key.replace('Stage', '')} ${text}`
                    : `${key.replace('Stage', '')} - ${text}`
                }
              />
            </li>
          );
        })}
      </ul>
    </Details>
  );
};

interface ReleaseServiceStatusProps {
  isApproved?: boolean;
  releaseId: string;
}

const ReleaseServiceStatus = ({
  isApproved = false,
  releaseId,
}: ReleaseServiceStatusProps) => {
  const { currentStatus, currentStatusDetail } = useReleaseServiceStatus({
    releaseId,
  });
  return (
    <>
      <CurrentStatusBlock
        currentStatus={currentStatus}
        color={currentStatusDetail.color}
        isApproved={isApproved}
      />
      <ReleaseStages currentStatus={currentStatus} />
    </>
  );
};

export default ReleaseServiceStatus;
