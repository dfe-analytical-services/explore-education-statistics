import React, { Component } from 'react';
import Tabs from '../../../components/Tabs';
import TabsSection from '../../../components/TabsSection';
import { DataQuery } from './ContentBlock';
import { TableRenderer } from './TableRenderer';
import { ChartRenderer } from './ChartRenderer';

interface Chart {
  type: string;
  attributes: string[];
}

interface DataBlockProps {
  type: string;
  heading: string;
  dataQuery: DataQuery;
  charts: Chart[];
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

    await this.getData(dataQuery);
  }

  public async componentWillUnmount() {
    this.currentDataQuery = undefined;
  }

  private async getData(dataQuery: DataQuery) {
    if (this.currentDataQuery === dataQuery) return;

    this.currentDataQuery = dataQuery;

    const response = await fetch(`http://localhost:5001${dataQuery.path}`, {
      body: dataQuery.body,
      headers: {
        'Content-Type': 'application/json',
      },
      method: dataQuery.method,
    });

    const json = await response.json();

    const publicationId = json.publicationId;

    const metaResponse = await fetch(
      `http://localhost:5000/api/TableBuilder/meta/${publicationId}`,
    );

    const jsonMeta = await metaResponse.json();

    if (this.currentDataQuery === dataQuery) {
      this.parseDataResponse(json);

      this.setState({
        tables: [{ data: json, meta: jsonMeta }],
      });
    }
  }

  private parseDataResponse(json: any): void {
    return;
  }

  public render() {
    const { charts, dataQuery, heading } = this.props;

    const id = new Date().getDate();

    console.log(charts);

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
