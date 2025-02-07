import { StatusBlockProps } from '@admin/components/StatusBlock';
import getStatusDetail from '@admin/pages/release/utils/getStatusDetail';
import releaseVersionService, {
  ReleaseVersionStageStatus,
} from '@admin/services/releaseVersionService';
import isEqual from 'lodash/isEqual';
import { useCallback, useEffect, useRef, useState } from 'react';
import { forceCheck } from 'react-lazyload';

export interface StatusDetail {
  color: StatusBlockProps['color'];
  text: string;
}

interface Options {
  refreshPeriod?: number;
  releaseVersionId: string;
  onChange?: (status: ReleaseVersionStageStatus) => void;
}

/**
 * Hook to get and refresh the status for the given release.
 */
export default function useReleasePublishingStatus({
  refreshPeriod = 10000,
  releaseVersionId,
  onChange,
}: Options): {
  currentStatus?: ReleaseVersionStageStatus;
  currentStatusDetail: StatusDetail;
} {
  const [currentStatus, setCurrentStatus] =
    useState<ReleaseVersionStageStatus>();
  const [currentStatusDetail, setStatusDetail] = useState<StatusDetail>({
    color: 'blue',
    text: '',
  });

  const timeoutRef = useRef<NodeJS.Timeout>();

  const fetchNextStatus = useCallback(async () => {
    const status = await releaseVersionService.getReleaseVersionStatus(
      releaseVersionId,
    );

    const setNextStatus = (nextStatus: ReleaseVersionStageStatus) => {
      setCurrentStatus(prevStatus => {
        if (onChange && !isEqual(prevStatus, nextStatus)) {
          onChange(nextStatus);
        }

        return nextStatus;
      });
    };

    if (!status) {
      // 204 response waiting for status
      setNextStatus({ overallStage: 'Validating' });

      timeoutRef.current = setTimeout(fetchNextStatus, refreshPeriod);
    } else {
      setNextStatus(status);

      if (status && status.overallStage === 'Started') {
        timeoutRef.current = setTimeout(fetchNextStatus, refreshPeriod);
      }
    }
    forceCheck();
  }, [releaseVersionId, onChange, refreshPeriod]);

  function cancelTimer() {
    if (timeoutRef.current) {
      clearInterval(timeoutRef.current);
    }
  }

  useEffect(() => {
    fetchNextStatus();

    return () => {
      // cleans up the timeout
      cancelTimer();
    };
  }, [fetchNextStatus]);

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
