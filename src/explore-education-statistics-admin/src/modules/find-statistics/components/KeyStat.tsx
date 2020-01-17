import LoadingSpinner from '@common/components/LoadingSpinner';
import { ChartRendererProps } from '@common/modules/find-statistics/components/ChartRenderer';
import SummaryRenderer, {
  SummaryRendererProps,
  getLatestMeasures,
} from '@common/modules/find-statistics/components/SummaryRenderer';
import { Props as TableRendererProps } from '@common/modules/find-statistics/components/TimePeriodDataTableRenderer';
import DataBlockService, {
  DataBlockData,
  DataBlockRequest,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import { Chart, Summary, Table } from '@common/services/publicationService';
import React, { Component, MouseEvent, ReactNode } from 'react';
import { parseMetaData } from '@common/modules/find-statistics/components/charts/ChartFunctions';
import KeyStatTile from '@common/modules/find-statistics/components/KeyStatTile';

export interface DataBlockProps {
  id: string;
  type: string;
  heading?: string;
  dataBlockRequest?: DataBlockRequest;
  charts?: Chart[];
  tables?: Table[];

  summary?: Summary;

  height?: number;
  showTables?: boolean;
  additionalTabContent?: ReactNode;

  onToggle?: (section: { id: string; title: string }) => void;

  dataBlockResponse?: DataBlockResponse;

  onSummaryDetailsToggle?: (
    isOpened: boolean,
    event: MouseEvent<HTMLElement>,
  ) => void;
}

interface DataBlockState {
  isLoading: boolean;
  isError: boolean;
  charts?: ChartRendererProps[];
  tables?: TableRendererProps[];
  summary?: SummaryRendererProps;
}

class KeyStat extends Component<DataBlockProps, DataBlockState> {
  public static defaultProps = {
    showTables: true,
  };

  public state: DataBlockState = {
    isLoading: false,
    isError: false,
  };

  public async componentDidMount() {
    const { dataBlockRequest } = this.props;

    this.setState({ isLoading: true, isError: false });

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
    const newState: DataBlockState = { isLoading: false, isError: false };

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
      (!isError && summary && (
        <>
          <KeyStatTile
            {...summary}
            measures={getLatestMeasures(summary.data.result)}
            indicatorKey={summary.dataKeys[0]}
            onToggle={onSummaryDetailsToggle}
            dataSummary={summary.dataSummary[0]}
            dataDefinition={summary.dataDefinition[0]}
          />
        </>
      )) ||
        null
    );
  }
}

export default KeyStat;
