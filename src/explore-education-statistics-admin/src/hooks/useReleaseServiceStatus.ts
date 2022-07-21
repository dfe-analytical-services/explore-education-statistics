import { StatusBlockProps } from '@admin/components/StatusBlock';
import { getStatusDetail } from '@admin/components/ReleaseServiceStatus';
import releaseService, {
  ReleaseStageStatuses,
} from '@admin/services/releaseService';
import { useCallback, useEffect, useRef, useState } from 'react';
import { forceCheck } from 'react-lazyload';

export interface StatusDetail {
  color: StatusBlockProps['color'];
  text: string;
}

interface Props {
  refreshPeriod?: number;
  releaseId: string;
}

/**
 * Hook to get and refresh the status for the given release.
 */
export default function useReleaseServiceStatus({
  refreshPeriod = 10000,
  releaseId,
}: Props): {
  currentStatus: ReleaseStageStatuses | undefined;
  currentStatusDetail: StatusDetail;
} {
  const [currentStatus, setCurrentStatus] = useState<ReleaseStageStatuses>();
  const [currentStatusDetail, setStatusDetail] = useState<StatusDetail>({
    color: 'blue',
    text: '',
  });

  const timeoutRef = useRef<NodeJS.Timeout>();

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

  useEffect(() => {
    if (currentStatus && currentStatus.overallStage) {
      const status = getStatusDetail(currentStatus.overallStage);
      if (status.color === 'red' || status.color === 'green') {
        cancelTimer();
      }
      setStatusDetail(status);
    }
  }, [currentStatus]);

  return { currentStatus, currentStatusDetail };
}
