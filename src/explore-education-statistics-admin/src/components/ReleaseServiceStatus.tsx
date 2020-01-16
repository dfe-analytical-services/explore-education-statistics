import React, { useState, useEffect } from 'react';
import classNames from 'classnames';
import styles from '@admin/pages/release/edit-release/data/ReleaseDataUploadsSection.module.scss';
import dashboardService from '@admin/services/dashboard/service';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Details from '@common/components/Details';

interface Props {
  releaseId: string;
  refreshPeriod?: number;
}

interface ReleaseServiceStatus {
  overallStage: string;
}

const ReleaseServiceStatus = ({ releaseId, refreshPeriod = 5000 }: Props) => {
  const [currentStatus, setCurrentStatus] = useState<ReleaseServiceStatus>();
  const [intervalId, setIntervalId] = useState<NodeJS.Timeout | undefined>();

  function fetchReleaseServiceStatus() {
    // update the status
    dashboardService.getReleaseStatus(releaseId).then(([status]) => {
      console.log(status);
      setCurrentStatus(status);
    });
  }

  function initialiseTimer() {
    fetchReleaseServiceStatus();
    setIntervalId(setInterval(fetchReleaseServiceStatus, refreshPeriod));
  }

  function cancelTimer() {
    if (intervalId) {
      clearInterval(intervalId);
      setIntervalId(undefined);
    }
  }
  useEffect(() => {
    initialiseTimer();
  }, []);

  useEffect(() => {
    return function cleanup() {
      cancelTimer();
    };
  });

  function getReleaseServiceStatusClass(
    releaseServiceCode: string,
  ): string[] | undefined {
    switch (releaseServiceCode) {
      case 'NOT_FOUND':
        return [styles.ragStatusAmber];
      case 'RUNNING_PHASE_1':
        return [styles.ragStatusAmber];
      case 'RUNNING_PHASE_2':
        return [styles.ragStatusAmber];
      case 'RUNNING_PHASE_3':
        return [styles.ragStatusAmber];
      case 'COMPLETE':
        cancelTimer();
        return [styles.ragStatusGreen];
      case 'FAILED':
        cancelTimer();
        return [styles.ragStatusRed];
      default:
        return undefined;
    }
  }
  if (!currentStatus) return null;
  return (
    <>
      <div>
        <div>
          <strong
            className={classNames(
              'govuk-!-margin-right-1',
              'govuk-tag',
              //   currentStatus && getReleaseServiceStatusClass(currentStatus),
            )}
          >
            {currentStatus && currentStatus.overallStage}
          </strong>

          {currentStatus && currentStatus.overallStage === 'Started' && (
            <Details className={styles.errorSummary} summary="See more">
              <ul>
                {Object.entries(currentStatus).map(([key, val]) => (
                  <li key={key}>
                    {key} - {val}
                  </li>
                ))}
              </ul>
            </Details>
          )}
        </div>
      </div>
    </>
  );
};

export default ReleaseServiceStatus;
