import releaseDataFileService, {
  DataFile,
  DataFileImportStatus,
  ImportStatusCode,
} from '@admin/services/releaseDataFileService';
import Details from '@common/components/Details';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ProgressBar from '@common/components/ProgressBar';
import Tag, { TagProps } from '@common/components/Tag';
import WarningMessage from '@common/components/WarningMessage';
import useInterval from '@common/hooks/useInterval';
import useMounted from '@common/hooks/useMounted';
import React, { useCallback, useEffect, useState } from 'react';

export const getImportStatusLabel = (
  statusCode: ImportStatusCode,
): string | undefined => {
  switch (statusCode) {
    case 'NOT_FOUND':
      return 'Not Found';
    case 'UPLOADING':
      return 'Uploading';
    case 'QUEUED':
      return 'Queued';
    case 'PROCESSING_ARCHIVE_FILE':
      return 'Processing archive file';
    case 'STAGE_1':
      return 'Validating';
    case 'STAGE_2':
      return 'Importing';
    case 'STAGE_3':
      return 'Importing';
    case 'STAGE_4':
      return 'Importing';
    case 'COMPLETE':
      return 'Complete';
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
): TagProps['colour'] => {
  switch (statusCode) {
    case 'NOT_FOUND':
    case 'UPLOADING':
    case 'QUEUED':
    case 'PROCESSING_ARCHIVE_FILE':
    case 'STAGE_1':
    case 'STAGE_2':
    case 'STAGE_3':
    case 'STAGE_4':
    case 'CANCELLING':
      return 'orange';
    case 'COMPLETE':
      return 'green';
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
  releaseId: string;
  dataFile: DataFile;
  onStatusChange?: ImporterStatusChangeHandler;
}
const ImporterStatus = ({
  releaseId,
  dataFile,
  onStatusChange,
}: ImporterStatusProps) => {
  const [currentStatus, setCurrentStatus] = useState<StatusState>({
    status: dataFile.status,
    percentageComplete: 0,
  });

  const fetchStatus = useCallback(async () => {
    const nextStatus = await releaseDataFileService.getDataFileImportStatus(
      releaseId,
      dataFile,
    );

    setCurrentStatus(nextStatus);

    if (onStatusChange && nextStatus.status !== dataFile.status) {
      onStatusChange(dataFile, nextStatus);
    }
  }, [releaseId, dataFile, onStatusChange]);

  const [cancelInterval] = useInterval(fetchStatus, 5000);

  useMounted(() => {
    if (dataFile.status !== 'COMPLETE') {
      fetchStatus();
    }
  });

  useEffect(() => {
    if (terminalImportStatuses.includes(currentStatus.status)) {
      cancelInterval();
    }
  }, [cancelInterval, currentStatus]);

  const hasTerminalStatus = terminalImportStatuses.includes(
    currentStatus.status,
  );

  return (
    <div>
      <div className="dfe-flex dfe-align-items--center">
        <Tag
          colour={
            dataFile.replacedBy
              ? 'blue'
              : getImportStatusColour(currentStatus.status)
          }
          strong
        >
          {dataFile.replacedBy
            ? 'Data replacement in progress'
            : getImportStatusLabel(currentStatus.status)}
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

      {currentStatus.status === 'FAILED' && (
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
            <a href="https://rsconnect/rsc/dfe-published-data-qa/">
              data screener
            </a>{' '}
            to check for potential causes of this failure before trying again.
          </WarningMessage>
        </>
      )}
    </div>
  );
};

export default ImporterStatus;
