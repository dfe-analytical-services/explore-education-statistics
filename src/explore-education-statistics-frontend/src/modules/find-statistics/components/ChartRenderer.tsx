import React, { Component } from 'react';
import {
  AttributesMetaItem,
  CharacteristicsData,
  PublicationMeta,
} from '../../../services/tableBuilderService';
import { LineChartBlock } from './Charts/LineChartBlock';

interface ChartRendererProps {
  type: string;
  attributes: string[];

  data: any;
  meta: PublicationMeta;
}

export class ChartRenderer extends Component<ChartRendererProps> {
  public render() {
    const allAttributeLabels = Object.values(this.props.meta.attributes).flat();

    const usedAttributeLabels = this.props.attributes
      .map(attributeName =>
        allAttributeLabels.find(({ name }) => name === attributeName),
      )
      .filter(_ => _ !== undefined) as AttributesMetaItem[]; // just in case

    const yAxisLabel = 'Absence Rate';
    const xAxisLabel = 'School Year';

    const labels = usedAttributeLabels.reduce((obj: any, next) => {
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
            chartDataKeys={this.props.attributes}
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
