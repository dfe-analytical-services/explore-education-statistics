import {
  DataSetUploadScreeningStatus,
  ScreenerTestResult,
} from '@admin/services/releaseDataFileService';
import ProgressBar from '@common/components/ProgressBar';
import React from 'react';
import Tag, { TagProps } from '@common/components/Tag';
import LoadingSpinner from '@common/components/LoadingSpinner';

interface Props {
  dataSetTitle: string;
  percentageComplete: number;
  status: DataSetUploadScreeningStatus;
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

export const terminalScreeningStatuses: DataSetUploadScreeningStatus[] = [
  'ScreenerError',
  'PendingReview',
  'PendingImport',
  'FailedScreening',
];

export default function ScreenerStatus({
  dataSetTitle,
  percentageComplete,
  status,
}: Props) {
  const hasTerminalStatus = terminalScreeningStatuses.includes(status);

  return (
    <>
      <Tag colour={getDataSetUploadScreeningStatusColour(status)}>
        {getDataSetUploadScreeningStatusLabel(status)}
      </Tag>

      {!hasTerminalStatus && (
        <LoadingSpinner inline size="sm" className="govuk-!-margin-left-1" />
      )}

      {!hasTerminalStatus && (
        <ProgressBar
          testId={`${dataSetTitle}-screener-progress-bar`}
          className="govuk-!-margin-top-2"
          value={percentageComplete}
          width={200}
        />
      )}
    </>
  );
}
