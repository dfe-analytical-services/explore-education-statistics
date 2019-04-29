import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import { MapFeature } from '@common/modules/find-statistics/components/charts/MapBlock';
import SummaryRenderer, {
  SummaryRendererProps,
} from '@common/modules/find-statistics/components/SummaryRenderer';
import TableRenderer, {
  TableRendererProps,
} from '@common/modules/find-statistics/components/TableRenderer';
import { baseUrl } from '@common/services/api';
import { Chart, DataQuery, Summary } from '@common/services/publicationService';
import {
  CharacteristicsData,
  PublicationMeta,
} from '@common/services/tableBuilderService';
import React, { Component } from 'react';
import ChartRenderer, { ChartRendererProps } from './ChartRenderer';

export interface DataBlockProps {
  type: string;
  heading?: string;
  dataQuery?: DataQuery;
  charts?: Chart[];
  summary?: Summary;
  data?: CharacteristicsData;
  meta?: PublicationMeta;
  height?: number;
}

interface DataBlockState {
  charts?: ChartRendererProps[];
  // downloads?: any[];
  tables?: TableRendererProps[];
  summary?: SummaryRendererProps;
}

export class DataBlock extends Component<DataBlockProps, DataBlockState> {
  public state: DataBlockState = {};

  private currentDataQuery?: DataQuery = undefined;

  public async componentDidMount() {
    const { dataQuery, data, meta } = this.props;

    if (dataQuery) {
      await this.getData(dataQuery);
    } else {
      this.parseDataResponse(data, meta);
    }
  }

  public async componentWillUnmount() {
    this.currentDataQuery = undefined;
  }

  private async getData(dataQuery: DataQuery) {
    if (this.currentDataQuery === dataQuery) return;

    this.currentDataQuery = dataQuery;

    const response = await fetch(`${baseUrl.data}${dataQuery.path}`, {
      body: dataQuery.body,
      headers: {
        'Content-Type': 'application/json',
      },
      method: dataQuery.method,
    });

    const json: CharacteristicsData = await response.json();

    const { publicationId } = json;

    const metaResponse = await fetch(
      `${
        baseUrl.data
      }/api/TableBuilder/meta/CharacteristicDataNational/${publicationId}`,
    );

    const jsonMeta: PublicationMeta = await metaResponse.json();

    if (this.currentDataQuery === dataQuery) {
      this.parseDataResponse(json, jsonMeta);
    }
  }

  private parseDataResponse(
    json?: CharacteristicsData,
    jsonMeta?: PublicationMeta,
  ): void {
    const newState: DataBlockState = {};

    if (json && jsonMeta) {
      if (json.result.length > 0) {
        newState.tables = [{ data: json, meta: jsonMeta }];
      }

      const { charts, summary } = this.props;

      if (charts) {
        newState.charts = charts.map((chart: Chart) => ({
          ...chart,
          geometry: chart.geometry as MapFeature,
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
    }

    this.setState(newState);
  }

  public render() {
    const id = new Date().getDate();

    const { heading, height } = this.props;
    const { charts, summary, tables } = this.state;

    return (
      <div className="govuk-datablock">
        <Tabs>
          {summary && (
            <TabsSection id={`${id}_summary`} title="Summary">
              <h3>{heading}</h3>
              <SummaryRenderer {...summary} />
            </TabsSection>
          )}

          {tables && (
            <TabsSection id={`${id}0`} title="Data tables">
              <h3>{heading}</h3>
              {tables.map((table, idx) => {
                const key = `${id}0_table_${idx}`;

                return <TableRenderer key={key} {...table} />;
              })}
            </TabsSection>
          )}

          {charts && (
            <TabsSection id={`${id}1`} title="Charts" lazy={false}>
              <h3>{heading}</h3>
              {charts.map((chart, idx) => {
                const key = `${id}_chart_${idx}`;
                return <ChartRenderer key={key} {...chart} height={height} />;
              })}
            </TabsSection>
          )}

          <TabsSection id={`${id}2`} title="Data downloads">
            downloads
          </TabsSection>
        </Tabs>
      </div>
    );
  }
}
