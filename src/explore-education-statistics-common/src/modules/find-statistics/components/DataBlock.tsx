import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import ChartRenderer, {
  ChartRendererProps,
} from '@common/modules/find-statistics/components/ChartRenderer';
import SummaryRenderer, {
  SummaryRendererProps,
} from '@common/modules/find-statistics/components/SummaryRenderer';
import TableRenderer, {
  Props as TableRendererProps,
} from '@common/modules/find-statistics/components/TableRenderer';
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
import React, { Component, ReactNode } from 'react';

import DataSource from '@common/modules/find-statistics/components/DataSource';
import DownloadDetails from './DownloadDetails';

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
  refreshCallback?: (callback: () => void) => void;
}

interface DataBlockState {
  isLoading: boolean;
  charts?: ChartRendererProps[];
  tables?: TableRendererProps[];
  summary?: SummaryRendererProps;
}

class DataBlock extends Component<DataBlockProps, DataBlockState> {
  public static defaultProps = {
    showTables: true,
  };

  public state: DataBlockState = {
    isLoading: false,
  };

  private query?: DataQuery = undefined;

  public async componentDidMount() {
    const { dataBlockRequest } = this.props;

    this.setState({ isLoading: true });

    if (dataBlockRequest) {
      const result = await DataBlockService.getDataBlockForSubject(
        dataBlockRequest,
      );

      this.parseDataResponse(result);
    } else {
      // this.parseDataResponse(data, meta);
    }
  }

  public async componentWillUnmount() {
    this.query = undefined;
  }

  // eslint-disable-next-line class-methods-use-this

  private parseDataResponse(response: DataBlockResponse): void {
    const newState: DataBlockState = { isLoading: false };

    const data: DataBlockData = response;
    const meta = response.metaData;

    const { charts, summary, tables } = this.props;

    if (response.result.length > 0) {
      if (tables) {
        newState.tables = [
          {
            data,
            meta,
            ...tables[0],
          },
        ];
      }
    }

    if (charts) {
      newState.charts = charts.map(chart => ({
        xAxis: { title: '' },
        yAxis: { title: '' },

        ...chart,

        data,
        meta,
      }));
    }

    if (summary) {
      newState.summary = {
        ...summary,
        data,
        meta,
      };
    }
    this.setState(newState);
  }

  public render() {
    const {
      heading = '',
      height,
      showTables,
      additionalTabContent,
      id,
      refreshCallback,
    } = this.props;
    const { charts, summary, tables, isLoading } = this.state;

    return (
      <div data-testid={`DataBlock ${heading}`}>
        {heading && <h3>{heading}</h3>}

        {isLoading ? (
          <LoadingSpinner text="Loading content..." />
        ) : (
          <Tabs>
            {summary && (
              <TabsSection id={`datablock_${id}_summary`} title="Summary">
                <SummaryRenderer {...summary} />
              </TabsSection>
            )}

            {tables && showTables && (
              <TabsSection id={`datablock_${id}_tables`} title="Data tables">
                {tables.map((table, idx) => {
                  const key = `${id}0_table_${idx}`;

                  return (
                    <React.Fragment key={key}>
                      <TableRenderer {...table} />
                      <DataSource />
                      <DownloadDetails />
                    </React.Fragment>
                  );
                })}

                {additionalTabContent}
              </TabsSection>
            )}

            {charts && (
              <TabsSection
                id={`datablock_${id}_charts`}
                title="Charts"
                lazy={false}
              >
                {charts.map((chart, idx) => {
                  const key = `${id}_chart_${idx}`;

                  return (
                    <React.Fragment key={key}>
                      <ChartRenderer
                        {...chart}
                        height={height}
                        refreshCallback={refreshCallback}
                      />
                      <DataSource />
                      <DownloadDetails />
                    </React.Fragment>
                  );
                })}

                {additionalTabContent}
              </TabsSection>
            )}
          </Tabs>
        )}
      </div>
    );
  }
}

export default DataBlock;
