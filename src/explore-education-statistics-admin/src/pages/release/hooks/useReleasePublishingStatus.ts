import { StatusBlockProps } from '@admin/components/StatusBlock';
import getStatusDetail from '@admin/pages/release/utils/getStatusDetail';
import releaseService, {
  ReleaseStageStatuses,
} from '@admin/services/releaseService';
import { useCallback, useEffect, useRef, useState } from 'react';
import { forceCheck } from 'react-lazyload';

export interface StatusDetail {
  color: StatusBlockProps['color'];
  text: string;
}

interface Options {
  refreshPeriod?: number;
  releaseId: string;
}

/**
 * Hook to get and refresh the status for the given release.
 */
export default function useReleasePublishingStatus({
  refreshPeriod = 10000,
  releaseId,
}: Options): {
  currentStatus: ReleaseStageStatuses | undefined;
  currentStatusDetail: StatusDetail;
} {
  const [currentStatus, setCurrentStatus] = useState<ReleaseStageStatuses>();
  const [currentStatusDetail, setStatusDetail] = useState<StatusDetail>({
    color: 'blue',
    text: '',
  });

  const timeoutRef = useRef<NodeJS.Timeout>();

  const fetchReleasePublishingStatus = useCallback(async () => {
    const status = await releaseService.getReleaseStatus(releaseId);
    if (!status) {
      // 204 response waiting for status
      setCurrentStatus({ overallStage: 'Validating' });
      timeoutRef.current = setTimeout(
        fetchReleasePublishingStatus,
        refreshPeriod,
      );
    } else {
      setCurrentStatus(status);
      if (status && status.overallStage === 'Started') {
        timeoutRef.current = setTimeout(
          fetchReleasePublishingStatus,
          refreshPeriod,
        );
      }
    }
    forceCheck();
  }, [releaseId, refreshPeriod]);

  function cancelTimer() {
    if (timeoutRef.current) clearInterval(timeoutRef.current);
  }

  useEffect(() => {
    fetchReleasePublishingStatus();
    return () => {
      // cleans up the timeout
      cancelTimer();
    };
  }, [fetchReleasePublishingStatus]);

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
