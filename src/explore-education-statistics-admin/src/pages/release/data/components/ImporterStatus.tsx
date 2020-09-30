import releaseDataFileService, {
  DataFile,
  DataFileImportStatus,
  ImportStatusCode,
} from '@admin/services/releaseDataFileService';
import Details from '@common/components/Details';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tag, { TagProps } from '@common/components/Tag';
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
    case 'RUNNING_PHASE_1':
      return 'Validating';
    case 'RUNNING_PHASE_2':
      return 'Importing';
    case 'RUNNING_PHASE_3':
      return 'Importing';
    case 'COMPLETE':
      return 'Complete';
    case 'FAILED':
      return 'Failed';
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
    case 'RUNNING_PHASE_1':
    case 'RUNNING_PHASE_2':
    case 'RUNNING_PHASE_3':
      return 'orange';
    case 'COMPLETE':
      return 'green';
    case 'FAILED':
      return 'red';
    default:
      return undefined;
  }
};

export const terminalImportStatuses: ImportStatusCode[] = [
  'NOT_FOUND',
  'COMPLETE',
  'FAILED',
];

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
  const [currentStatus, setCurrentStatus] = useState<DataFileImportStatus>({
    numberOfRows: dataFile.rows,
    status: dataFile.status,
  });

  const fetchStatus = useCallback(async () => {
    const nextStatus = await releaseDataFileService.getDataFileImportStatus(
      releaseId,
      dataFile.fileName,
    );

    setCurrentStatus(nextStatus);

    if (onStatusChange && nextStatus.status !== dataFile.status) {
      onStatusChange(dataFile, nextStatus);
    }
  }, [releaseId, dataFile, onStatusChange]);

  const [cancelInterval] = useInterval(fetchStatus, 5000);

  useMounted(() => {
    fetchStatus();
  });

  useEffect(() => {
    if (terminalImportStatuses.includes(currentStatus.status)) {
      cancelInterval();
    }
  }, [cancelInterval, currentStatus]);

  return (
    <div>
      <div className="dfe-flex dfe-align-items--center">
        <Tag colour={getImportStatusColour(currentStatus.status)} strong>
          {getImportStatusLabel(currentStatus.status)}
        </Tag>

        {!terminalImportStatuses.includes(currentStatus.status) && (
          <LoadingSpinner
            alert
            hideText
            inline
            size="sm"
            className="govuk-!-margin-left-1"
            text={`Processing data file: ${dataFile.fileName}`}
          />
        )}
      </div>

      {currentStatus.errors && currentStatus.errors.length > 0 && (
        <Details
          className="govuk-!-margin-top-1 govuk-!-margin-bottom-0"
          summary="See errors"
        >
          <ul className="govuk-!-margin-top-0">
            {currentStatus.errors.map((error, index) => (
              <li key={index.toString()}>{error}</li>
            ))}
          </ul>
        </Details>
      )}
    </div>
  );
};

export default ImporterStatus;
