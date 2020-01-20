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

interface ReleaseServiceStatus {
  overallStage: string;
}

const ReleaseServiceStatus = ({
  releaseId,
  refreshPeriod = 5000,
  exclude,
  handleApiErrors,
}: Props & ErrorControlProps) => {
  const [currentStatus, setCurrentStatus] = useState<ReleaseServiceStatus>();
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

  const statusDetailColor = (text: string) => {
    if (currentStatus) {
      switch (text) {
        case 'Scheduled':
          return 'blue';
        case 'Invalid':
        case 'Failed':
        case 'Cancelled':
          return 'red';
        case 'Queued':
        case 'Started':
          return 'orange';
        case 'Complete':
          return 'green';
        default:
          return undefined;
      }
    }
    return undefined;
  };

  useEffect(() => {
    if (currentStatus && currentStatus.overallStage) {
      const color = statusDetailColor(currentStatus.overallStage);
      if (color === ('red' || 'green')) {
        cancelTimer();
      }
      setStatusColor(color);
    }
  }, [currentStatus]);

  if (!currentStatus) return null;
  return (
    <>
      {exclude !== 'status' && (
        <StatusBlock color={statusColor} text={currentStatus.overallStage} />
      )}

      {currentStatus &&
        currentStatus.overallStage === 'Started' &&
        exclude !== 'details' && (
          <Details className={styles.errorSummary} summary="View stages">
            <ul className="govuk-list">
              {Object.entries(currentStatus).map(
                ([key, val]) =>
                  statusDetailColor(val) !== undefined && (
                    <li key={key}>
                      <StatusBlock
                        color={statusDetailColor(val)}
                        text={`${key.replace('Stage', '')} - ${val}`}
                      />{' '}
                    </li>
                  ),
              )}
            </ul>
          </Details>
        )}
    </>
  );
};

export default withErrorControl(ReleaseServiceStatus);
