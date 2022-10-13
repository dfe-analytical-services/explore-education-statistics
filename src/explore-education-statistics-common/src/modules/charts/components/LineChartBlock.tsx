import ChartContainer from '@common/modules/charts/components/ChartContainer';
import createReferenceLine from '@common/modules/charts/components/utils/createReferenceLine';
import CustomTooltip from '@common/modules/charts/components/CustomTooltip';
import useLegend from '@common/modules/charts/components/hooks/useLegend';
import {
  AxisConfiguration,
  ChartDefinition,
  ChartProps,
  ChartSymbol,
} from '@common/modules/charts/types/chart';
import { DataSetCategory } from '@common/modules/charts/types/dataSet';
import { LegendConfiguration } from '@common/modules/charts/types/legend';
import createDataSetCategories, {
  toChartData,
} from '@common/modules/charts/util/createDataSetCategories';
import getMinorAxisSize from '@common/modules/charts/util/getMinorAxisSize';
import {
  getMajorAxisDomainTicks,
  getMinorAxisDomainTicks,
} from '@common/modules/charts/util/domainTicks';
import getCategoryLabel from '@common/modules/charts/util/getCategoryLabel';
import getMinorAxisDecimalPlaces from '@common/modules/charts/util/getMinorAxisDecimalPlaces';
import { Dictionary } from '@common/types';
import formatPretty from '@common/utils/number/formatPretty';
import parseNumber from '@common/utils/number/parseNumber';
import LineChartLegendLabel from '@common/modules/charts/components/LineChartLegendLabel';
import getUnit from '@common/modules/charts/util/getUnit';
import React, { memo } from 'react';
import {
  CartesianGrid,
  Legend,
  LegendType,
  Line,
  LineChart,
  ResponsiveContainer,
  Symbols,
  SymbolsProps,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import getDataSetCategoryConfigs from '@common/modules/charts/util/getDataSetCategoryConfigs';

const lineStyles: Dictionary<string> = {
  solid: '',
  dashed: '5 5',
  dotted: '2 2',
};

// eslint-disable-next-line react/display-name
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
  legend: LegendConfiguration;
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
  includeNonNumericData,
}: LineChartProps) => {
  const [legendProps, renderLegend] = useLegend();

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
    includeNonNumericData,
  );

  const chartData = dataSetCategories.map(toChartData);

  const minorDomainTicks = getMinorAxisDomainTicks(chartData, axes.minor);
  const majorDomainTicks = getMajorAxisDomainTicks(chartData, axes.major);
  const dataSetCategoryConfigs = getDataSetCategoryConfigs(
    dataSetCategories,
    legend.items,
    meta,
  );
  const minorAxisDecimals = getMinorAxisDecimalPlaces(dataSetCategoryConfigs);
  const minorAxisUnit = axes.minor.unit || getUnit(dataSetCategoryConfigs);
  const yAxisWidth = getMinorAxisSize({
    dataSetCategories,
    minorAxisSize: axes.minor.size,
    minorAxisDecimals,
    minorAxisUnit,
  });
  const xAxisHeight = parseNumber(axes.major.size);

  return (
    <ChartContainer
      height={height || 300}
      legend={legendProps}
      legendPosition={legend.position}
      yAxisWidth={yAxisWidth}
      yAxisLabel={axes.minor.label}
      xAxisHeight={xAxisHeight}
      xAxisLabel={axes.major.label}
    >
      <ResponsiveContainer width={width || '100%'} height={height || 300}>
        <LineChart
          aria-label={alt}
          role="img"
          focusable={false}
          data={chartData}
          margin={{
            left: 30,
            top: 20,
          }}
        >
          <Tooltip
            content={
              <CustomTooltip
                dataSetCategories={dataSetCategories}
                dataSetCategoryConfigs={dataSetCategoryConfigs}
              />
            }
            wrapperStyle={{ zIndex: 1000 }}
          />

          {legend.position !== 'none' && legend.position !== 'inline' && (
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
            width={yAxisWidth}
            tickFormatter={tick =>
              formatPretty(tick, minorAxisUnit, minorAxisDecimals)
            }
          />

          <XAxis
            {...majorDomainTicks}
            type="category"
            dataKey="name"
            hide={!axes.major.visible}
            unit={axes.major.unit}
            height={xAxisHeight}
            padding={{ left: 20, right: 20 }}
            tickMargin={10}
            tickFormatter={getCategoryLabel(dataSetCategories)}
          />

          {dataSetCategoryConfigs.map(({ config, dataKey, dataSet }) => (
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
              label={props =>
                legend.position === 'inline' ? (
                  <LineChartLegendLabel
                    {...props}
                    name={config.label}
                    colour={config.colour}
                    totalDataPoints={chartData.length}
                    labelPosition={config.inlinePosition}
                  />
                ) : null
              }
            />
          ))}

          {axes.major.referenceLines?.map(referenceLine =>
            createReferenceLine({
              chartData,
              label: referenceLine.label,
              position: referenceLine.position,
              style: referenceLine.style,
              x: referenceLine.position,
            }),
          )}

          {axes.minor.referenceLines?.map(referenceLine =>
            createReferenceLine({
              chartData,
              label: referenceLine.label,
              position: referenceLine.position,
              style: referenceLine.style,
              y: referenceLine.position,
            }),
          )}
        </LineChart>
      </ResponsiveContainer>
    </ChartContainer>
  );
};

export const lineChartBlockDefinition: ChartDefinition = {
  type: 'line',
  name: 'Line',
  capabilities: {
    canPositionLegendInline: true,
    canSize: true,
    canSort: true,
    hasGridLines: true,
    hasLegend: true,
    hasLegendPosition: true,
    hasLineStyle: true,
    hasReferenceLines: true,
    hasSymbols: true,
    requiresGeoJson: false,
    stackable: false,
  },
  options: {
    defaults: {
      height: 300,
    },
  },
  legend: {
    defaults: {
      position: 'bottom',
    },
  },
  axes: {
    major: {
      id: 'xaxis',
      title: 'X Axis (major axis)',
      type: 'major',
      capabilities: {
        canRotateLabel: false,
      },
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
      capabilities: {
        canRotateLabel: true,
      },
      defaults: {
        min: 0,
        showGrid: true,
        tickConfig: 'default',
        tickSpacing: 1,
        unit: '',
        label: {
          width: 100,
        },
      },
    },
  },
};

export default memo(LineChartBlock);
