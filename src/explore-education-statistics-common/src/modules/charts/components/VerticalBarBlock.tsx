import ChartContainer from '@common/modules/charts/components/ChartContainer';
import CustomTooltip from '@common/modules/charts/components/CustomTooltip';
import {
  AxisConfiguration,
  ChartDefinition,
  RenderLegend,
  StackedBarProps,
} from '@common/modules/charts/types/chart';
import { DataSetCategory } from '@common/modules/charts/types/dataSet';
import createDataSetCategories, {
  toChartData,
} from '@common/modules/charts/util/createDataSetCategories';
import {
  getMajorAxisDomainTicks,
  getMinorAxisDomainTicks,
} from '@common/modules/charts/util/domainTicks';
import getCategoryDataSetConfigurations from '@common/modules/charts/util/getCategoryDataSetConfigurations';
import getCategoryLabel from '@common/modules/charts/util/getCategoryLabel';
import parseNumber from '@common/utils/number/parseNumber';
import React, { memo } from 'react';
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

export interface VerticalBarProps extends StackedBarProps {
  axes: {
    major: AxisConfiguration;
    minor: AxisConfiguration;
  };
}

const VerticalBarBlock = ({
  alt,
  data,
  meta,
  height,
  width,
  axes,
  stacked,
  legend,
  renderLegend,
}: VerticalBarProps & RenderLegend) => {
  if (
    axes === undefined ||
    axes.major === undefined ||
    axes.minor === undefined ||
    data === undefined ||
    meta === undefined
  )
    return <div>Unable to render chart, chart incorrectly configured</div>;

  const dataSetCategories: DataSetCategory[] = createDataSetCategories(
    axes.major,
    data,
    meta,
  );

  const chartData = dataSetCategories.map(toChartData);

  const minorDomainTicks = getMinorAxisDomainTicks(chartData, axes.minor);
  const majorDomainTicks = getMajorAxisDomainTicks(chartData, axes.major);

  const yAxisWidth = parseNumber(axes.minor.size);
  const xAxisHeight = parseNumber(axes.major.size);

  return (
    <ChartContainer
      height={height || 300}
      yAxisWidth={yAxisWidth}
      yAxisLabel={axes.major.label}
      xAxisHeight={xAxisHeight}
      xAxisLabel={axes.minor.label}
    >
      <ResponsiveContainer width={width || '100%'} height={height || 300}>
        <BarChart
          aria-label={alt}
          role="img"
          focusable={false}
          data={chartData}
          margin={{
            left: 30,
          }}
        >
          <CartesianGrid
            strokeDasharray="3 3"
            vertical={axes.minor.showGrid !== false}
            horizontal={axes.major.showGrid !== false}
          />

          <YAxis
            {...minorDomainTicks}
            type="number"
            hide={!axes.minor.visible}
            unit={axes.minor.unit}
            width={parseNumber(axes.minor.size)}
          />

          <XAxis
            {...majorDomainTicks}
            type="category"
            dataKey="name"
            hide={!axes.major.visible}
            unit={axes.major.unit}
            height={parseNumber(axes.major.size)}
            padding={{ left: 20, right: 20 }}
            tickMargin={10}
            tickFormatter={getCategoryLabel(dataSetCategories)}
          />

          <Tooltip
            content={<CustomTooltip dataSetCategories={dataSetCategories} />}
            wrapperStyle={{ zIndex: 1000 }}
          />

          {legend && legend !== 'none' && (
            <Legend content={renderLegend} align="left" layout="vertical" />
          )}

          {getCategoryDataSetConfigurations(
            dataSetCategories,
            axes.major,
            meta,
          ).map(({ config, dataKey, dataSet }) => (
            <Bar
              key={dataKey}
              dataKey={dataKey}
              isAnimationActive={false}
              name={config.label}
              fill={config.colour}
              unit={dataSet.indicator.unit}
              stackId={stacked ? 'a' : undefined}
            />
          ))}

          {axes.major.referenceLines?.map(referenceLine => (
            <ReferenceLine
              key={`${referenceLine.position}_${referenceLine.label}`}
              x={referenceLine.position}
              label={referenceLine.label}
            />
          ))}

          {axes.minor.referenceLines?.map(referenceLine => (
            <ReferenceLine
              key={`${referenceLine.position}_${referenceLine.label}`}
              y={referenceLine.position}
              label={referenceLine.label}
            />
          ))}
        </BarChart>
      </ResponsiveContainer>
    </ChartContainer>
  );
};

export const verticalBarBlockDefinition: ChartDefinition = {
  type: 'verticalbar',
  name: 'Vertical bar',
  capabilities: {
    dataSymbols: false,
    stackable: true,
    lineStyle: false,
    gridLines: true,
    canSize: true,
    canSort: true,
    fixedAxisGroupBy: false,
    hasReferenceLines: true,
    hasLegend: true,
    requiresGeoJson: false,
  },
  options: {
    defaults: {
      height: 300,
      legend: 'top',
    },
  },
  data: [
    {
      type: 'bar',
      title: 'Bar',
      entryCount: 1,
      targetAxis: 'xaxis',
    },
  ],
  axes: {
    major: {
      id: 'major',
      title: 'X Axis (major axis)',
      type: 'major',
      defaults: {
        groupBy: 'timePeriod',
        min: 0,
        showGrid: true,
        size: 50,
        sortAsc: true,
        sortBy: 'name',
        tickConfig: 'default',
        tickSpacing: 1,
        unit: '',
      },
    },
    minor: {
      id: 'minor',
      title: 'Y Axis (minor axis)',
      type: 'minor',
      defaults: {
        min: 0,
        showGrid: true,
        size: 50,
        tickConfig: 'default',
        tickSpacing: 1,
        unit: '',
      },
    },
  },
};

export default memo(VerticalBarBlock);
