import HorizontalBarBlock from '@common/modules/charts/components/HorizontalBarBlock';
import LineChartBlock from '@common/modules/charts/components/LineChartBlock';
import MapBlock from '@common/modules/charts/components/MapBlock';
import VerticalBarBlock from '@common/modules/charts/components/VerticalBarBlock';
import { PublicationSubjectMeta } from '@common/modules/table-tool/services/tableBuilderService';
import {
  BoundaryLevel,
  DataBlockData,
  DataBlockLocationMetadata,
  LabelValueMetadata,
  LabelValueUnitMetadata,
} from '@common/services/dataBlockService';
import {
  AxisConfiguration,
  AxisGroupBy,
  AxisType,
  ChartType,
  DataSetConfiguration,
} from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import { ReactNode } from 'react';
import { LegendProps } from 'recharts';

export interface ChartMetaData {
  filters: PublicationSubjectMeta['filters'];
  indicators: Dictionary<LabelValueUnitMetadata>;
  locations: Dictionary<DataBlockLocationMetadata>;
  boundaryLevels?: BoundaryLevel[];
  timePeriod: Dictionary<LabelValueMetadata>;
}

export interface AbstractChartProps {
  data: DataBlockData;
  meta: ChartMetaData;
  title?: string;
  height: number;
  width?: number;
}

export interface ChartProps extends AbstractChartProps {
  labels: Dictionary<DataSetConfiguration>;
  axes: Dictionary<AxisConfiguration>;
  legend?: 'none' | 'top' | 'bottom';
  /**
   * Callback to enable us to render legends outside
   * of the chart container.
   * This is a bit of a hack because no such API
   * technically exists in Recharts, however, we can get
   * around this by using the Legend's `content` prop.
   */
  renderLegend?: (props: LegendProps) => ReactNode;
}

export interface StackedBarProps extends ChartProps {
  stacked?: boolean;
}

export interface ChartCapabilities {
  hasAxes: boolean;
  dataSymbols: boolean;
  stackable: boolean;
  lineStyle: boolean;
  gridLines: boolean;
  canSize: boolean;
  fixedAxisGroupBy: boolean;
  hasReferenceLines: boolean;
  hasLegend: boolean;
}

export interface ChartDefinition {
  type: ChartType;
  name: string;
  capabilities: ChartCapabilities;
  options: {
    stacked?: boolean;
    legend?: 'none' | 'top' | 'bottom';
    height: number;
    width?: number;
    title?: string;
  };
  data: {
    type: string;
    title: string;
    entryCount: number | 'multiple';
    targetAxis: string;
  }[];
  axes: Dictionary<{
    id: string;
    title: string;
    type: AxisType;
    defaultDataType?: AxisGroupBy;
    forcedDataType?: AxisGroupBy;
  }>;

  requiresGeoJson: boolean;
}

export const chartDefinitions: ChartDefinition[] = [
  LineChartBlock.definition,
  VerticalBarBlock.definition,
  HorizontalBarBlock.definition,
  MapBlock.definition,
];
