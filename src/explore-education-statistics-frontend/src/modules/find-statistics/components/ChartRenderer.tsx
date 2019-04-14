import { HorizontalBarBlock } from '@common/modules/find-statistics/components/charts/HorizontalBarBlock';
import { LineChartBlock } from '@common/modules/find-statistics/components/charts/LineChartBlock';
import { MapFeature } from '@common/modules/find-statistics/components/charts/MapBlock';
import { VerticalBarBlock } from '@common/modules/find-statistics/components/charts/VerticalBarBlock';
import { Axis } from '@common/services/publicationService';
import {
  CharacteristicsData,
  IndicatorsMetaItem,
  PublicationMeta,
} from '@common/services/tableBuilderService';
import dynamic from 'next-server/dynamic';
import React, { Component } from 'react';

const DynamicMapBlock = dynamic(
  () => import('@common/modules/find-statistics/components/charts/MapBlock'),
  {
    ssr: false,
  },
);

export interface ChartRendererProps {
  type: string;
  indicators: string[];
  data: CharacteristicsData;
  meta: PublicationMeta;
  xAxis: Axis;
  yAxis: Axis;
  height?: number;
  stacked?: boolean;
  geometry: MapFeature;
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

    const labels = usedIndicatorLabels.reduce((obj, next) => {
      return {
        ...obj,
        [next.name]: next.label,
      };
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

    const xAxis: Axis = this.props.xAxis;
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
