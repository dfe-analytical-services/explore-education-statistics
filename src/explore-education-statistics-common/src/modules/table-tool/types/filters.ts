import {
  FilterOption,
  GeoJsonFeature,
  IndicatorOption,
  LocationOption,
  TimePeriodOption,
} from '@common/services/tableBuilderService';
import camelCase from 'lodash/camelCase';

interface GroupedFilterOption extends FilterOption {
  group?: string;
}

export abstract class Filter {
  public readonly value: string;

  public readonly label: string;

  public readonly group?: string;

  protected constructor({ value, label, group }: GroupedFilterOption) {
    this.value = value;
    this.label = label;
    this.group = group;
  }

  public get id() {
    return this.value;
  }
}

export class CategoryFilter extends Filter {
  public readonly isTotal: boolean;

  public readonly category: string;

  public constructor({
    value,
    label,
    group,
    isTotal = false,
    category,
  }: GroupedFilterOption & { isTotal?: boolean; category: string }) {
    super({ value, label, group });
    this.isTotal = isTotal;
    this.category = category;
  }
}

export class LocationFilter extends Filter {
  public readonly level: string;

  public readonly geoJson?: GeoJsonFeature[];

  public constructor({
    value,
    label,
    level,
    geoJson,
    group,
  }: GroupedFilterOption & LocationOption) {
    super({ value, label, group });

    this.level = camelCase(level);
    this.geoJson = geoJson;
  }

  public get id() {
    return `${this.level}_${this.value}`;
  }
}

export class Indicator extends Filter {
  public readonly unit: string;

  public readonly decimalPlaces?: number;

  public readonly name: string;

  public constructor({
    value,
    label,
    unit,
    group,
    name,
    decimalPlaces,
  }: GroupedFilterOption & IndicatorOption) {
    super({ value, label, group });

    this.unit = unit;
    this.name = name;
    this.decimalPlaces = decimalPlaces;
  }
}

export class TimePeriodFilter extends Filter {
  public readonly year: number;

  public readonly code: string;

  public constructor({ year, code, label }: TimePeriodOption) {
    super({ label, value: `${year}_${code}` });

    this.code = code;
    this.year = year;
  }
}
