import ChartContainer from '@common/modules/charts/components/ChartContainer';
import createReferenceLine from '@common/modules/charts/components/utils/createReferenceLine';
import CustomTooltip from '@common/modules/charts/components/CustomTooltip';
import useLegend from '@common/modules/charts/components/hooks/useLegend';
import {
  AxisConfiguration,
  ChartDefinition,
  ChartProps,
  ChartSymbol,
  LineChartDataLabelPosition,
} from '@common/modules/charts/types/chart';
import { DataSetCategory } from '@common/modules/charts/types/dataSet';
import {
  LegendConfiguration,
  LegendLabelColour,
} from '@common/modules/charts/types/legend';
import { axisTickStyle } from '@common/modules/charts/util/chartUtils';
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
import LineChartLabel from '@common/modules/charts/components/LineChartLabel';
import getUnit from '@common/modules/charts/util/getUnit';
import React, { memo } from 'react';
import {
  CartesianGrid,
  Legend,
  LegendType,
  Line,
  LineChart,
  ReferenceLine,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
  Symbols,
  SymbolsProps,
  LabelProps,
} from 'recharts';
import getDataSetCategoryConfigs from '@common/modules/charts/util/getDataSetCategoryConfigs';

const lineStyles: Dictionary<string> = {
  solid: '',
  dashed: '5 5',
  dotted: '2 2',
};

export interface LineChartProps extends ChartProps {
  dataLabelColour?: LegendLabelColour;
  dataLabelPosition?: LineChartDataLabelPosition;
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
  showDataLabels,
  dataLabelColour = 'inherit',
  dataLabelPosition,
}: LineChartProps) => {
  const [legendProps, renderLegend] = useLegend();

  const dataSetCategories: DataSetCategory[] = createDataSetCategories({
    axisConfiguration: axes.major,
    data,
    meta,
    includeNonNumericData,
  });

  const chartDataUnsorted = dataSetCategories.map(toChartData);
  // If no `sortAsc` has been set, we should default
  // to true as it's not really natural to sort in
  // descending order most of the time.
  const chartData =
    axes.major.sortAsc ?? true
      ? chartDataUnsorted
      : chartDataUnsorted.reverse();

  const minorDomainTicks = getMinorAxisDomainTicks(chartData, axes.minor);
  const majorDomainTicks = getMajorAxisDomainTicks(chartData, axes.major);
  const dataSetCategoryConfigs = getDataSetCategoryConfigs({
    dataSetCategories,
    legendItems: legend.items,
    meta,
  });
  const minorAxisDecimals = getMinorAxisDecimalPlaces(
    dataSetCategoryConfigs,
    axes.minor.decimalPlaces,
  );
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

  const rightMargin =
    legend.position === 'inline' &&
    legend.items.some(legendItem => legendItem.inlinePosition === 'right')
      ? 160 // Arbitrary number that covers max width of labels
      : 0;

  if (!chartData.length) {
    return <p className="govuk-!-margin-top-5">No data to display.</p>;
  }

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
            right: rightMargin,
          }}
        >
          <Tooltip
            content={
              <CustomTooltip
                dataSetCategories={dataSetCategories}
                dataSetCategoryConfigs={dataSetCategoryConfigs}
                order="value"
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
            hide={!axes.minor.visible}
            tick={axisTickStyle}
            tickFormatter={tick =>
              formatPretty(tick, minorAxisUnit, minorAxisDecimals)
            }
            type="number"
            width={yAxisWidth}
          />

          <XAxis
            {...majorDomainTicks}
            axisLine={!chartHasNegativeValues}
            dataKey="name"
            height={xAxisHeight}
            hide={!axes.major.visible}
            padding={{ left: 20, right: 20 }}
            tick={axisTickStyle}
            tickMargin={10}
            tickFormatter={getCategoryLabel(dataSetCategories)}
            type="category"
            unit={axes.major.unit}
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
              label={(props: LabelProps & { index: number }) => (
                <LineChartLabel
                  colour={config.colour}
                  decimalPlaces={dataSet.indicator.decimalPlaces}
                  index={props.index}
                  inlinePositionOffset={config.inlinePositionOffset}
                  isDataLabel={showDataLabels}
                  isLastItem={props.index === chartData.length - 1}
                  isLegendLabel={legend.position === 'inline'}
                  labelColour={
                    showDataLabels ? dataLabelColour : config.labelColour
                  }
                  name={config.label}
                  position={
                    showDataLabels ? dataLabelPosition : config.inlinePosition
                  }
                  unit={dataSet.indicator.unit}
                  value={props.value}
                  x={props.x}
                  y={props.y}
                />
              )}
            />
          ))}

          {chartHasNegativeValues && <ReferenceLine y={0} stroke="#666" />}

          {axes.major.referenceLines?.map(referenceLine =>
            createReferenceLine({
              axis: 'x',
              axisType: 'major',
              chartData,
              label: referenceLine.label,
              labelWidth: referenceLine.labelWidth,
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
              axisDomain: minorDomainTicks.domain,
              axisType: 'minor',
              chartData,
              label: referenceLine.label,
              labelWidth: referenceLine.labelWidth,
              otherAxisDomain: majorDomainTicks.domain,
              otherAxisEnd: referenceLine.otherAxisEnd,
              otherAxisPosition: referenceLine.otherAxisPosition,
              otherAxisStart: referenceLine.otherAxisStart,
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
    canIncludeNonNumericData: true,
    canPositionLegendInline: true,
    canSetBarThickness: false,
    canSetDataLabelColour: true,
    canSetDataLabelPosition: true,
    canShowDataLabels: true,
    canShowAllMajorAxisTicks: false,
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
      includeNonNumericData: false,
      showDataLabels: false,
    },
  },
  legend: {
    defaults: {
      position: 'inline',
    },
  },
  axes: {
    major: {
      axis: 'x',
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
      axis: 'y',
      id: 'yaxis',
      title: 'Y Axis (minor axis)',
      type: 'minor',
      defaults: {
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

const getDot =
  (symbol: ChartSymbol | 'none' = 'circle') =>
  // eslint-disable-next-line react/display-name
  ({ ref, ...props }: SymbolsProps) => {
    if (symbol === 'none') {
      return undefined;
    }

    return <Symbols {...props} ref={ref as never} type={symbol} />;
  };

const getLegendType = (
  symbol: LegendType | undefined = 'square',
): LegendType | undefined => {
  if (symbol === 'none') {
    return undefined;
  }

  return symbol;
};
