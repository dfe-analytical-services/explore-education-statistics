import { Dictionary } from '@common/types';
import {
  BoundaryLevel,
  GeoJsonFeature,
  TableDataResult,
} from '@common/services/tableBuilderService';

/**
 * Worker version of the FullTable type for use only in web workers.
 * When mapFullTable is refactored to not use classes (EES-2613) we can remove this.
 */
export interface WorkerFilter {
  value: string;
  label: string;
  group?: string;
  id: string; // ids don't get serialised into the worker as they're getters so can't be used.
}

export interface WorkerCategoryFilter extends WorkerFilter {
  isTotal: boolean;
  category: string;
}

export interface WorkerLocationFilter extends WorkerFilter {
  code: string;
  level: string;
  geoJson: GeoJsonFeature;
}

export interface WorkerTimePeriodFilter extends WorkerFilter {
  code: string;
  order: number;
  year: number;
}

export interface WorkerIndicator extends WorkerFilter {
  decimalPlaces: number;
  name: string;
  unit: string;
}

export interface WorkerFullTableMeta {
  publicationName: string;
  subjectName: string;
  locations: WorkerLocationFilter[];
  timePeriodRange: WorkerTimePeriodFilter[];

  filters: Dictionary<{
    name: string;
    options: WorkerCategoryFilter[];
  }>;
  indicators: WorkerIndicator[];
  boundaryLevels: BoundaryLevel[];
  footnotes: {
    id: string;
    label: string;
  }[];
  geoJsonAvailable: boolean;
}

export interface WorkerFullTable {
  title?: string;
  subjectMeta: WorkerFullTableMeta;
  results: TableDataResult[];
}
