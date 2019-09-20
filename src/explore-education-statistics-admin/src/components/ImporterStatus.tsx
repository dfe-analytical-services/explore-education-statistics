import React, { Component } from 'react';
import importStatusService from '@admin/services/release/imports/service';
import {
  ImportStatus,
  ImportStatusCode,
} from '@admin/services/release/imports/types';
import classNames from 'classnames';
import styles from '@admin/pages/release/edit-release/data/ReleaseDataUploadsSection.module.scss';
import SummaryListItem from '@common/components/SummaryListItem';
import { DataFile } from '@admin/services/release/edit-release/data/types';

interface State {
  isFetching: boolean;
  current?: ImportStatus | undefined;
  errorMessage: string;
}

interface Props {
  releaseId: string;
  dataFile: DataFile;
  onStatusChangeHandler: (datafile: DataFile, status: ImportStatusCode) => void;
}

export const getImportStatusLabel = (importstatusCode: ImportStatusCode) => {
  switch (importstatusCode) {
    case 'NOT_FOUND':
      return 'Queued';
    case 'RUNNING_PHASE_1':
      return 'Validating';
    case 'RUNNING_PHASE_2':
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
  public state = {
    isFetching: true,
    current: undefined,
  };

  private intervalId?: NodeJS.Timeout;

  public componentDidMount() {
    this.fetchImportStatus();
    this.initialiseTimer();
  }

  public componentWillUnmount() {
    this.cancelTimer();
  }

  private static refreshPeriod = 5000;

  private getImportStatusClass = (importstatusCode: ImportStatusCode) => {
    switch (importstatusCode) {
      case 'NOT_FOUND':
        return [styles.ragStatusAmber];
      case 'RUNNING_PHASE_1':
        return [styles.ragStatusAmber];
      case 'RUNNING_PHASE_2':
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
          }),
      )
      .catch(
        () =>
          this.intervalId &&
          this.setState({
            current: null,
            isFetching: false,
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
    const { current } = this.state;
    const currentStatus: ImportStatus = (current as unknown) as ImportStatus;

    return (
      <SummaryListItem term="Status">
        <strong
          className={classNames(
            'govuk-tag',
            currentStatus && this.getImportStatusClass(currentStatus.status),
          )}
        >
          {currentStatus && getImportStatusLabel(currentStatus.status)}
        </strong>
      </SummaryListItem>
    );
  }
}

export default ImporterStatus;
