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
import expandDataSet from '@common/modules/charts/util/expandDataSet';
import getCategoryDataSetConfigurations from '@common/modules/charts/util/getCategoryDataSetConfigurations';
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

export interface HorizontalBarProps extends StackedBarProps {
  axes: {
    major: AxisConfiguration;
    minor: AxisConfiguration;
  };
}

const HorizontalBarBlock = ({
  data,
  meta,
  height,
  width,
  stacked = false,
  labels,
  axes,
  legend,
  renderLegend,
}: HorizontalBarProps & RenderLegend) => {
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

  return (
    <ResponsiveContainer width={width || '100%'} height={height || 300}>
      <BarChart
        data={chartData}
        layout="vertical"
        stackOffset={stacked ? 'sign' : undefined}
        margin={{
          left: 30,
        }}
      >
        <CartesianGrid
          strokeDasharray="3 3"
          horizontal={axes.minor?.showGrid !== false}
          vertical={axes.major.showGrid !== false}
        />

        <XAxis
          {...minorDomainTicks}
          type="number"
          hide={!axes.minor.visible}
          unit={axes.minor.unit}
          height={parseNumber(axes.minor.size)}
          padding={{ left: 20, right: 20 }}
          tickMargin={10}
        />

        <YAxis
          {...majorDomainTicks}
          type="category"
          dataKey="name"
          hide={!axes.major.visible}
          unit={axes.major.unit}
          width={parseNumber(axes.major.size)}
        />

        <Tooltip content={CustomTooltip} wrapperStyle={{ zIndex: 1000 }} />

        {legend && legend !== 'none' && (
          <Legend content={renderLegend} align="left" layout="vertical" />
        )}

        {getCategoryDataSetConfigurations(
          dataSetCategories,
          labels,
          axes.major,
          meta,
        ).map(config => (
          <Bar
            key={config.dataKey}
            dataKey={config.dataKey}
            isAnimationActive={false}
            name={config.label}
            fill={config.colour}
            unit={config.dataSet.indicator.unit}
            stackId={stacked ? 'a' : undefined}
          />
        ))}

        {axes.major.referenceLines?.map(referenceLine => (
          <ReferenceLine
            key={`${referenceLine.position}_${referenceLine.label}`}
            y={referenceLine.position}
            label={referenceLine.label}
          />
        ))}

        {axes.minor.referenceLines?.map(referenceLine => (
          <ReferenceLine
            key={`${referenceLine.position}_${referenceLine.label}`}
            x={referenceLine.position}
            label={referenceLine.label}
          />
        ))}
      </BarChart>
    </ResponsiveContainer>
  );
};

export const horizontalBarBlockDefinition: ChartDefinition = {
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
      targetAxis: 'yaxis',
    },
  ],
  axes: {
    major: {
      id: 'major',
      title: 'Y Axis (major axis)',
      type: 'major',
      defaults: {
        groupBy: 'timePeriod',
      },
    },
    minor: {
      id: 'minor',
      title: 'X Axis (minor axis)',
      type: 'minor',
    },
  },
};

export default memo(HorizontalBarBlock);
