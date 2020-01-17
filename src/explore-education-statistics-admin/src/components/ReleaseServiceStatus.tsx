import React, { useState, useEffect, useRef } from 'react';
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
  // has additional props too
  overallStage: string;
}

const ReleaseServiceStatus = ({ releaseId, refreshPeriod = 5000 }: Props) => {
  const [currentStatus, setCurrentStatus] = useState<ReleaseServiceStatus>();
  const [statusColor, setStatusColor] = useState<
    'blue' | 'orange' | 'red' | 'green' // could be StatusBlockProps.color once the component is created
  >('blue');

  function fetchReleaseServiceStatus() {
    // update the status
    return dashboardService.getReleaseStatus(releaseId).then(([status]) => {
      console.log(status);
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

  useEffect(() => {
    // overallStage status changed?
    // stop timer? set block color?
    if (currentStatus && currentStatus.overallStage) {
      switch (currentStatus.overallStage) {
        case 'Scheduled':
        case 'Started':
          // blue?
          break;
        case 'Cancelled':
          // blue? and stop timer?
          break;
        case 'Invalid':
        case 'Failed':
          // red? and stop timer?
          break;
        case 'Complete':
          // green? and stop timer?
          break;
        default:
          break;
      }
    }
  }, [currentStatus]);

  if (!currentStatus) return null;
  return (
    <>
      <div>
        <div>
          {/* this strong block could be a StatusBlock component that takes color/className prop(s) and renders the text children */}
          <strong className={classNames('govuk-!-margin-right-1', 'govuk-tag')}>
            {currentStatus && currentStatus.overallStage}
          </strong>
          {/* show loading spinner if timer is active (it's still processing) */}

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
