import CustomTooltip from '@common/modules/charts/components/CustomTooltip';
import {
  AxisConfiguration,
  ChartDefinition,
  ChartProps,
  ChartSymbol,
  RenderLegend,
} from '@common/modules/charts/types/chart';
import { DataSetCategory } from '@common/modules/charts/types/dataSet';
import createDataSetCategories, {
  toChartData,
} from '@common/modules/charts/util/createDataSetCategories';
import {
  getMajorAxisDomainTicks,
  getMinorAxisDomainTicks,
} from '@common/modules/charts/util/domainTicks';
import getCategoryLabel from '@common/modules/charts/util/getCategoryLabel';
import { Dictionary } from '@common/types';
import parseNumber from '@common/utils/number/parseNumber';

import React, { memo } from 'react';
import {
  CartesianGrid,
  Legend,
  LegendType,
  Line,
  LineChart,
  ReferenceLine,
  ResponsiveContainer,
  Symbols,
  SymbolsProps,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import getCategoryDataSetConfigurations from '../util/getCategoryDataSetConfigurations';

const lineStyles: Dictionary<string> = {
  solid: '',
  dashed: '5 5',
  dotted: '2 2',
};

const getDot = (symbol: ChartSymbol | 'none' = 'circle') => (
  props: SymbolsProps,
) => {
  if (symbol === 'none') {
    return undefined;
  }

  return <Symbols {...props} type={symbol} />;
};

const getLegendType = (
  symbol: LegendType | undefined = 'square',
): LegendType | undefined => {
  if (symbol === 'none') {
    return undefined;
  }

  return symbol;
};

export interface LineChartProps extends ChartProps {
  axes: {
    major: AxisConfiguration;
    minor: AxisConfiguration;
  };
}

const LineChartBlock = ({
  alt,
  data,
  meta,
  height,
  axes,
  legend,
  width,
  renderLegend,
}: LineChartProps & RenderLegend) => {
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
      <LineChart
        aria-label={alt}
        role="img"
        focusable={false}
        data={chartData}
        margin={{
          left: 30,
        }}
      >
        <Tooltip
          content={<CustomTooltip dataSetCategories={dataSetCategories} />}
          wrapperStyle={{ zIndex: 1000 }}
        />

        {legend && legend !== 'none' && (
          <Legend content={renderLegend} align="left" layout="vertical" />
        )}

        <CartesianGrid
          strokeDasharray="3 3"
          horizontal={axes.minor.showGrid !== false}
          vertical={axes.major.showGrid !== false}
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

        {getCategoryDataSetConfigurations(
          dataSetCategories,
          axes.major,
          meta,
        ).map(({ config, dataKey, dataSet }) => (
          <Line
            key={dataKey}
            dataKey={dataKey}
            isAnimationActive={false}
            name={config.label}
            stroke={config.colour}
            fill={config.colour}
            unit={dataSet.indicator.unit}
            type="linear"
            legendType={getLegendType(config.symbol)}
            dot={getDot(config.symbol)}
            strokeWidth="2"
            strokeDasharray={lineStyles[config.lineStyle ?? 'solid']}
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
      </LineChart>
    </ResponsiveContainer>
  );
};

export const lineChartBlockDefinition: ChartDefinition = {
  type: 'line',
  name: 'Line',
  capabilities: {
    dataSymbols: true,
    stackable: false,
    lineStyle: true,
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
      type: 'line',
      title: 'Line',
      entryCount: 'multiple',
      targetAxis: 'xaxis',
    },
  ],
  axes: {
    major: {
      id: 'xaxis',
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
      id: 'yaxis',
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

export default memo(LineChartBlock);
