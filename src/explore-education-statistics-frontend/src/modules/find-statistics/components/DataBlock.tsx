import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import { MapFeature } from '@common/modules/find-statistics/components/charts/MapBlock';
import {
  SummaryRenderer,
  SummaryRendererProps,
} from '@common/modules/find-statistics/components/SummaryRenderer';
import {
  TableRenderer,
  TableRendererProps,
} from '@common/modules/find-statistics/components/TableRenderer';
import { baseUrl } from '@common/services/api';
import { Chart, DataQuery, Summary } from '@common/services/publicationService';
import {
  CharacteristicsData,
  PublicationMeta,
} from '@common/services/tableBuilderService';
import Link from '@frontend/components/Link';
import React, { Component } from 'react';
import { ChartRenderer, ChartRendererProps } from './ChartRenderer';

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
    const { dataQuery } = this.props;

    if (dataQuery) {
      await this.getData(dataQuery);
    } else {
      this.parseDataResponse(this.props.data, this.props.meta);
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

    const publicationId = json.publicationId;

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

      if (this.props.charts) {
        newState.charts = this.props.charts.map(chart => ({
          ...chart,
          geometry: chart.geometry as MapFeature,
          data: json,
          meta: jsonMeta,
        }));
      }

      if (this.props.summary) {
        newState.summary = {
          ...this.props.summary,
          data: json,
          meta: jsonMeta,
        };
      }
    }

    this.setState(newState);
  }

  public render() {
    const id = new Date().getDate();

    return (
      <div
        className="govuk-datablock"
        data-testid={`DataBlock ${this.props.heading}`}
      >
        <Tabs>
          {this.state.summary && (
            <TabsSection id={`${id}_summary`} title="Summary">
              <h3>{this.props.heading}</h3>
              <SummaryRenderer {...this.state.summary} />
            </TabsSection>
          )}

          {this.state.tables && (
            <TabsSection id={`${id}0`} title="Data tables">
              <h3>{this.props.heading}</h3>
              {this.state.tables.map((table, idx) => (
                <TableRenderer key={`${id}0_table_${idx}`} {...table} />
              ))}
              <h2 className="govuk-heading-m govuk-!-margin-top-9">
                Explore and edit this data online
              </h2>
              <p>Use our table tool to add and remove data for this table.</p>
              <Link to={`/table-tool/`} className="govuk-button">
                Explore data
              </Link>
            </TabsSection>
          )}

          {this.state.charts && (
            <TabsSection id={`${id}1`} title="Charts" lazy={true}>
              <h3>{this.props.heading}</h3>
              {this.state.charts.map((chart, idx) => (
                <ChartRenderer
                  key={`${id}_chart_${idx}`}
                  {...chart}
                  height={this.props.height}
                />
              ))}
              <h2 className="govuk-heading-m govuk-!-margin-top-9">
                Explore and edit this data online
              </h2>
              <p>Use our table tool to add and remove data for this chart.</p>
              <Link to={`/table-tool/`} className="govuk-button">
                Explore chart data
              </Link>
            </TabsSection>
          )}

          <TabsSection id={`${id}2`} title="Data downloads">
            <p>
              You can customise and download data as Excel or .csv files. Our
              data can also be accessed via an API.
            </p>
            <div className="govuk-inset-text">
              Data downloads have not yet been implemented within the service.
            </div>
            <h2 className="govuk-heading-m govuk-!-margin-top-9">
              Explore and edit this data online
            </h2>
            <p>Use our table tool to explore this data.</p>
            <Link to={`/table-tool/`} className="govuk-button">
              Explore data
            </Link>
          </TabsSection>
        </Tabs>
      </div>
    );
  }
}
