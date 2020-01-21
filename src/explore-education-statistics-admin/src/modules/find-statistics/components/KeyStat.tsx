import KeyStatTile from '@admin/modules/find-statistics/components/EditableKeyStatTile';
import LoadingSpinner from '@common/components/LoadingSpinner';
import { parseMetaData } from '@common/modules/find-statistics/components/charts/ChartFunctions';
import { DataBlockProps } from '@common/modules/find-statistics/components/DataBlock';
import {
  getLatestMeasures,
  SummaryRendererProps,
} from '@common/modules/find-statistics/components/SummaryRenderer';
import DataBlockService, {
  DataBlockData,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import React, { Component } from 'react';

interface KeyStatState {
  isLoading: boolean;
  isError: boolean;
  summary?: SummaryRendererProps;
}

class KeyStat extends Component<DataBlockProps, KeyStatState> {
  public static defaultProps = {
    showTables: true,
  };

  public state: KeyStatState = {
    isLoading: true,
    isError: false,
  };

  public async componentDidMount() {
    const { dataBlockRequest } = this.props;

    if (dataBlockRequest) {
      const result = await DataBlockService.getDataBlockForSubject(
        dataBlockRequest,
      );

      if (result) {
        this.parseDataResponse(result);
      } else {
        this.setState({
          isError: true,
          isLoading: false,
        });
      }
    } else {
      const { dataBlockResponse } = this.props;
      if (dataBlockResponse) {
        this.parseDataResponse(dataBlockResponse);
      }
    }
  }

  private parseDataResponse(response: DataBlockResponse): void {
    const newState: KeyStatState = { isLoading: false, isError: false };

    const data: DataBlockData = response;
    const chartMetadata = parseMetaData(response.metaData);

    if (chartMetadata === undefined) return;

    const { summary } = this.props;

    if (summary) {
      newState.summary = {
        ...summary,
        data,
        meta: chartMetadata,
      };
    }
    this.setState(newState);
  }

  public render() {
    const { onSummaryDetailsToggle } = this.props;
    const { summary, isLoading, isError } = this.state;
    return isLoading ? (
      <LoadingSpinner text="Loading content..." />
    ) : (
      <KeyStatTile
        {...summary}
        measures={getLatestMeasures(summary.data.result)}
        indicatorKey={summary.dataKeys[0]}
        onToggle={onSummaryDetailsToggle}
        dataSummary={summary.dataSummary[0]}
        dataDefinition={summary.dataDefinition[0]}
      />
    );
  }
}

export default KeyStat;
