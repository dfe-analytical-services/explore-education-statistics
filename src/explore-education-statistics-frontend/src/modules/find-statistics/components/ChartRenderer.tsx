import React, { Component } from 'react';
import { Axis } from '../../../services/publicationService';
import {
  AttributesMetaItem,
  CharacteristicsData,
  PublicationMeta,
} from '../../../services/tableBuilderService';
import { LineChartBlock } from './Charts/LineChartBlock';
import { StackedBarVerticalBlock } from './Charts/StackedBarVerticalBlock';
import { StackedBarHorizontalBlock } from './Charts/StackedBarHorizontalBlock';

interface ChartRendererProps {
  type: string;
  attributes: string[];

  data: any;
  meta: PublicationMeta;

  xAxis: Axis;
  yAxis: Axis;
}

export class ChartRenderer extends Component<ChartRendererProps> {
  public render() {
    const allAttributeLabels = Array.prototype.concat(
      ...Object.values(this.props.meta.attributes),
    );

    const usedAttributeLabels = this.props.attributes
      .map(attributeName =>
        allAttributeLabels.find(({ name }) => name === attributeName),
      )
      .filter(_ => _ !== undefined) as AttributesMetaItem[]; // just in case

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
            xAxis={this.props.xAxis}
            yAxis={this.props.yAxis}
          />
        );
      case 'stackedbarvertical':
        return <StackedBarVerticalBlock />;
      case 'stackedbarhorizontal':
        return <StackedBarHorizontalBlock />;
      default:
        return (
          <div>[ Unimplemented chart type requested ${this.props.type} ]</div>
        );
    }
  }
}
