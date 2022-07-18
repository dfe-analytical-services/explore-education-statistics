import StatusBlock, { StatusBlockProps } from '@admin/components/StatusBlock';
import releaseService, {
  ReleaseStageStatuses,
} from '@admin/services/releaseService';
import Details from '@common/components/Details';
import React, { useCallback, useEffect, useRef, useState } from 'react';
import Tag from '@common/components/Tag';
import { forceCheck } from 'react-lazyload';

interface Props {
  releaseId: string;
  refreshPeriod?: number;
  exclude?: 'status' | 'details';
  isApproved?: boolean;
  newAdminStyle?: boolean; // EES-3217 CLEANUP
}

const ReleaseServiceStatus = ({
  releaseId,
  refreshPeriod = 10000,
  exclude,
  isApproved = false,
  newAdminStyle = false,
}: Props) => {
  const [currentStatus, setCurrentStatus] = useState<ReleaseStageStatuses>();
  const [statusColor, setStatusColor] = useState<StatusBlockProps['color']>(
    'blue',
  );
  const timeoutRef = useRef<NodeJS.Timeout>();
  const notStartedStatuses = newAdminStyle
    ? ['Validating', 'Invalid']
    : ['Validating', 'Scheduled', 'Invalid'];

  const fetchReleaseServiceStatus = useCallback(() => {
    return releaseService
      .getReleaseStatus(releaseId)
      .then(status => {
        if (!status) {
          // 204 response waiting for status
          setCurrentStatus({ overallStage: 'Validating' });
          timeoutRef.current = setTimeout(
            fetchReleaseServiceStatus,
            refreshPeriod,
          );
        } else {
          setCurrentStatus(status);
          if (status && status.overallStage === 'Started') {
            timeoutRef.current = setTimeout(
              fetchReleaseServiceStatus,
              refreshPeriod,
            );
          }
        }
      })
      .then(forceCheck);
  }, [releaseId, refreshPeriod]);

  function cancelTimer() {
    if (timeoutRef.current) clearInterval(timeoutRef.current);
  }

  useEffect(() => {
    fetchReleaseServiceStatus();
    return () => {
      // cleans up the timeout
      cancelTimer();
    };
  }, [fetchReleaseServiceStatus]);

  const statusDetailColor = useCallback(
    (status: string): { color: StatusBlockProps['color']; text: string } => {
      if (currentStatus) {
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
      }
      return { color: 'orange', text: 'Requesting status' };
    },
    [currentStatus],
  );

  useEffect(() => {
    if (currentStatus && currentStatus.overallStage) {
      const { color } = statusDetailColor(currentStatus.overallStage);
      if (color === 'red' || color === 'green') {
        cancelTimer();
      }
      setStatusColor(color);
    }
  }, [currentStatus, statusDetailColor]);

  if (!currentStatus) return null;

  return (
    <>
      {exclude !== 'status' && (
        <>
          {isApproved &&
            !['Complete', 'Scheduled'].includes(currentStatus.overallStage) && (
              <Tag>Approved</Tag>
            )}
          <StatusBlock
            color={statusColor}
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
      )}

      {currentStatus &&
        !notStartedStatuses.includes(currentStatus.overallStage) &&
        exclude !== 'details' && (
          <Details
            className="govuk-!-margin-bottom-0 govuk-!-margin-top-1"
            summary="View stages"
          >
            {newAdminStyle &&
              !['Validating', 'Scheduled', 'Invalid'].includes(
                currentStatus.overallStage,
              ) && <p>Release process started</p>}
            <ul className="govuk-list">
              {Object.entries(currentStatus).map(([key, val]) => {
                if (['overallStage', 'releaseId', 'lastUpdated'].includes(key))
                  return null;
                const { color, text } = statusDetailColor(val);

                if (!color) {
                  return null;
                }

                return (
                  <li key={key}>
                    <StatusBlock
                      checklistStyle
                      color={color}
                      text={
                        newAdminStyle
                          ? `${key.replace('Stage', '')} ${text}`
                          : `${key.replace('Stage', '')} - ${text}`
                      }
                      newAdminStyle={newAdminStyle}
                    />
                  </li>
                );
              })}
            </ul>
          </Details>
        )}
    </>
  );
};

export default ReleaseServiceStatus;
