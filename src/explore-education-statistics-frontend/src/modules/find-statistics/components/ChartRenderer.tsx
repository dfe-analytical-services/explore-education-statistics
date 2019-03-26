import dynamic from 'next-server/dynamic';
import React, { Component } from 'react';
import { Axis } from '../../../services/publicationService';
import {
  CharacteristicsData,
  IndicatorsMetaItem,
  PublicationMeta,
} from '../../../services/tableBuilderService';
import { HorizontalBarBlock } from './Charts/HorizontalBarBlock';
import { LineChartBlock } from './Charts/LineChartBlock';
import { VerticalBarBlock } from './Charts/VerticalBarBlock';

const DynamicMapBlock = dynamic(() => import('./Charts/MapBlock'), {
  ssr: false,
});

interface ChartRendererProps {
  type: string;
  indicators: string[];

  data: any;
  meta: PublicationMeta;

  xAxis?: Axis;
  yAxis?: Axis;

  height?: number;
  [property: string]: any;
}

export class ChartRenderer extends Component<ChartRendererProps> {
  public render() {
    const allIndicatorLabels = Array.prototype.concat(
      ...Object.values(this.props.meta.indicators),
    );

    const usedIndicatorLabels = this.props.indicators
      .map(indicatorName =>
        allIndicatorLabels.find(({ name }) => name === indicatorName),
      )
      .filter(_ => _ !== undefined) as IndicatorsMetaItem[]; // just in case

    const labels = usedIndicatorLabels.reduce((obj: any, next) => {
      obj[next.name] = next.label;
      return obj;
    }, {});

    const characteristicsData: CharacteristicsData = this.props.data;

    // TODO : Temporary sort on the results to get them in date order
    characteristicsData.result.sort((a, b) => {
      return a.timePeriod < b.timePeriod
        ? -1
        : a.timePeriod > b.timePeriod
        ? 1
        : 0;
    });

    // @ts-ignore
    const xAxis: Axis = this.props.xAxis;
    // @ts-ignore
    const yAxis: Axis = this.props.yAxis;

    switch (this.props.type.toLowerCase()) {
      case 'line':
        return (
          <LineChartBlock
            chartDataKeys={this.props.indicators}
            characteristicsData={characteristicsData}
            labels={labels}
            xAxis={xAxis}
            yAxis={yAxis}
            height={this.props.height}
          />
        );
      case 'verticalbar':
        return (
          <VerticalBarBlock
            chartDataKeys={this.props.indicators}
            characteristicsData={characteristicsData}
            labels={labels}
            xAxis={xAxis}
            yAxis={yAxis}
            height={this.props.height}
          />
        );
      case 'horizontalbar':
        return (
          <HorizontalBarBlock
            chartDataKeys={this.props.indicators}
            characteristicsData={characteristicsData}
            labels={labels}
            xAxis={xAxis}
            yAxis={yAxis}
            height={this.props.height}
            stacked={this.props.stacked}
          />
        );
      case 'map':
        return (
          <DynamicMapBlock
            chartDataKeys={this.props.indicators}
            characteristicsData={characteristicsData}
            labels={labels}
            xAxis={xAxis}
            yAxis={yAxis}
            height={this.props.height}
            geometry={this.props.geometry}
          />
        );
      default:
        return (
          <div>[ Unimplemented chart type requested ${this.props.type} ]</div>
        );
    }
  }
}
