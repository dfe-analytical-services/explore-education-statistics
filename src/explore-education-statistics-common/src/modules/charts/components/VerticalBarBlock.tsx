import { useDesktopMedia } from '@common/hooks/useMedia';
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
import getMinorAxisDecimalPlaces from '@common/modules/charts/util/getMinorAxisDecimalPlaces';
import formatPretty from '@common/utils/number/formatPretty';
import parseNumber from '@common/utils/number/parseNumber';
import getUnit from '@common/modules/charts/util/getUnit';
import getMinorAxisSize from '@common/modules/charts/util/getMinorAxisSize';
import { otherAxisPositionTypes } from '@common/modules/charts/types/referenceLinePosition';
import React, {
  memo,
  RefObject,
  useCallback,
  useEffect,
  useRef,
  useState,
} from 'react';
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
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';

const chartBottomMargin = 20;

export interface VerticalBarProps extends StackedBarProps {
  legend: LegendConfiguration;
  axes: {
    major: AxisConfiguration;
    minor: AxisConfiguration;
  };
}

const VerticalBarBlock = ({
  alt,
  axes,
  barThickness,
  data,
  height,
  includeNonNumericData,
  legend,
  meta,
  showDataLabels,
  stacked,
}: VerticalBarProps) => {
  const { isMedia: isDesktopMedia } = useDesktopMedia();
  const [xAxisTickWidth, setXAxisTickWidth] = useState<number>();
  const containerRef = useRef<RefObject<HTMLDivElement>>(null);

  // Make recharts show all x axis ticks, it automatically
  // shows / hides them depending on available space otherwise.
  const showAllXAxisTicks =
    isDesktopMedia && axes.major.tickConfig === 'showAll';

  const [legendProps, renderLegend] = useLegend();

  const dataSetCategories: DataSetCategory[] = createDataSetCategories({
    axisConfiguration: axes.major,
    data,
    includeNonNumericData,
    meta,
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

  const majorAxisCategories = chartData.map(({ name }) => name);
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

  // Major axis reference lines positioned between data points need to be rendered on
  // the minor axis. Filter them out, add a `perpendicularLine` flag to make them
  // identifiable and append them to the minor axis reference lines.
  const majorAxisReferenceLines = axes.major.referenceLines.filter(
    line => line.position !== otherAxisPositionTypes.betweenDataPoints,
  );
  const perpendicularMajorAxisReferenceLines = axes.major.referenceLines
    .filter(line => line.position === otherAxisPositionTypes.betweenDataPoints)
    .map(line => ({ ...line, perpendicularLine: true }));
  const minorAxisReferenceLines = [
    ...axes.minor.referenceLines,
    ...perpendicularMajorAxisReferenceLines,
  ];

  const resizeTicks = useCallback(
    (containerWidth: number) => {
      // Set x axis tick width based on the number of ticks. 20 is to add some spacing.
      // Only do this when showAllXAxisTicks is true, recharts handles it otherwise.
      setXAxisTickWidth(
        showAllXAxisTicks
          ? (containerWidth - yAxisWidth) / majorAxisCategories.length - 20
          : undefined,
      );
    },
    [majorAxisCategories.length, showAllXAxisTicks, yAxisWidth],
  );

  useEffect(() => {
    // A known bug with refs in recharts 2.x results in having to use `current.current`.
    // It should be fixed in v3.
    if (showAllXAxisTicks && containerRef.current?.current?.clientWidth) {
      resizeTicks(containerRef.current?.current?.clientWidth);
    }
  }, [resizeTicks, showAllXAxisTicks]);

  const [handleResize] = useDebouncedCallback((containerWidth: number) => {
    resizeTicks(containerWidth);
  }, 300);

  if (!chartData.length) {
    return <p className="govuk-!-margin-top-5">No data to display.</p>;
  }

  return (
    <ChartContainer
      height={height || 300}
      legend={
        stacked && legendProps?.payload
          ? { ...legendProps, payload: [...legendProps.payload].reverse() }
          : legendProps
      }
      legendPosition={legend.position}
      yAxisWidth={yAxisWidth}
      yAxisLabel={axes.minor.label}
      xAxisHeight={xAxisHeight}
      xAxisLabel={axes.major.label}
    >
      <ResponsiveContainer
        height={height || 300}
        onResize={handleResize}
        ref={containerRef}
      >
        <BarChart
          aria-label={alt}
          role="img"
          focusable={false}
          data={chartData}
          margin={{
            left: 30,
            top: chartBottomMargin,
          }}
        >
          <CartesianGrid
            strokeDasharray="3 3"
            vertical={axes.minor.showGrid !== false}
            horizontal={axes.major.showGrid !== false}
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
            height={parseNumber(axes.major.size)}
            hide={!axes.major.visible}
            interval={showAllXAxisTicks ? 0 : undefined}
            padding={{ left: 20, right: 20 }}
            tick={{
              ...axisTickStyle,
              ...(showAllXAxisTicks &&
                xAxisTickWidth && { width: xAxisTickWidth }),
            }}
            tickMargin={10}
            tickFormatter={getCategoryLabel(dataSetCategories)}
            type="category"
            unit={axes.major.unit}
          />

          <Tooltip
            content={
              <CustomTooltip
                dataSetCategories={dataSetCategories}
                dataSetCategoryConfigs={dataSetCategoryConfigs}
                order={stacked ? 'reverse' : 'default'}
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
                      position: 'top',
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

          {majorAxisReferenceLines.map(referenceLine =>
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

          {minorAxisReferenceLines.map(referenceLine =>
            createReferenceLine({
              axis: 'y',
              axisDomain: minorDomainTicks.domain,
              axisType: 'minor',
              chartBottomMargin,
              chartData,
              chartInnerHeight: height - (xAxisHeight ?? 0),
              label: referenceLine.label,
              labelWidth: referenceLine.labelWidth,
              otherAxisDomain: majorDomainTicks.domain,
              otherAxisEnd: referenceLine.otherAxisEnd,
              otherAxisPosition: referenceLine.otherAxisPosition,
              otherAxisStart: referenceLine.otherAxisStart,
              perpendicularLine: referenceLine.perpendicularLine,
              position: referenceLine.perpendicularLine
                ? referenceLine.otherAxisPosition
                : referenceLine.position,
              style: referenceLine.style,
              y: referenceLine.perpendicularLine
                ? referenceLine.otherAxisPosition
                : referenceLine.position,
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
    canIncludeNonNumericData: true,
    canPositionLegendInline: false,
    canSetBarThickness: true,
    canSetDataLabelColour: false,
    canSetDataLabelPosition: false,
    canShowDataLabels: true,
    canShowAllMajorAxisTicks: true,
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
      axis: 'x',
      id: 'major',
      title: 'X Axis (major axis)',
      type: 'major',
      defaults: {
        groupBy: 'timePeriod',
        groupByFilter: '',
        min: 0,
        size: 50,
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
      defaults: {
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
