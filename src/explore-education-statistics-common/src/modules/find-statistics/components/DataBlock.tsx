/* eslint-disable */
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import { MapFeature } from '@common/modules/find-statistics/components/charts/MapBlock';
import SummaryRenderer, {
  SummaryRendererProps,
} from '@common/modules/find-statistics/components/SummaryRenderer';
import TableRenderer, {
  Props as TableRendererProps,
} from '@common/modules/find-statistics/components/TableRenderer';
import {
  Chart,
  DataQuery,
  Summary,
  Table,
} from '@common/services/publicationService';

import React, { Component, ReactNode } from 'react';
import DataBlockService, {
  DataBlockRequest,
  DataBlockMetadata,
  DataBlockData,
} from '@common/services/dataBlockService';
import ChartRenderer, {
  ChartRendererProps,
} from '@common/modules/find-statistics/components/ChartRenderer';

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
}

interface DataBlockState {
  charts?: ChartRendererProps[];
  // downloads?: any[];
  tables?: TableRendererProps[];
  summary?: SummaryRendererProps;
}

class DataBlock extends Component<DataBlockProps, DataBlockState> {
  public static defaultProps = {
    showTables: true,
  };

  public state: DataBlockState = {};

  private currentDataQuery?: DataQuery = undefined;

  public async componentDidMount() {
    const { dataBlockRequest } = this.props;

    if (dataBlockRequest) {
      const result = await DataBlockService.getDataBlockForSubject(
        dataBlockRequest,
      );

      this.parseDataResponse(result.data, result.metaData);
    } else {
      // this.parseDataResponse(data, meta);
    }
  }

  public async componentWillUnmount() {
    this.currentDataQuery = undefined;
  }

  private parseDataResponse(
    json: DataBlockData,
    jsonMeta: DataBlockMetadata,
  ): void {
    const newState: DataBlockState = {};

    const { charts, summary, tables } = this.props;

    if (json.result.length > 0) {
      if (tables) {
        newState.tables = [{ data: json, meta: jsonMeta, ...tables[0] }];
      } else {
        //TODO: remove when data is updated
        newState.tables = [
          {
            data: json,
            meta: jsonMeta,
            indicators: Object.keys(json.result[0].measures),
          },
        ];
      }
    }

    if (charts) {
      newState.charts = charts.map(chart => ({
        xAxis: { title: '' },
        yAxis: { title: '' },

        ...chart,

        data: json,
        meta: jsonMeta,
      }));
    }

    if (summary) {
      newState.summary = {
        ...summary,
        data: json,
        meta: jsonMeta,
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
    } = this.props;
    const { charts, summary, tables } = this.state;

    return (
      <div className="govuk-datablock" data-testid={`DataBlock ${heading}`}>
        <Tabs>
          {summary && (
            <TabsSection id={`datablock_${id}_summary`} title="Summary">
              <h3>{heading}</h3>
              <SummaryRenderer {...summary} />
            </TabsSection>
          )}

          {tables && showTables && (
            <TabsSection id={`datablock_${id}_tables`} title="Data tables">
              <h3>{heading}</h3>
              {tables.map((table, idx) => {
                const key = `${id}0_table_${idx}`;

                return <TableRenderer key={key} {...table} />;
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
              <h3>{heading}</h3>
              {charts.map((chart, idx) => {
                const key = `${id}_chart_${idx}`;

                return <ChartRenderer key={key} {...chart} height={height} />;
              })}
              {additionalTabContent}
            </TabsSection>
          )}

          <TabsSection id={`datablock_${id}_downloads`} title="Data downloads">
            <p>
              You can customise and download data as Excel, .csv or .pdf files.
              Our data can also be accessed via an API.
            </p>
            <div className="govuk-inset-text">
              Data downloads have not yet been implemented within the service.
            </div>
            {additionalTabContent}
          </TabsSection>
        </Tabs>
      </div>
    );
  }
}

export default DataBlock;
