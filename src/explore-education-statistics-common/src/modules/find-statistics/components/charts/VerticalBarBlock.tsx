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

export type VerticalBarProps = StackedBarProps;

export default class VerticalBarBlock extends Component<VerticalBarProps> {
  public static definition: ChartDefinition = {
    type: 'verticalbar',
    name: 'Vertical bar',

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
        targetAxis: 'xaxis',
      },
    ],

    axes: [
      {
        id: 'major',
        title: 'X Axis',
        type: 'major',
        defaultDataType: 'timePeriod',
      },
      {
        id: 'minor',
        title: 'Y Axis',
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
      labels,
      axes,
      stacked,
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

            <YAxis
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
              width={conditionallyAdd(axes.minor && axes.minor.size)}
            />

            <XAxis
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
              padding={{ left: 20, right: 20 }}
              height={conditionallyAdd(
                axes.major.size,
                legend === 'bottom' ? 0 : undefined,
              )}
              tickMargin={10}
            />

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
        {children}
      </>
    );
  }
}
