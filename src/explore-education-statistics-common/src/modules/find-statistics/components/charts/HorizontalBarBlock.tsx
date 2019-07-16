import {
  generateReferenceLines,
  ChartDefinition,
  ChartDataB,
  createDataForAxis,
  getKeysForChart,
  mapNameToNameLabel,
  StackedBarProps,
  populateDefaultChartProps,
} from '@common/modules/find-statistics/components/charts/ChartFunctions';
import React, { Component } from 'react';
import {
  Bar,
  BarChart,
  CartesianGrid,
  Legend,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import LoadingSpinner from '@common/components/LoadingSpinner';

export default class HorizontalBarBlock extends Component<StackedBarProps> {
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
        id: 'major',
        title: 'Y Axis',
        type: 'major',
        defaultDataType: 'timePeriods',
      },
      {
        id: 'minor',
        title: 'X Axis',
        type: 'minor',
      },
    ],
  };

  public render() {
    const {
      data,
      meta,
      height,
      width,
      referenceLines,
      stacked = false,
      labels,
      axes,
    } = this.props;

    if (!axes.major || !data) return <LoadingSpinner />;

    const chartData: ChartDataB[] = createDataForAxis(
      axes.major,
      data.result,
      meta,
    ).map(mapNameToNameLabel(labels, meta.timePeriods));

    const keysForChart = getKeysForChart(chartData);

    return (
      <ResponsiveContainer width={width || '100%'} height={height || 600}>
        <BarChart data={chartData} layout="vertical" margin={{ left: 30 }}>
          <CartesianGrid
            strokeDasharray="3 3"
            horizontal={axes.minor && axes.minor.showGrid !== false}
            vertical={axes.major && axes.major.showGrid !== false}
          />

          {axes.minor && axes.minor.visible !== false && (
            <XAxis
              type="number"
              label={{
                angle: -90,
                offset: 0,
                position: 'left',
                value: '',
              }}
              scale="auto"
              padding={{ left: 20, right: 20 }}
              tickMargin={10}
            />
          )}

          {axes.major && axes.major.visible !== false && (
            <YAxis
              type="category"
              dataKey="name"
              label={{
                offset: 5,
                position: 'bottom',
                value: '',
              }}
              scale="auto"
              interval={
                axes.minor && axes.minor.visible !== false
                  ? 'preserveStartEnd'
                  : undefined
              }
            />
          )}

          <Tooltip cursor={false} />
          <Legend />

          {Array.from(keysForChart).map(name => (
            <Bar
              key={name}
              {...populateDefaultChartProps(name, labels[name])}
              stackId={stacked ? 'a' : undefined}
            />
          ))}

          {referenceLines && generateReferenceLines(referenceLines)}
        </BarChart>
      </ResponsiveContainer>
    );
  }
}
