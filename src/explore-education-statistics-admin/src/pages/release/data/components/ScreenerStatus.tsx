import releaseDataFileService, {
  DataSetScreenerProgress,
  DataSetUpload,
  DataSetUploadScreeningStatus,
  ScreenerTestResult,
} from '@admin/services/releaseDataFileService';
import ProgressBar from '@common/components/ProgressBar';
import useInterval from '@common/hooks/useInterval';
import useMounted from '@common/hooks/useMounted';
import React, { useCallback, useEffect, useState } from 'react';
import Tag, { TagProps } from '@common/components/Tag';

export type ScreenerStatusChangeHandler = (
  dataSetUpload: DataSetUpload,
  progress: DataSetScreenerProgress,
) => void;

interface Props {
  className?: string;
  dataSetUpload: DataSetUpload;
  hideErrors?: boolean;
  releaseVersionId: string;
  //   onStatusChange?: ScreenerStatusChangeHandler;
}

export const getScreenerTestResultStatusLabel = (
  statusCode: ScreenerTestResult,
): string | undefined => {
  switch (statusCode) {
    case 'WARNING':
      return 'Warning';
    case 'PASS':
      return 'Pass';
    case 'FAIL':
      return 'Fail';
    default:
      return undefined;
  }
};

export const getScreenerTestResultStatusColour = (
  statusCode: ScreenerTestResult,
): TagProps['colour'] => {
  switch (statusCode) {
    case 'WARNING':
      return 'orange';
    case 'PASS':
      return 'green';
    case 'FAIL':
      return 'red';
    default:
      return undefined;
  }
};

export const getDataSetUploadScreeningStatusLabel = (
  statusCode: DataSetUploadScreeningStatus,
): string | undefined => {
  switch (statusCode) {
    case 'Screening':
      return 'Screening';
    case 'PendingReview':
      return 'Pending review';
    case 'PendingImport':
      return 'Pending import';
    case 'FailedScreening':
      return 'Failed screening';
    case 'ScreenerError':
      return 'Screener error';
    default:
      return undefined;
  }
};

export const getDataSetUploadScreeningStatusColour = (
  statusCode: DataSetUploadScreeningStatus,
): TagProps['colour'] => {
  switch (statusCode) {
    case 'PendingReview':
      return 'orange';
    case 'Screening':
      return 'blue';
    case 'PendingImport':
      return 'light-blue';
    case 'FailedScreening':
    case 'ScreenerError':
      return 'red';
    default:
      return undefined;
  }
};

type StatusState = Pick<
  DataSetScreenerProgress,
  'status' | 'percentageComplete' | 'stage' | 'completed'
>;

export default function ScreenerStatus({
  dataSetUpload,
  releaseVersionId,
  //   onStatusChange,
  hideErrors,
  className,
}: Props) {
  // TODO: Maybe define a base "Status" component which this and "ImporterStatus" inherit
  const [currentStatus, setCurrentStatus] = useState<StatusState>({
    status: dataSetUpload.status,
    percentageComplete: 0,
    stage: 'PENDING',
    completed: false,
  });

  const fetchStatus = useCallback(async () => {
    const nextStatus = await releaseDataFileService.getDataFileScreeningStatus(
      releaseVersionId,
      dataSetUpload.id,
    );

    console.log(nextStatus.stage); // TODO: remove
    console.log(nextStatus.status); // TODO: remove
    console.log(nextStatus.percentageComplete); // TODO: remove

    setCurrentStatus(nextStatus);

    // if (onStatusChange && nextStatus.stage !== dataSetUpload.status) {
    //   onStatusChange(dataSetUpload, nextStatus);
    // }
    //   }, [releaseVersionId, dataSetUpload, onStatusChange]);
  }, [releaseVersionId, dataSetUpload]);

  const terminalScreeningStatuses: DataSetUploadScreeningStatus[] = [
    'ScreenerError',
    'PendingReview',
    'PendingImport',
    'FailedScreening',
  ];

  const [cancelInterval] = useInterval(fetchStatus, 5000);

  useMounted(() => {
    if (!terminalScreeningStatuses.includes(dataSetUpload.status)) {
      fetchStatus();
    }
  });

  useEffect(() => {
    if (terminalScreeningStatuses.includes(dataSetUpload.status)) {
      cancelInterval();
    }
  }, [cancelInterval, currentStatus]);

  const hasTerminalStatus = terminalScreeningStatuses.includes(
    currentStatus.status,
  );

  return (
    <>
      <Tag colour={getDataSetUploadScreeningStatusColour(dataSetUpload.status)}>
        {getDataSetUploadScreeningStatusLabel(dataSetUpload.status)}
      </Tag>
      {!hasTerminalStatus && (
        <ProgressBar
          className="govuk-!-margin-top-2"
          value={currentStatus.percentageComplete}
          width={200}
        />
      )}
    </>
  );
}
