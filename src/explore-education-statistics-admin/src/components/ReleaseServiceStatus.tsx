import StatusBlock, { StatusBlockProps } from '@admin/components/StatusBlock';
import styles from '@admin/pages/release/edit-release/data/ReleaseDataUploadsSection.module.scss';
import dashboardService from '@admin/services/dashboard/service';
import Details from '@common/components/Details';
import React, { useCallback, useEffect, useRef, useState } from 'react';
import { forceCheck } from 'react-lazyload';

interface Props {
  releaseId: string;
  refreshPeriod?: number;
  exclude?: 'status' | 'details';
}

export interface ReleaseStatus {
  overallStage: string;
}

const ReleaseServiceStatus = ({
  releaseId,
  refreshPeriod = 10000,
  exclude,
}: Props) => {
  const [currentStatus, setCurrentStatus] = useState<ReleaseStatus>();
  const [statusColor, setStatusColor] = useState<StatusBlockProps['color']>(
    'blue',
  );
  const timeoutRef = useRef<NodeJS.Timeout>();

  const fetchReleaseServiceStatus = useCallback(() => {
    return dashboardService
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
          if (
            status &&
            (status.overallStage === 'Started' ||
              status.overallStage === 'Queued')
          ) {
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
          case 'Scheduled':
            return { color: 'blue', text: status };
          case 'NotStarted':
            return { color: 'blue', text: 'Not Started' };
          case 'Invalid':
          case 'Failed':
          case 'Cancelled':
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
        <StatusBlock
          color={statusColor}
          text={
            currentStatus
              ? currentStatus.overallStage
              : 'Waiting to be scheduled...'
          }
        />
      )}

      {currentStatus &&
        currentStatus.overallStage !== 'Scheduled' &&
        currentStatus.overallStage !== 'Invalid' &&
        exclude !== 'details' && (
          <Details className={styles.errorSummary} summary="View stages">
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
                      color={color}
                      text={`${key.replace('Stage', '')} - ${text}`}
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
