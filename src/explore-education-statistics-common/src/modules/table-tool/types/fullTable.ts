import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import {
  BoundaryLevel,
  TableDataResult,
} from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';

export interface FullTableMeta {
  publicationName: string;
  subjectName: string;
  locations: LocationFilter[];
  timePeriodRange: TimePeriodFilter[];
  filters: Dictionary<{
    legend: string;
    name: string;
    options: CategoryFilter[];
    order: number;
  }>;
  indicators: Indicator[];
  boundaryLevels: BoundaryLevel[];
  footnotes: {
    id: string;
    label: string;
  }[];
  geoJsonAvailable: boolean;
  isCroppedTable: boolean;
}

export interface FullTable {
  title?: string;
  subjectMeta: FullTableMeta;
  results: TableDataResult[];
}
