import ChartContainer from '@common/modules/charts/components/ChartContainer';
import CustomTooltip from '@common/modules/charts/components/CustomTooltip';
import useLegend from '@common/modules/charts/components/hooks/useLegend';
import createReferenceLine from '@common/modules/charts/components/utils/createReferenceLine';
import {
  AxisConfiguration,
  ChartDefinition,
  StackedBarProps,
} from '@common/modules/charts/types/chart';
import { DataSetCategory } from '@common/modules/charts/types/dataSet';
import { LegendConfiguration } from '@common/modules/charts/types/legend';
import createDataSetCategories, {
  toChartData,
} from '@common/modules/charts/util/createDataSetCategories';
import {
  getMajorAxisDomainTicks,
  getMinorAxisDomainTicks,
} from '@common/modules/charts/util/domainTicks';
import getDataSetCategoryConfigs from '@common/modules/charts/util/getDataSetCategoryConfigs';
import getCategoryLabel from '@common/modules/charts/util/getCategoryLabel';
import getMinorAxisDecimalPlaces from '@common/modules/charts/util/getMinorAxisDecimalPlaces';
import parseNumber from '@common/utils/number/parseNumber';
import getUnit from '@common/modules/charts/util/getUnit';
import getMinorAxisSize from '@common/modules/charts/util/getMinorAxisSize';
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
import formatPretty from '@common/utils/number/formatPretty';

export interface VerticalBarProps extends StackedBarProps {
  legend: LegendConfiguration;
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
  barThickness,
  axes,
  stacked,
  legend,
  includeNonNumericData,
  showDataLabels,
  dataLabelPosition,
}: VerticalBarProps) => {
  const [legendProps, renderLegend] = useLegend();
  if (
    axes === undefined ||
    axes.major === undefined ||
    axes.minor === undefined ||
    data === undefined ||
    meta === undefined
  ) {
    return <div>Unable to render chart, chart incorrectly configured</div>;
  }

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
  const chartHasNegativeValues =
    (parseNumber(minorDomainTicks.domain?.[0]) ?? 0) < 0;

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
        <BarChart
          aria-label={alt}
          role="img"
          focusable={false}
          data={chartData}
          margin={{
            left: 30,
            top: 20,
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
            width={yAxisWidth}
            tickFormatter={tick =>
              formatPretty(tick, minorAxisUnit, minorAxisDecimals)
            }
          />

          <XAxis
            {...majorDomainTicks}
            type="category"
            axisLine={!chartHasNegativeValues}
            dataKey="name"
            hide={!axes.major.visible}
            unit={axes.major.unit}
            height={parseNumber(axes.major.size)}
            padding={{ left: 20, right: 20 }}
            tickMargin={10}
            tickFormatter={getCategoryLabel(dataSetCategories)}
          />

          <Tooltip
            content={
              <CustomTooltip
                dataSetCategories={dataSetCategories}
                dataSetCategoryConfigs={dataSetCategoryConfigs}
              />
            }
            wrapperStyle={{ zIndex: 1000 }}
          />

          {legend.position !== 'none' && (
            <Legend content={renderLegend} align="left" layout="vertical" />
          )}

          {dataSetCategoryConfigs.map(({ config, dataKey, dataSet }) => (
            <Bar
              key={dataKey}
              dataKey={dataKey}
              isAnimationActive={false}
              name={config.label}
              fill={config.colour}
              unit={dataSet.indicator.unit}
              stackId={stacked ? 'a' : undefined}
              maxBarSize={barThickness}
              label={
                showDataLabels
                  ? {
                      fontSize: 14,
                      offset: 5,
                      position:
                        dataLabelPosition === 'inside' ? 'insideTop' : 'top',
                      formatter: (value: string | number) =>
                        formatPretty(
                          value.toString(),
                          dataSet.indicator.unit,
                          dataSet.indicator.decimalPlaces,
                        ),
                    }
                  : undefined
              }
            />
          ))}

          {chartHasNegativeValues && <ReferenceLine y={0} stroke="#666" />}

          {axes.major.referenceLines?.map(referenceLine =>
            createReferenceLine({
              axis: 'x',
              axisType: 'major',
              chartData,
              label: referenceLine.label,
              otherAxisDomain: minorDomainTicks.domain,
              otherAxisPosition: referenceLine.otherAxisPosition,
              position: referenceLine.position,
              style: referenceLine.style,
              x: referenceLine.position,
            }),
          )}

          {axes.minor.referenceLines?.map(referenceLine =>
            createReferenceLine({
              axis: 'y',
              axisType: 'minor',
              chartData,
              label: referenceLine.label,
              otherAxisDomain: majorDomainTicks.domain,
              otherAxisPosition: referenceLine.otherAxisPosition,
              position: referenceLine.position,
              style: referenceLine.style,
              y: referenceLine.position,
            }),
          )}
        </BarChart>
      </ResponsiveContainer>
    </ChartContainer>
  );
};

export const verticalBarBlockDefinition: ChartDefinition = {
  type: 'verticalbar',
  name: 'Vertical bar',
  capabilities: {
    canPositionLegendInline: false,
    canSize: true,
    canSort: true,
    hasGridLines: true,
    hasLegend: true,
    hasLegendPosition: true,
    hasLineStyle: false,
    hasSymbols: false,
    hasReferenceLines: true,
    requiresGeoJson: false,
    stackable: true,
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
      axis: 'x',
      id: 'major',
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
      referenceLineDefaults: {
        style: 'none',
      },
    },
    minor: {
      axis: 'y',
      id: 'minor',
      title: 'Y Axis (minor axis)',
      type: 'minor',
      capabilities: {
        canRotateLabel: true,
      },
      defaults: {
        showGrid: true,
        tickConfig: 'default',
        tickSpacing: 1,
        unit: '',
        label: {
          width: 100,
        },
      },
      referenceLineDefaults: {
        style: 'none',
      },
    },
  },
};

export default memo(VerticalBarBlock);
