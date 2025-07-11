import { useMobileMedia } from '@common/hooks/useMedia';
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
import { axisTickStyle } from '@common/modules/charts/util/chartUtils';
import createDataSetCategories, {
  toChartData,
} from '@common/modules/charts/util/createDataSetCategories';
import {
  getMajorAxisDomainTicks,
  getMinorAxisDomainTicks,
} from '@common/modules/charts/util/domainTicks';
import getDataSetCategoryConfigs from '@common/modules/charts/util/getDataSetCategoryConfigs';
import getCategoryLabel from '@common/modules/charts/util/getCategoryLabel';
import getUnit from '@common/modules/charts/util/getUnit';
import getMinorAxisDecimalPlaces from '@common/modules/charts/util/getMinorAxisDecimalPlaces';
import parseNumber from '@common/utils/number/parseNumber';
import formatPretty from '@common/utils/number/formatPretty';
import getAccessibleTextColour from '@common/utils/colour/getAccessibleTextColour';
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
import groupBy from 'lodash/groupBy';

const defaultLabelTextColour = '#0B0C0C';

export interface HorizontalBarProps extends StackedBarProps {
  legend: LegendConfiguration;
  axes: {
    major: AxisConfiguration;
    minor: AxisConfiguration;
  };
}

const HorizontalBarBlock = ({
  alt,
  data,
  meta,
  height,
  barThickness,
  width,
  stacked = false,

  axes,
  legend,
  includeNonNumericData,
  showDataLabels,
  dataLabelPosition,
}: HorizontalBarProps) => {
  const [legendProps, renderLegend] = useLegend();
  const { isMedia: isMobileMedia } = useMobileMedia();

  const dataSetCategories: DataSetCategory[] = createDataSetCategories({
    axisConfiguration: axes.major,
    data,
    meta,
    includeNonNumericData,
  });

  const groupedDataSetCategories = groupBy(
    dataSetCategories,
    dataSetCategory => dataSetCategory.filter.group,
  );

  const chartDataUnsorted = axes.major.groupByFilterGroups
    ? Object.entries(groupedDataSetCategories).map(([groupKey, group]) =>
        Object.assign({}, ...group.map(toChartData), { name: groupKey }),
      )
    : dataSetCategories.map(toChartData);
  // If no `sortAsc` has been set, we should default
  // to true as it's not really natural to sort in
  // descending order most of the time.
  const chartData =
    axes.major.sortAsc ?? true
      ? chartDataUnsorted
      : chartDataUnsorted.reverse();

  const minorDomainTicks = getMinorAxisDomainTicks(chartData, axes.minor);
  const majorDomainTicks = getMajorAxisDomainTicks(chartData, axes.major);

  // Enforce a max y axis width on mobile as large widths cause
  // the chart to not be visible.
  const maxMobileYAxisWidth = 160;
  const yAxisWidth =
    isMobileMedia && axes.major.size && axes.major.size > maxMobileYAxisWidth
      ? maxMobileYAxisWidth
      : parseNumber(axes.major.size);
  const xAxisHeight = parseNumber(axes.minor.size);

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
  const chartHasNegativeValues =
    (parseNumber(minorDomainTicks.domain?.[0]) ?? 0) < 0;

  if (!chartData.length) {
    return <p className="govuk-!-margin-top-5">No data to display.</p>;
  }

  return (
    <ChartContainer
      height={height || 300}
      legend={legendProps}
      legendPosition={legend.position}
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
          layout="vertical"
          stackOffset={stacked ? 'sign' : undefined}
          margin={{
            left: 30,
            top: 20,
          }}
        >
          <CartesianGrid
            strokeDasharray="3 3"
            horizontal={axes.minor?.showGrid !== false}
            vertical={axes.major.showGrid !== false}
          />

          <XAxis
            {...minorDomainTicks}
            height={xAxisHeight}
            hide={!axes.minor.visible}
            padding={{ left: 0, right: 20 }}
            tick={axisTickStyle}
            tickMargin={10}
            tickFormatter={tick =>
              formatPretty(tick, minorAxisUnit, minorAxisDecimals)
            }
            type="number"
          />

          <YAxis
            {...majorDomainTicks}
            axisLine={!chartHasNegativeValues}
            dataKey="name"
            hide={!axes.major.visible}
            tick={axisTickStyle}
            tickFormatter={getCategoryLabel(dataSetCategories)}
            type="category"
            unit={axes.major.unit}
            width={yAxisWidth}
          />

          <Tooltip
            content={
              <CustomTooltip
                dataSetCategories={dataSetCategories}
                dataSetCategoryConfigs={dataSetCategoryConfigs}
              />
            }
            position={isMobileMedia ? { x: 0 } : undefined}
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
              maxBarSize={barThickness}
              name={config.label}
              fill={config.colour}
              unit={dataSet.indicator.unit}
              stackId={stacked ? 'a' : undefined}
              label={
                showDataLabels
                  ? {
                      fill:
                        dataLabelPosition === 'outside'
                          ? defaultLabelTextColour
                          : getAccessibleTextColour({
                              backgroundColour: config.colour,
                              textColour: defaultLabelTextColour,
                            }),
                      fontSize: 14,
                      position:
                        dataLabelPosition === 'inside'
                          ? 'insideRight'
                          : 'right',
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
          {chartHasNegativeValues && <ReferenceLine x={0} stroke="#666" />}

          {axes.major.referenceLines?.map(referenceLine =>
            createReferenceLine({
              axis: 'y',
              axisType: 'major',
              chartData,
              label: referenceLine.label,
              labelWidth: referenceLine.labelWidth,
              otherAxisDomain: minorDomainTicks.domain,
              otherAxisPosition: referenceLine.otherAxisPosition,
              position: referenceLine.position,
              style: referenceLine.style,
              y: referenceLine.position,
            }),
          )}

          {axes.minor.referenceLines?.map(referenceLine =>
            createReferenceLine({
              axis: 'x',
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
              x: referenceLine.position,
            }),
          )}
        </BarChart>
      </ResponsiveContainer>
    </ChartContainer>
  );
};

export const horizontalBarBlockDefinition: ChartDefinition = {
  type: 'horizontalbar',
  name: 'Horizontal bar',
  capabilities: {
    canIncludeNonNumericData: true,
    canPositionLegendInline: false,
    canSetBarThickness: true,
    canSetDataLabelColour: false,
    canSetDataLabelPosition: true,
    canShowDataLabels: true,
    canShowAllMajorAxisTicks: false,
    canSize: true,
    canSort: true,
    hasGridLines: true,
    hasLegend: true,
    hasLegendPosition: true,
    hasLineStyle: false,
    hasReferenceLines: true,
    hasSymbols: false,
    requiresGeoJson: false,
    stackable: true,
  },
  options: {
    defaults: {
      barThickness: undefined,
      height: 300,
      includeNonNumericData: false,
      showDataLabels: false,
      stacked: false,
    },
  },
  legend: {
    defaults: {
      position: 'bottom',
    },
  },
  axes: {
    major: {
      axis: 'y',
      id: 'major',
      title: 'Y Axis (major axis)',
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
      referenceLineDefaults: {
        style: 'none',
      },
    },
    minor: {
      axis: 'x',
      id: 'minor',
      title: 'X Axis (minor axis)',
      type: 'minor',
      defaults: {
        showGrid: true,
        size: 50,
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

export default memo(HorizontalBarBlock);
