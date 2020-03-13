import styles from '@admin/pages/release/edit-release/data/ReleaseDataUploadsSection.module.scss';
import { DataFile } from '@admin/services/release/edit-release/data/editReleaseDataService';
import importStatusService from '@admin/services/release/imports/service';
import {
  ImportStatus,
  ImportStatusCode,
} from '@admin/services/release/imports/types';
import Details from '@common/components/Details';
import LoadingSpinner from '@common/components/LoadingSpinner';
import SummaryListItem from '@common/components/SummaryListItem';
import Tag from '@common/components/Tag';
import classNames from 'classnames';
import React, { Component } from 'react';

interface Props {
  releaseId: string;
  dataFile: DataFile;
  onStatusChangeHandler: (datafile: DataFile, status: ImportStatusCode) => void;
}

export const getImportStatusLabel = (importstatusCode: ImportStatusCode) => {
  switch (importstatusCode) {
    case 'NOT_FOUND':
      return 'Not Found';
    case 'QUEUED':
      return 'Queued';
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

class ImporterStatus extends Component<Props> {
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

  private getImportStatusClass = (importstatusCode: ImportStatusCode) => {
    switch (importstatusCode) {
      case 'NOT_FOUND':
        return [styles.ragStatusAmber];
      case 'QUEUED':
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
    const { dataFile, releaseId, onStatusChangeHandler } = this.props;

    // Do check to avoid an extra render on mount.
    const { isFetching } = this.state;
    if (!isFetching) {
      this.setState({ isFetching: true });
    }

    // NOTE: The intervalRef check is because the request may be in progress
    // when the timer is canceled. This prevents setState being called after
    // the component has unmounted.

    importStatusService
      .getImportStatus(releaseId, dataFile.filename)
      .then(
        importStatus =>
          this.intervalId &&
          this.setState({
            current: importStatus,
            isFetching: false,
            running: 'NOT_FOUND,QUEUED,RUNNING_PHASE_1, RUNNING_PHASE_2, RUNNING_PHASE_3'.match(
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
        const currentStatus: ImportStatus = (current as unknown) as ImportStatus;
        onStatusChangeHandler(dataFile, currentStatus.status);
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
    const currentStatus: ImportStatus = (current as unknown) as ImportStatus;

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
