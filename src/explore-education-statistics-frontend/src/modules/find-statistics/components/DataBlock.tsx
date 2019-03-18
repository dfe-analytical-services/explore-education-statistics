import React, { Component } from 'react';
import Tabs from '../../../components/Tabs';
import TabsSection from '../../../components/TabsSection';
import { Chart, DataQuery } from '../../../services/publicationService';
import { PublicationMeta } from '../../../services/tableBuilderService';
import { ChartRenderer } from './ChartRenderer';
import { TableRenderer } from './TableRenderer';

interface DataBlockProps {
  type: string;
  heading?: string;
  dataQuery?: DataQuery;
  charts?: Chart[];
}

interface DataBlockState {
  charts?: any[];
  downloads?: any[];
  tables?: any[];
}

export class DataBlock extends Component<DataBlockProps, DataBlockState> {
  public state: DataBlockState = {
    charts: undefined,
    downloads: undefined,
    tables: undefined,
  };

  private currentDataQuery?: DataQuery = undefined;

  public async componentDidMount() {
    const { dataQuery } = this.props;

    if (dataQuery) await this.getData(dataQuery);
  }

  public async componentWillUnmount() {
    this.currentDataQuery = undefined;
  }

  private async getData(dataQuery: DataQuery) {
    if (this.currentDataQuery === dataQuery) return;

    this.currentDataQuery = dataQuery;

    const response = await fetch(
      `${process.env.DATA_API_BASE_URL}${dataQuery.path}`,
      {
        body: dataQuery.body,
        headers: {
          'Content-Type': 'application/json',
        },
        method: dataQuery.method,
      },
    );

    const json = await response.json();

    const publicationId = json.publicationId;

    const metaResponse = await fetch(
      `${
        process.env.DATA_API_BASE_URL
      }/api/TableBuilder/meta/CharacteristicDataNational/${publicationId}`,
    );

    const jsonMeta: PublicationMeta = await metaResponse.json();

    if (this.currentDataQuery === dataQuery) {
      this.parseDataResponse(json);

      const newState: any = {
        tables: [{ data: json, meta: jsonMeta }],
      };

      if (this.props.charts) {
        newState.charts = this.props.charts.map(chart => ({
          ...chart,
          data: json,
          meta: jsonMeta,
        }));
      }

      this.setState(newState);
    }
  }

  private parseDataResponse(json: any): void {
    return;
  }

  public render() {
    const { charts, dataQuery, heading } = this.props;

    const id = new Date().getDate();

    return (
      <div className="govuk-datablock">
        <Tabs>
          <TabsSection id={`${id}0`} title="Data tables">
            {this.state.tables !== undefined
              ? this.state.tables.map((table: any, idx) => (
                  <TableRenderer key={`${id}0_table_${idx}`} {...table} />
                ))
              : ''}
          </TabsSection>
          <TabsSection id={`${id}1`} title="Charts">
            {this.state.charts !== undefined
              ? this.state.charts.map((chart: any, idx) => (
                  <ChartRenderer key={`${id}_chart_${idx}`} {...chart} />
                ))
              : ''}
          </TabsSection>
          <TabsSection id={`${id}2`} title="Data downloads">
            downloads
          </TabsSection>
        </Tabs>
      </div>
    );
  }
}
