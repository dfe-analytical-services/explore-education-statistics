import ChartFunctions, {
  ChartData,
  ChartDefinition,
  ChartProps,
} from '@common/modules/find-statistics/components/charts/ChartFunctions';
import React, { Component } from 'react';
import {
  Bar,
  BarChart,
  CartesianGrid,
  Legend,
  ResponsiveContainer,
  Tooltip,
} from 'recharts';

import { colours } from './Charts';

interface StackedBarHorizontalProps extends ChartProps {
  stacked?: boolean;
}

export default class HorizontalBarBlock extends Component<
  StackedBarHorizontalProps
> {
  public static definition: ChartDefinition = {
    type: 'horizontalbar',
    name: 'Horizontal Bar',

    data: [
      {
        type: 'bar',
        title: 'Bar',
        entryCount: 1,
        targetAxis: 'yaxis',
      },
    ],

    axes: [
      {
        id: 'yaxis',
        title: 'Y Axis',
        type: 'major',
      },
    ],
  };

  public render() {
    const {
      data,
      meta,
      height,
      width,
      xAxis,
      referenceLines,
      yAxis,
      stacked,
      dataSets,
      configuration,
    } = this.props;

    if (dataSets === undefined) return <div />;

    const chartData: ChartData[] = ChartFunctions.generateDataGroupedByGroups(
      ChartFunctions.filterResultsByDataSet(dataSets, data.result),
      meta,
    );

    const chartKeys: Set<string> = chartData.reduce(
      (keys: Set<string>, next) => {
        return new Set([
          ...Array.from(keys),
          ...(next.data || []).map(({ name }) => name),
        ]);
      },
      new Set<string>(),
    );

    return (
      <ResponsiveContainer width={width || '100%'} height={height || 600}>
        <BarChart
          data={chartData}
          layout="vertical"
          margin={ChartFunctions.calculateMargins(yAxis, xAxis, referenceLines)}
        >
          {ChartFunctions.calculateYAxis(yAxis, {
            type: 'category',
            dataKey: (xAxis.key || ['name'])[0],
          })}

          <CartesianGrid />

          {ChartFunctions.calculateXAxis(xAxis, { type: 'number' })}

          <Tooltip cursor={false} />
          <Legend />

          {Array.from(chartKeys).map((dataKey, index) => (
            <Bar
              key={dataKey}
              dataKey={dataKey}
              fill={colours[index]}
              name={
                configuration.dataLabels[dataKey] &&
                configuration.dataLabels[dataKey].label
              }
              unit={
                configuration.dataLabels[dataKey] &&
                configuration.dataLabels[dataKey].unit
              }
              stackId={stacked ? 'a' : undefined}
            />
          ))}

          {referenceLines &&
            ChartFunctions.generateReferenceLines(referenceLines)}
        </BarChart>
      </ResponsiveContainer>
    );
  }
}
