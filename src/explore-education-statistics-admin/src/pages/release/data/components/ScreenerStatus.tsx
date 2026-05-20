import releaseDataFileService, {
  DataSetScreenerProgress,
  DataSetUpload,
  DataSetUploadStatus,
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

export const getDataSetUploadStatusLabel = (
  statusCode: DataSetUploadStatus,
): string | undefined => {
  switch (statusCode) {
    case 'UPLOADING':
      return 'Uploading';
    case 'SCREENING':
      return 'Screening';
    case 'PENDING_REVIEW':
      return 'Pending review';
    case 'PENDING_IMPORT':
      return 'Pending import';
    case 'FAILED_SCREENING':
      return 'Failed screening';
    case 'SCREENER_ERROR':
      return 'Screener error';
    default:
      return undefined;
  }
};

export const getDataSetUploadStatusColour = (
  statusCode: DataSetUploadStatus,
): TagProps['colour'] => {
  switch (statusCode) {
    case 'UPLOADING':
      return 'turquoise';
    case 'PENDING_REVIEW':
      return 'orange';
    case 'SCREENING':
      return 'blue';
    case 'PENDING_IMPORT':
      return 'light-blue';
    case 'FAILED_SCREENING':
    case 'SCREENER_ERROR':
      return 'red';
    default:
      return undefined;
  }
};

type StatusState = Pick<
  DataSetScreenerProgress,
  'stage' | 'percentageComplete'
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
    // status: dataSetUpload.status,
    stage: 'PENDING',
    percentageComplete: 0,
  });

  const fetchStatus = useCallback(async () => {
    const nextStatus = await releaseDataFileService.getDataFileScreeningStatus(
      releaseVersionId,
    );

    console.log(nextStatus[0].stage); // TODO: remove
    console.log(nextStatus[0].status); // TODO: remove
    console.log(nextStatus[0].percentageComplete); // TODO: remove

    setCurrentStatus(nextStatus[0]);

    // if (onStatusChange && nextStatus.stage !== dataSetUpload.status) {
    //   onStatusChange(dataSetUpload, nextStatus);
    // }
    //   }, [releaseVersionId, dataSetUpload, onStatusChange]);
  }, [releaseVersionId, dataSetUpload]);

  const terminalScreeningStatuses: DataSetUploadStatus[] = [
    'SCREENER_ERROR',
    'PENDING_REVIEW',
    'PENDING_IMPORT',
    'FAILED_SCREENING',
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
      <Tag colour={getDataSetUploadStatusColour(dataSetUpload.status)}>
        {getDataSetUploadStatusLabel(dataSetUpload.status)}
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
