/* eslint-disable max-classes-per-file */
import {
  FilterOption,
  GeoJsonFeature,
  IndicatorOption,
  TimePeriodOption,
} from '@common/services/tableBuilderService';

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

  /**
   * Get an identifier that guarantees
   * uniqueness for the filter somehow.
   *
   * This is unlike {@property value} where
   * duplicates are allowed (e.g. locations),
   * making it potentially unsafe for equality
   * checks with other filters of the same type.
   */
  public get id(): string {
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

export interface LocationCompositeId {
  level: string;
  value: string;
}

export class LocationFilter extends Filter {
  public readonly code: string;

  public readonly level: string;

  public readonly geoJson?: GeoJsonFeature[];

  public constructor({
    id,
    value,
    label,
    level,
    geoJson,
    group,
  }: GroupedFilterOption & {
    id?: string;
    level: string;
    geoJson?: GeoJsonFeature[];
  }) {
    // Fallback to using the code if there's no id.
    // This is the case for historical Permalinks created prior to EES-2955.
    const idOrFallback = id ?? value;
    super({ value: idOrFallback, label, group });
    // Always set the code so that it can be included in the downloadable CSV file.
    this.code = value;
    this.level = level;
    this.geoJson = geoJson;
  }

  public static createId(params: { value: string; level: string }): string {
    return JSON.stringify({
      level: params.level,
      value: params.value,
    });
  }

  public static parseCompositeId(id: string): LocationCompositeId {
    const { level, value } = JSON.parse(id);

    if (typeof level !== 'string' || typeof value !== 'string') {
      throw new Error(`Could not parse invalid location composite id: ${id}`);
    }

    return {
      level,
      value,
    };
  }

  public get id(): string {
    return LocationFilter.createId({
      value: this.value,
      level: this.level,
    });
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

  public readonly order: number;

  public constructor({
    year,
    code,
    label,
    order,
  }: TimePeriodOption & { order: number }) {
    super({ label, value: `${year}_${code}` });

    this.code = code;
    this.year = year;
    this.order = order;
  }
}
