import releaseDataFileQueries from '@admin/queries/releaseDataFileQueries';
import {
  DataSetScreenerProgress,
  DataSetUpload,
  DataSetUploadScreeningStatus,
  ScreenerTestResult,
} from '@admin/services/releaseDataFileService';
import ProgressBar from '@common/components/ProgressBar';
import { useQuery } from '@tanstack/react-query';
import React from 'react';
import Tag, { TagProps } from '@common/components/Tag';
import LoadingSpinner from '@common/components/LoadingSpinner';

export type ScreenerStatusChangeHandler = (
  dataSetUpload: DataSetUpload,
  progress: DataSetScreenerProgress,
) => void;

interface Props {
  dataSetUpload: DataSetUpload;
  releaseVersionId: string;
  onStatusChange?: ScreenerStatusChangeHandler;
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

export const terminalScreeningStatuses: DataSetUploadScreeningStatus[] = [
  'ScreenerError',
  'PendingReview',
  'PendingImport',
  'FailedScreening',
];

export default function ScreenerStatus({
  dataSetUpload,
  releaseVersionId,
  onStatusChange,
}: Props) {
  const { data } = useQuery({
    ...releaseDataFileQueries.screeningStatus(
      releaseVersionId,
      dataSetUpload.id,
    ),
    // No point polling an upload that has already finished screening.
    enabled: !terminalScreeningStatuses.includes(dataSetUpload.screeningStatus),
    // Poll every 5 seconds until screening reaches a status it can't move on from.
    refetchInterval: nextStatus =>
      nextStatus && terminalScreeningStatuses.includes(nextStatus.status)
        ? false
        : 5000,
    onSuccess: nextStatus => {
      if (nextStatus.status !== dataSetUpload.screeningStatus) {
        onStatusChange?.(dataSetUpload, nextStatus);
      }
    },
  });

  const currentStatus: StatusState = data ?? {
    status: dataSetUpload.screeningStatus,
    percentageComplete: 0,
    stage: 'PENDING',
    completed: false,
  };

  const hasTerminalStatus = terminalScreeningStatuses.includes(
    currentStatus.status,
  );

  return (
    <>
      <Tag colour={getDataSetUploadScreeningStatusColour(currentStatus.status)}>
        {getDataSetUploadScreeningStatusLabel(currentStatus.status)}
      </Tag>

      {!hasTerminalStatus && (
        <LoadingSpinner inline size="sm" className="govuk-!-margin-left-1" />
      )}

      {!hasTerminalStatus && (
        <ProgressBar
          testId={`${dataSetUpload.dataSetTitle}-screener-progress-bar`}
          className="govuk-!-margin-top-2"
          value={currentStatus.percentageComplete}
          width={200}
        />
      )}
    </>
  );
}
