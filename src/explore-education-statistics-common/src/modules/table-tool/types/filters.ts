import {
  FilterOption,
  IndicatorOption,
  TimePeriodOption,
} from '@common/modules/table-tool/services/tableBuilderService';
import camelCase from 'lodash/camelCase';

export abstract class Filter {
  public readonly value: string;

  public readonly label: string;

  public readonly filterGroup?: string;

  protected constructor({ value, label, filterGroup }: FilterOption) {
    this.value = value;
    this.label = label;
    this.filterGroup = filterGroup;
  }

  public get id() {
    return this.value;
  }
}

export class CategoryFilter extends Filter {
  public readonly isTotal: boolean;

  public constructor(
    { value, label, filterGroup }: FilterOption,
    isTotal = false,
  ) {
    super({ value, label, filterGroup });
    this.isTotal = isTotal;
  }
}

export class LocationFilter extends Filter {
  public readonly level: string;

  public constructor(
    { value, label, filterGroup }: FilterOption,
    level: string,
  ) {
    super({ value, label, filterGroup });
    this.level = camelCase(level);
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
    filterGroup,
    name,
    decimalPlaces,
  }: IndicatorOption) {
    super({ value, label, filterGroup });
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
