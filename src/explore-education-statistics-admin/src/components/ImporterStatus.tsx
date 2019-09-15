import React, { Component } from 'react';
import importStatusService from '@admin/services/release/imports/service';
import {ImportStatus, ImportStatusCode} from '@admin/services/release/imports/types';
import classNames from 'classnames';
import styles from '@admin/pages/release/edit-release/data/ReleaseDataUploadsSection.module.scss';
import SummaryListItem from '@common/components/SummaryListItem';

interface State {
  isFetching: boolean;
  current?: ImportStatus | undefined;
  errorMessage: string;
}

interface Props {
  releaseId: string;
  datafileName: string;
}

export const getImportStatusLabel = (importstatusCode: ImportStatusCode) => {
    switch (importstatusCode) {
        case "NOT_FOUND":
            return 'Queued';
        case 'RUNNING_PHASE_1':
            return 'Validating';
        case 'RUNNING_PHASE_2':
            return 'Importing...';
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

  private intervalId?: any;

  public componentDidMount() {
    this.fetchImportStatus();
    this.initialiseTimer();
  }

  public componentWillUnmount() {
    this.cancelTimer();
  }

  private fetchImportStatus() {
    const { datafileName, releaseId } = this.props;

    // Do check to avoid an extra render on mount.
    const { isFetching } = this.state;
    if (!isFetching) {
      this.setState({ isFetching: true });
    }

    // NOTE: The intervalRef check is because the request may be in progress
    // when the timer is canceled. This prevents setState being called after
    // the component has unmounted.

    importStatusService
      .getImportStatus(releaseId, datafileName)
      .then(
        importStatus =>
          this.intervalId &&
          this.setState({
            current: importStatus,
            isFetching: false,
          }),
      )
      .catch(
        error =>
          this.intervalId &&
          this.setState({
            current: null,
            isFetching: false,
          }),
      );
  }

  private initialiseTimer() {
    this.intervalId = setInterval(this.fetchImportStatus.bind(this), 10000);
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
        <strong className={classNames('govuk-tag', [styles.ragStatusRed])}>
          Failed
        </strong>
        <strong className={classNames('govuk-tag', [styles.ragStatusAmber])}>
          {currentStatus && getImportStatusLabel(currentStatus.status)}
        </strong>
        <strong className={classNames('govuk-tag', [styles.ragStatusGreen])}>
          Complete
        </strong>
      </SummaryListItem>
    );
  }
}

export default ImporterStatus;
