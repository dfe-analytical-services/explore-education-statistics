import { Dictionary } from '@common/types';
import {
  BoundaryLevel,
  GeoJsonFeature,
  TableDataResult,
} from '@common/services/tableBuilderService';

/**
 * Raw version of the FullTable type for use only in web workers.
 * When mapFullTable is refactored to not use classes (EES-2613) we can remove this.
 */
interface RawFilter {
  value: string;
  label: string;
  group?: string;
  id: string; // ids don't get serialised into the worker as they're getters so can't be used.
}

interface RawCategoryFilter extends RawFilter {
  isTotal: boolean;
  category: string;
}

interface RawLocationFilter extends RawFilter {
  level: string;
  geoJson: GeoJsonFeature;
}

interface RawTimePeriodFilter extends RawFilter {
  code: string;
  order: number;
  year: number;
}

interface RawIndicator extends RawFilter {
  decimalPlaces: number;
  name: string;
  unit: string;
}

export interface RawFullTableMeta {
  publicationName: string;
  subjectName: string;
  locations: RawLocationFilter[];
  timePeriodRange: RawTimePeriodFilter[];

  filters: Dictionary<{
    name: string;
    options: RawCategoryFilter[];
  }>;
  indicators: RawIndicator[];
  boundaryLevels: BoundaryLevel[];
  footnotes: {
    id: string;
    label: string;
  }[];
  geoJsonAvailable: boolean;
}

export interface WorkerFullTable {
  title?: string;
  subjectMeta: RawFullTableMeta;
  results: TableDataResult[];
}
