import {
  calculateMargins,
  ChartDataB,
  ChartDefinition,
  createDataForAxis,
  getKeysForChart,
  mapNameToNameLabel,
  populateDefaultChartProps,
  StackedBarProps,
} from '@common/modules/find-statistics/components/charts/ChartFunctions';
import React, { Component } from 'react';
import {
  Bar,
  BarChart,
  CartesianGrid,
  Label,
  Legend,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import LoadingSpinner from '@common/components/LoadingSpinner';

export default class VerticalBarBlock extends Component<StackedBarProps> {
  public static definition: ChartDefinition = {
    type: 'verticalbar',
    name: 'Vertical Bar',

    capabilities: {
      dataSymbols: false,
      stackable: true,
    },

    data: [
      {
        type: 'bar',
        title: 'Bar',
        entryCount: 1,
        targetAxis: 'xaxis',
      },
    ],

    axes: [
      {
        id: 'major',
        title: 'X Axis',
        type: 'major',
        defaultDataType: 'timePeriods',
      },
      {
        id: 'minor',
        title: 'Y Axis',
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
      labels,
      axes,
      stacked,
      legend,
      legendHeight,
    } = this.props;

    if (axes.major && data) {
      const chartData: ChartDataB[] = createDataForAxis(
        axes.major,
        data.result,
        meta,
      ).map(mapNameToNameLabel(labels, meta.timePeriods, meta.locations));

      const keysForChart = getKeysForChart(chartData);

      const yAxis = { key: undefined, title: '' };
      const xAxis = { key: undefined, title: '' };

      return (
        <ResponsiveContainer width={width || 900} height={height || 300}>
          <BarChart
            data={chartData}
            margin={{
              left: 30,
              top: legend === 'top' ? 10 : 0,
            }}
          >
            <CartesianGrid
              strokeDasharray="3 3"
              vertical={axes.minor && axes.minor.showGrid !== false}
              horizontal={axes.major && axes.major.showGrid !== false}
            />

            {axes.minor && (
              <YAxis
                type="number"
                hide={axes.minor.visible === false}
                label={{
                  angle: -90,
                  offset: 0,
                  position: 'left',
                  value: '',
                }}
                scale="auto"
                interval={
                  axes.minor && !axes.minor.visible
                    ? 'preserveStartEnd'
                    : undefined
                }
              />
            )}

            {axes.major && (
              <XAxis
                type="category"
                dataKey="name"
                hide={axes.major.visible === false}
                label={{
                  offset: 5,
                  position: 'bottom',
                  value: '',
                }}
                scale="auto"
                padding={{ left: 20, right: 20 }}
                height={legend === 'bottom' ? 50 : undefined}
                tickMargin={10}
              />
            )}

            <Tooltip />
            {(legend === 'top' || legend === 'bottom') && (
              <Legend
                verticalAlign={legend}
                height={+legendHeight}
                margin={{ top: 5, bottom: 5 }}
              />
            )}

            {Array.from(keysForChart).map(name => (
              <Bar
                key={name}
                {...populateDefaultChartProps(name, labels[name])}
                stackId={stacked ? 'a' : undefined}
                label={{
                  content: <span>hello</span>,
                }}
              />
            ))}
          </BarChart>
        </ResponsiveContainer>
      );
    }

    return <LoadingSpinner />;
  }
}
