import styles from '@admin/pages/release/data/components/ReleaseDataUploadsSection.module.scss';
import releaseDataFileService, {
  DataFile,
  DataFileImportStatus,
  ImportStatusCode,
} from '@admin/services/releaseDataFileService';
import Details from '@common/components/Details';
import LoadingSpinner from '@common/components/LoadingSpinner';
import SummaryListItem from '@common/components/SummaryListItem';
import Tag from '@common/components/Tag';
import classNames from 'classnames';
import React, { Component } from 'react';

export const getImportStatusLabel = (statusCode: ImportStatusCode) => {
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

export type ImportStatusChangeHandler = (
  dataFile: DataFile,
  status: ImportStatusCode,
) => void;

interface ImportStatusProps {
  releaseId: string;
  dataFile: DataFile;
  onStatusChange: ImportStatusChangeHandler;
}

class ImporterStatus extends Component<ImportStatusProps> {
  private static refreshPeriod = 5000;

  public state = {
    isFetching: true,
    current: undefined,
    running: false,
  };

  private intervalId?: NodeJS.Timeout;

  public componentDidMount() {
    this.fetchImportStatus();
    this.initialiseTimer();
  }

  public componentWillUnmount() {
    this.cancelTimer();
  }

  private getImportStatusClass = (statusCode: ImportStatusCode) => {
    switch (statusCode) {
      case 'NOT_FOUND':
        this.cancelTimer();
        return [styles.ragStatusAmber];
      case 'UPLOADING':
        return [styles.ragStatusAmber];
      case 'QUEUED':
        return [styles.ragStatusAmber];
      case 'PROCESSING_ARCHIVE_FILE':
        return [styles.ragStatusAmber];
      case 'RUNNING_PHASE_1':
        return [styles.ragStatusAmber];
      case 'RUNNING_PHASE_2':
        return [styles.ragStatusAmber];
      case 'RUNNING_PHASE_3':
        return [styles.ragStatusAmber];
      case 'COMPLETE':
        this.cancelTimer();
        return [styles.ragStatusGreen];
      case 'FAILED':
        this.cancelTimer();
        return [styles.ragStatusRed];
      default:
        return undefined;
    }
  };

  private fetchImportStatus() {
    const { dataFile, releaseId, onStatusChange } = this.props;

    // Do check to avoid an extra render on mount.
    const { isFetching } = this.state;
    if (!isFetching) {
      this.setState({ isFetching: true });
    }

    // NOTE: The intervalRef check is because the request may be in progress
    // when the timer is canceled. This prevents setState being called after
    // the component has unmounted.

    releaseDataFileService
      .getDataFileImportStatus(releaseId, dataFile.filename)
      .then(
        importStatus =>
          this.intervalId &&
          this.setState({
            current: importStatus,
            isFetching: false,
            running: 'UPLOADING,QUEUED,PROCESSING_ARCHIVE_FILE,RUNNING_PHASE_1, RUNNING_PHASE_2, RUNNING_PHASE_3'.match(
              importStatus.status,
            ),
          }),
      )
      .catch(
        () =>
          this.intervalId &&
          this.setState({
            current: null,
            isFetching: false,
            running: false,
          }),
      )
      .finally(() => {
        const { current } = this.state;
        const currentStatus: DataFileImportStatus = (current as unknown) as DataFileImportStatus;
        onStatusChange(dataFile, currentStatus.status);
      });
  }

  private initialiseTimer() {
    this.intervalId = setInterval(
      this.fetchImportStatus.bind(this),
      ImporterStatus.refreshPeriod,
    );
  }

  private cancelTimer() {
    if (this.intervalId) {
      clearInterval(this.intervalId);
    }
  }

  public render() {
    const { current, running } = this.state;
    const currentStatus: DataFileImportStatus = (current as unknown) as DataFileImportStatus;

    return (
      <SummaryListItem term="Status">
        <div>
          <div className={styles.currentStatusContainer}>
            <Tag
              className={classNames(
                'govuk-!-margin-right-1',
                currentStatus &&
                  this.getImportStatusClass(currentStatus.status),
              )}
              strong
            >
              {currentStatus && getImportStatusLabel(currentStatus.status)}
            </Tag>
            {running && (
              <LoadingSpinner
                alert
                hideText
                inline
                size="sm"
                text="Currently processing data"
              />
            )}
          </div>
          {currentStatus &&
            currentStatus.errors &&
            currentStatus.errors.length > 0 && (
              <Details className={styles.errorSummary} summary="See Errors">
                <ul>
                  {currentStatus.errors.map((error, index) => (
                    <li key={index.toString()}>{error}</li>
                  ))}
                </ul>
              </Details>
            )}
        </div>
      </SummaryListItem>
    );
  }
}

export default ImporterStatus;
