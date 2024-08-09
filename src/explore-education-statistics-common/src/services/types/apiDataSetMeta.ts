import { GeographicLevelCode } from '@common/utils/locationLevelsMap';

export interface Filter {
  id: string;
  hint: string;
  label: string;
}

export interface FilterOption {
  id: string;
  label: string;
  isAggregate?: boolean;
}

export interface GeographicLevel {
  code: GeographicLevelCode;
  label: string;
}

export interface IndicatorOption {
  id: string;
  label: string;
  unit?: string;
}

export interface LocationGroup {
  level: GeographicLevel;
}

export interface LocationOption {
  id: string;
  label: string;
  code?: string;
  oldCode?: string;
  ukprn?: string;
  urn?: string;
  laEstab?: string;
}

export interface TimePeriodOption {
  code: string;
  period: string;
  label: string;
}
