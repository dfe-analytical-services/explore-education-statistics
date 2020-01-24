import StatusBlock, { StatusBlockProps } from '@admin/components/StatusBlock';
import styles from '@admin/pages/release/edit-release/data/ReleaseDataUploadsSection.module.scss';
import dashboardService from '@admin/services/dashboard/service';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import Details from '@common/components/Details';
import React, { useCallback, useEffect, useRef, useState } from 'react';

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
  handleApiErrors,
}: Props & ErrorControlProps) => {
  const [currentStatus, setCurrentStatus] = useState<ReleaseStatus>();
  const [statusColor, setStatusColor] = useState<StatusBlockProps['color']>(
    'blue',
  );

  const fetchReleaseServiceStatus = useCallback(() => {
    return dashboardService
      .getReleaseStatus(releaseId)
      .then(setCurrentStatus)
      .catch(handleApiErrors);
  }, [releaseId, handleApiErrors]);

  const intervalRef = useRef<NodeJS.Timeout>();

  function cancelTimer() {
    if (intervalRef.current) clearInterval(intervalRef.current);
  }

  useEffect(() => {
    fetchReleaseServiceStatus();
    intervalRef.current = setInterval(fetchReleaseServiceStatus, refreshPeriod);
    return () => {
      cancelTimer();
    };
  }, [fetchReleaseServiceStatus, refreshPeriod]);

  const statusDetailColor = (
    status: string,
  ): { color: StatusBlockProps['color']; text: string } => {
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
        case 'Queued':
        case 'Started':
          return { color: 'orange', text: status };
        case 'Complete':
          return { color: 'green', text: status };
        default:
          return { color: undefined, text: '' };
      }
    }
    return { color: undefined, text: '' };
  };

  useEffect(() => {
    if (currentStatus && currentStatus.overallStage) {
      const { color } = statusDetailColor(currentStatus.overallStage);
      if (color === 'red' || color === 'green') {
        cancelTimer();
      }
      setStatusColor(color);
    }
  }, [currentStatus]);

  if (!currentStatus) return null;
  return (
    <>
      {exclude !== 'status' && (
        <StatusBlock
          color={statusColor}
          text={`Release Process - ${currentStatus.overallStage}`}
        />
      )}

      {currentStatus &&
        currentStatus.overallStage !== 'Scheduled' &&
        currentStatus.overallStage !== 'Invalid' &&
        exclude !== 'details' && (
          <Details className={styles.errorSummary} summary="View stages">
            <ul className="govuk-list">
              {Object.entries(currentStatus).map(([key, val]) => {
                if (key === 'overallStage') return null;
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

export default withErrorControl(ReleaseServiceStatus);
