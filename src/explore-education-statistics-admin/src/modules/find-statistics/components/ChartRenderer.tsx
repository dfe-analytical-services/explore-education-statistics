import HorizontalBarBlock from '@common/modules/find-statistics/components/charts/HorizontalBarBlock';
import LineChartBlock from '@common/modules/find-statistics/components/charts/LineChartBlock';

import DynamicMapBlock, {
  MapFeature,
} from '@common/modules/find-statistics/components/charts/MapBlock';
import VerticalBarBlock from '@common/modules/find-statistics/components/charts/VerticalBarBlock';
import { Axis } from '@common/services/publicationService';
import {
  CharacteristicsData,
  IndicatorsMetaItem,
  PublicationMeta,
} from '@common/services/tableBuilderService';
import React, { Component } from 'react';

export interface ChartRendererProps {
  type: string;
  indicators: string[];
  data: CharacteristicsData;
  meta: PublicationMeta;
  xAxis?: Axis;
  yAxis?: Axis;
  height?: number;
  stacked?: boolean;
  geometry?: MapFeature;
}

export default class ChartRenderer extends Component<ChartRendererProps> {
  public render() {
    const {
      data,
      geometry,
      height,
      meta,
      indicators,
      stacked,
      type,
      xAxis = { title: '' },
      yAxis = { title: '' },
    } = this.props;

    const allIndicatorLabels = Array.prototype.concat(
      ...Object.values(meta.indicators),
    );

    const usedIndicatorLabels = indicators
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

    const characteristicsData: CharacteristicsData = data;

    // TODO : Temporary sort on the results to get them in date order
    characteristicsData.result.sort((a, b) => {
      if (a.timePeriod < b.timePeriod) {
        return -1;
      }

      if (a.timePeriod > b.timePeriod) {
        return 1;
      }

      return 0;
    });

    switch (type.toLowerCase()) {
      case 'line':
        return (
          <LineChartBlock
            chartDataKeys={indicators}
            characteristicsData={characteristicsData}
            labels={labels}
            xAxis={xAxis}
            yAxis={yAxis}
            height={height}
          />
        );
      case 'verticalbar':
        return (
          <VerticalBarBlock
            chartDataKeys={indicators}
            characteristicsData={characteristicsData}
            labels={labels}
            xAxis={xAxis}
            yAxis={yAxis}
            height={height}
          />
        );
      case 'horizontalbar':
        return (
          <HorizontalBarBlock
            chartDataKeys={indicators}
            characteristicsData={characteristicsData}
            labels={labels}
            xAxis={xAxis}
            yAxis={yAxis}
            height={height}
            stacked={stacked}
          />
        );
      case 'map':
        return (
          <DynamicMapBlock
            chartDataKeys={indicators}
            characteristicsData={characteristicsData}
            labels={labels}
            xAxis={xAxis}
            yAxis={yAxis}
            height={height}
            geometry={geometry}
          />
        );
      default:
        return <div>[ Unimplemented chart type requested ${type} ]</div>;
    }
  }
}
