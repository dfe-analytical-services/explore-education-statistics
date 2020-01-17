import React, { useEffect, useRef, useState } from 'react';
import styles from '@admin/pages/release/edit-release/data/ReleaseDataUploadsSection.module.scss';
import dashboardService from '@admin/services/dashboard/service';
import Details from '@common/components/Details';
import StatusBlock from '@admin/components/StatusBlock';

interface Props {
  releaseId: string;
  refreshPeriod?: number;
  exclude?: 'status' | 'details';
}

interface ReleaseServiceStatus {
  // has additional props too
  overallStage: string;
}

const ReleaseServiceStatus = ({
  releaseId,
  refreshPeriod = 5000,
  exclude,
}: Props) => {
  const [currentStatus, setCurrentStatus] = useState<ReleaseServiceStatus>();
  const [statusColor, setStatusColor] = useState<
    'blue' | 'orange' | 'red' | 'green' | undefined // could be StatusBlockProps.color once the component is created
  >('blue');

  function fetchReleaseServiceStatus() {
    // update the status
    return dashboardService.getReleaseStatus(releaseId).then(([status]) => {
      setCurrentStatus(status);
    });
  }

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
  }, []);

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
    // overallStage status changed?
    // stop timer? set block color?
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
          <Details className={styles.errorSummary} summary="See more">
            <ul className="govuk-list">
              {Object.entries(currentStatus).map(
                ([key, val]) =>
                  statusDetailColor(val) !== undefined && (
                    <li key={key}>
                      <StatusBlock
                        color={statusDetailColor(val)}
                        text={`${key} - ${val}`}
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

export default ReleaseServiceStatus;
