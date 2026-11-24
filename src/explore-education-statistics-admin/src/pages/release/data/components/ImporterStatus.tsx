import releaseDataFileQueries from '@admin/queries/releaseDataFileQueries';
import {
  DataFile,
  DataFileImportStatus,
  ImportStatusCode,
} from '@admin/services/releaseDataFileService';
import Details from '@common/components/Details';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ProgressBar from '@common/components/ProgressBar';
import Tag, { TagProps } from '@common/components/Tag';
import WarningMessage from '@common/components/WarningMessage';
import { useQuery } from '@tanstack/react-query';
import React, { useEffect, useRef } from 'react';

const getImportStatusLabel = (
  statusCode: ImportStatusCode,
  replacementInProgress: boolean,
): string | undefined => {
  switch (statusCode) {
    case 'NOT_FOUND':
      return 'Not found';
    case 'QUEUED':
      return 'Queued';
    case 'STAGE_1':
      return 'Validating';
    case 'STAGE_2':
      return 'Importing';
    case 'STAGE_3':
      return 'Importing';
    case 'COMPLETE':
      return replacementInProgress ? 'Replacing' : 'Complete';
    case 'FAILED':
      return 'Failed';
    case 'CANCELLING':
      return 'Cancelling';
    case 'CANCELLED':
      return 'Cancelled';
    default:
      return undefined;
  }
};

const getImportStatusColour = (
  statusCode: ImportStatusCode,
  replacementInProgress: boolean,
): TagProps['colour'] => {
  switch (statusCode) {
    case 'NOT_FOUND':
    case 'QUEUED':
    case 'STAGE_1':
    case 'STAGE_2':
    case 'STAGE_3':
    case 'CANCELLING':
      return 'orange';
    case 'COMPLETE':
      return replacementInProgress ? 'blue' : 'green';
    case 'FAILED':
    case 'CANCELLED':
      return 'red';
    default:
      return undefined;
  }
};

export const terminalImportStatuses: ImportStatusCode[] = [
  'NOT_FOUND',
  'COMPLETE',
  'FAILED',
  'CANCELLED',
];

type StatusState = Pick<
  DataFileImportStatus,
  'status' | 'percentageComplete' | 'errors'
>;

export type ImporterStatusChangeHandler = (
  dataFile: DataFile,
  status: DataFileImportStatus,
) => void;

interface ImporterStatusProps {
  className?: string;
  dataFile: DataFile;
  hideErrors?: boolean;
  releaseVersionId: string;
  onStatusChange?: ImporterStatusChangeHandler;
}

const ImporterStatus = ({
  className,
  dataFile,
  hideErrors,
  releaseVersionId,
  onStatusChange,
}: ImporterStatusProps) => {
  const { data, dataUpdatedAt, isFetchedAfterMount } = useQuery({
    ...releaseDataFileQueries.importStatus(releaseVersionId, dataFile.id),
    enabled: dataFile.status !== 'COMPLETE',
    // Poll every 5 seconds until the import reaches a status it can't move on from.
    refetchInterval: query =>
      query.state.data &&
      terminalImportStatuses.includes(query.state.data.status)
        ? false
        : 5000,
  });

  // Read the latest props from a ref so that a new callback or data file
  // reference from the parent doesn't re-trigger the effect below.
  const latestProps = useRef({ dataFile, onStatusChange });
  latestProps.current = { dataFile, onStatusChange };

  // Notify the parent once per fetch. Data that was already in the cache
  // when this mounted is ignored, as it may be stale.
  useEffect(() => {
    if (!data || !isFetchedAfterMount) return;
    const { dataFile: currentDataFile, onStatusChange: notify } =
      latestProps.current;
    if (data.status !== currentDataFile.status) {
      notify?.(currentDataFile, data);
    }
  }, [data, dataUpdatedAt, isFetchedAfterMount]);

  const currentStatus: StatusState = data ?? {
    status: dataFile.status,
    percentageComplete: 0,
  };

  const hasTerminalStatus = terminalImportStatuses.includes(
    currentStatus.status,
  );

  return (
    <div className={className}>
      <div className="dfe-flex dfe-align-items--center">
        <Tag
          colour={getImportStatusColour(
            currentStatus.status,
            dataFile.replacementInProgress,
          )}
        >
          {getImportStatusLabel(
            currentStatus.status,
            dataFile.replacementInProgress,
          )}
        </Tag>

        {!hasTerminalStatus && (
          <LoadingSpinner inline size="sm" className="govuk-!-margin-left-1" />
        )}
      </div>

      {!hasTerminalStatus && (
        <ProgressBar
          className="govuk-!-margin-top-2"
          value={currentStatus.percentageComplete}
          width={200}
        />
      )}

      {currentStatus.status === 'FAILED' && !hideErrors && (
        <>
          {currentStatus.errors && currentStatus.errors.length > 0 && (
            <Details
              className="govuk-!-margin-top-1 govuk-!-margin-bottom-0"
              summary="See errors"
            >
              <ul className="govuk-!-margin-top-0">
                {currentStatus.errors.map((error, index) => (
                  // eslint-disable-next-line react/no-array-index-key
                  <li key={index.toString()}>{error}</li>
                ))}
              </ul>
            </Details>
          )}
          <WarningMessage>
            Try running the file through the{' '}
            <a
              href="https://rsconnect/rsc/dfe-published-data-qa/"
              rel="noopener noreferrer nofollow"
              target="_blank"
            >
              data screener (opens in new tab)
            </a>{' '}
            to check for potential causes of this failure before trying again.
          </WarningMessage>
        </>
      )}
    </div>
  );
};

export default ImporterStatus;
