import {
  FilterOption,
  IndicatorOption,
  TimePeriodOption,
} from '@common/modules/full-table/services/tableBuilderService';
import camelCase from 'lodash/camelCase';

interface ToJSONFilter {
  _class: string;
  _construct: string;
}

export abstract class Filter {
  public readonly value: string;

  public readonly label: string;

  public constructor({ value, label }: FilterOption) {
    this.value = value;
    this.label = label;
  }

  public toJSON(): ToJSONFilter {
    return {
      _class: 'Filter',
      _construct: JSON.stringify([{ value: this.value, label: this.label }]),
    };
  }
}

export class CategoryFilter extends Filter {
  public readonly isTotal: boolean;

  public constructor({ value, label }: FilterOption, isTotal = false) {
    super({ value, label });
    this.isTotal = isTotal;
  }

  public toJSON() {
    return {
      _class: 'CategoryFilter',
      _construct: JSON.stringify([
        { value: this.value, label: this.label },
        this.isTotal,
      ]),
    };
  }
}

export class LocationFilter extends Filter {
  public readonly level: string;

  public constructor({ value, label }: FilterOption, level: string) {
    super({ value, label });
    this.level = camelCase(level);
  }

  public toJSON() {
    return {
      _class: 'LocationFilter',
      _construct: JSON.stringify([
        { value: this.value, label: this.label },
        this.level,
      ]),
    };
  }
}

export class Indicator extends Filter {
  public readonly unit: string;

  public constructor({ value, label, unit }: IndicatorOption) {
    super({ value, label });
    this.unit = unit;
  }

  public toJSON() {
    return {
      _class: 'Indicator',
      _construct: JSON.stringify([
        { value: this.value, label: this.label, unit: this.unit },
      ]),
    };
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

  public toJSON() {
    return {
      _class: 'TimePeriodFilter',
      _construct: JSON.stringify([
        {
          year: this.year,
          code: this.code,
          label: this.label,
        },
      ]),
    };
  }
}
