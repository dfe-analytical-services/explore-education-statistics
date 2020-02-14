import {
  ChartDataB,
  ChartDefinition,
  conditionallyAdd,
  createSortedAndMappedDataForAxis,
  generateMajorAxis,
  generateMinorAxis,
  getKeysForChart,
  populateDefaultChartProps,
  StackedBarProps,
} from '@common/modules/find-statistics/components/charts/ChartFunctions';

import classnames from 'classnames';
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

import './charts.scss';

export type HorizontalBarProps = StackedBarProps;

export default class HorizontalBarBlock extends Component<HorizontalBarProps> {
  public static definition: ChartDefinition = {
    type: 'horizontalbar',
    name: 'Horizontal bar',

    capabilities: {
      dataSymbols: false,
      stackable: true,
      lineStyle: false,
      gridLines: true,
      canSize: true,
      fixedAxisGroupBy: false,
      hasAxes: true,
      hasReferenceLines: true,
      hasLegend: true,
    },

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
        defaultDataType: 'timePeriod',
      },
      {
        id: 'minor',
        title: 'X Axis',
        type: 'minor',
      },
    ],

    requiresGeoJson: false,
  };

  public render() {
    const {
      data,
      meta,
      height,
      width,
      stacked = false,
      labels,
      axes,
      legend,
      legendHeight,
      children,
    } = this.props;

    if (
      axes === undefined ||
      axes.major === undefined ||
      axes.minor === undefined ||
      data === undefined ||
      meta === undefined
    )
      return <div>Unable to render chart, chart incorrectly configured</div>;

    const chartData: ChartDataB[] = createSortedAndMappedDataForAxis(
      axes.major,
      data.result,
      meta,
      labels,
    );

    const keysForChart = getKeysForChart(chartData);

    const minorDomainTicks = generateMinorAxis(chartData, axes.minor);
    const majorDomainTicks = generateMajorAxis(chartData, axes.major);

    return (
      <>
        <ResponsiveContainer width={width || '100%'} height={height || 300}>
          <BarChart
            data={chartData}
            layout="vertical"
            className={classnames({ 'legend-bottom': legend === 'bottom' })}
            stackOffset={stacked ? 'sign' : undefined}
            margin={{
              left: 30,
              top: legend === 'top' ? 10 : 0,
            }}
          >
            <CartesianGrid
              strokeDasharray="3 3"
              horizontal={axes.minor && axes.minor.showGrid !== false}
              vertical={axes.major && axes.major.showGrid !== false}
            />

            <XAxis
              type="number"
              dataKey="value"
              hide={axes.minor.visible === false}
              unit={
                (axes.minor.unit &&
                  axes.minor.unit !== '' &&
                  axes.minor.unit) ||
                ''
              }
              scale="auto"
              {...minorDomainTicks}
              height={conditionallyAdd(
                axes.minor.size,
                legend === 'bottom' ? 50 : undefined,
              )}
              padding={{ left: 20, right: 20 }}
              tickMargin={10}
            />

            <YAxis
              type="category"
              dataKey="name"
              hide={axes.major.visible === false}
              unit={
                (axes.major.unit &&
                  axes.major.unit !== '' &&
                  axes.major.unit) ||
                ''
              }
              scale="auto"
              {...majorDomainTicks}
              width={conditionallyAdd(axes.major.size)}
            />

            <Tooltip cursor={false} />
            {(legend === 'top' || legend === 'bottom') && (
              <Legend verticalAlign={legend} height={+(legendHeight || '50')} />
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
                  y={referenceLine.position}
                  label={referenceLine.label}
                />
              ))}

            {axes.minor &&
              axes.minor.referenceLines &&
              axes.minor.referenceLines.map(referenceLine => (
                <ReferenceLine
                  key={`${referenceLine.position}_${referenceLine.label}`}
                  x={referenceLine.position}
                  label={referenceLine.label}
                />
              ))}
          </BarChart>
        </ResponsiveContainer>
        {children}
      </>
    );
  }
}
