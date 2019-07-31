import {
  ChartDataB,
  ChartDefinition,
  conditionallyAdd,
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
  Legend,
  ReferenceLine,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';

import classnames from 'classnames';

import './charts.scss';

export default class VerticalBarBlock extends Component<StackedBarProps> {
  public static definition: ChartDefinition = {
    type: 'verticalbar',
    name: 'Vertical bar',

    capabilities: {
      dataSymbols: false,
      stackable: true,
      lineStyle: false,
      gridLines: true,
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

    if (axes.major === undefined || data === undefined || meta === undefined)
      return <div>Unable to render chart</div>;

    const chartData: ChartDataB[] = createDataForAxis(
      axes.major,
      data.result,
      meta,
    ).map(mapNameToNameLabel(labels, meta.timePeriods, meta.locations));

    const keysForChart = getKeysForChart(chartData);

    return (
      <ResponsiveContainer width={width || 900} height={height || 300}>
        <BarChart
          data={chartData}
          className={classnames({ 'legend-bottom': legend === 'bottom' })}
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
              width={conditionallyAdd(axes.minor && axes.minor.size)}
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
              height={conditionallyAdd(
                axes.major && axes.major.size,
                legend === 'bottom' ? 50 : undefined,
              )}
              tickMargin={10}
            />
          )}

          <Tooltip />
          {(legend === 'top' || legend === 'bottom') && (
            <Legend
              verticalAlign={legend}
              height={+(legendHeight || '50')}
              margin={{ top: 5, bottom: 5 }}
            />
          )}

          {Array.from(keysForChart).map(name => (
            <Bar
              key={name}
              {...populateDefaultChartProps(name, labels[name])}
              stackId={stacked ? 'a' : undefined}
            />
          ))}

          {axes.major &&
            axes.major.referenceLines &&
            axes.major.referenceLines.map(referenceLine => (
              <ReferenceLine
                key={`${referenceLine.position}_${referenceLine.label}`}
                x={referenceLine.position}
                label={referenceLine.label}
              />
            ))}

          {axes.minor &&
            axes.minor.referenceLines &&
            axes.minor.referenceLines.map(referenceLine => (
              <ReferenceLine
                key={`${referenceLine.position}_${referenceLine.label}`}
                y={referenceLine.position}
                label={referenceLine.label}
              />
            ))}
        </BarChart>
      </ResponsiveContainer>
    );
  }
}
