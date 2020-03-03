import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import ChartRenderer, {
  ChartRendererProps,
} from '@common/modules/find-statistics/components/ChartRenderer';
import { parseMetaData } from '@common/modules/find-statistics/components/charts/ChartFunctions';
import TimePeriodDataTableRenderer, {
  Props as TableRendererProps,
} from '@common/modules/find-statistics/components/TimePeriodDataTableRenderer';
import DataBlockService, {
  DataBlockData,
  DataBlockRequest,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import {
  Chart,
  DataQuery,
  Summary,
  Table,
} from '@common/services/publicationService';
import React, { Component, ReactNode, MouseEvent } from 'react';

export interface DataBlockProps {
  id: string;
  type: string;
  captionTitle?: string;
  dataBlockRequest?: DataBlockRequest;

  tables?: Table[];
  charts?: Chart[];
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
  dataBlockResponse?: DataBlockResponse;
}

class DataBlock extends Component<DataBlockProps, DataBlockState> {
  public static defaultProps = {
    showTables: true,
  };

  public state: DataBlockState = {
    isLoading: false,
    isError: false,
  };

  private query?: DataQuery = undefined;

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

  public componentDidUpdate(prevProps: DataBlockProps) {
    const { id: datablockId } = this.props;
    if (prevProps.id !== datablockId) this.componentDidMount();
  }

  public async componentWillUnmount() {
    this.query = undefined;
  }

  private parseDataResponse(response: DataBlockResponse): void {
    const newState: DataBlockState = {
      isLoading: false,
      isError: false,
      dataBlockResponse: response,
    };

    const data: DataBlockData = response;
    const chartMetadata = parseMetaData(response.metaData);

    if (chartMetadata === undefined) return;

    const { charts, tables, captionTitle } = this.props;

    if (response.result.length > 0) {
      if (tables) {
        newState.tables = [
          {
            captionTitle,
            response,
            ...tables[0], /// at present only one chart
          },
        ];
      }
    }

    if (charts) {
      newState.charts = charts.map<ChartRendererProps>(chart => {
        // There is a presumption that the configuration from the API is valid.
        // The data coming from the API is required to be optional for the ChartRenderer
        // But the data for the charts is required. The charts have validation that
        // prevent them from attempting to render.
        // @ts-ignore
        const rendererProps: ChartRendererProps = {
          data,
          meta: chartMetadata,
          ...chart,
        };

        return rendererProps;
      });
    }

    this.setState(newState);
  }

  public render() {
    const {
      captionTitle,
      height,
      showTables,
      additionalTabContent,
      onToggle,
      id,
    } = this.props;
    const { charts, tables, isLoading, isError } = this.state;
    return (
      <>
        {isLoading ? (
          <LoadingSpinner text="Loading content..." />
        ) : (
          <Tabs id={id} onToggle={onToggle}>
            {isError && (
              <TabsSection id={`${id}-error`} title="Error">
                An error occurred while loading the data, please try again later
              </TabsSection>
            )}

            {tables && showTables && (
              <TabsSection id={`${id}-tables`} title="Table">
                {tables.map((table, idx) => {
                  const key = `${id}0_table_${idx}`;

                  return (
                    <TimePeriodDataTableRenderer
                      {...table}
                      captionTitle={captionTitle}
                      key={key}
                    />
                  );
                })}

                {additionalTabContent}
              </TabsSection>
            )}

            {charts && charts.length > 0 && (
              <TabsSection
                datablockId={id}
                id={`${id}-charts`}
                title="Charts"
                lazy={false}
              >
                {charts.map((chart, idx) => {
                  const key = `${id}_chart_${idx}`;

                  return (
                    <React.Fragment key={key}>
                      {chart.data &&
                      chart.meta &&
                      chart.data.result.length > 0 ? (
                        <ChartRenderer {...chart} height={height} />
                      ) : (
                        <div>
                          Unable to render chart, invalid data configured
                        </div>
                      )}
                    </React.Fragment>
                  );
                })}

                {additionalTabContent}
              </TabsSection>
            )}
          </Tabs>
        )}
      </>
    );
  }
}

export default DataBlock;
