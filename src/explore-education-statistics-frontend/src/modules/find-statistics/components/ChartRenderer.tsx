import React, { Component } from 'react';
import {
  CharacteristicsData,
  IndicatorsMetaItem,
  PublicationMeta,
} from '../../../services/tableBuilderService';
import { LineChartBlock } from './Charts/LineChartBlock';

interface ChartRendererProps {
  type: string;
  indicators: string[];

  data: any;
  meta: PublicationMeta;
}

export class ChartRenderer extends Component<ChartRendererProps> {
  public render() {
    const allIndicatorLabels = Object.values(this.props.meta.indicators).flat();

    const usedIndicatorLabels = this.props.indicators
      .map(indicatorName =>
        allIndicatorLabels.find(({ name }) => name === indicatorName),
      )
      .filter(_ => _ !== undefined) as IndicatorsMetaItem[]; // just in case

    const yAxisLabel = 'Absence Rate';
    const xAxisLabel = 'School Year';

    const labels = usedIndicatorLabels.reduce((obj: any, next) => {
      obj[next.name] = next.label;
      return obj;
    }, {});

    const characteristicsData: CharacteristicsData = this.props.data;

    // TODO : Temporary sort on the results to get them in date order
    characteristicsData.result.sort((a, b) => {
      return a.year < b.year ? -1 : a.year > b.year ? 1 : 0;
    });

    switch (this.props.type.toLowerCase()) {
      case 'line':
        return (
          <LineChartBlock
            chartDataKeys={this.props.indicators}
            characteristicsData={characteristicsData}
            labels={labels}
            xAxisLabel={xAxisLabel}
            yAxisLabel={yAxisLabel}
          />
        );
      default:
        return '';
    }
  }
}
