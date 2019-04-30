export interface FilterOption {
  label: string;
  value: string;
}

export interface IndicatorOption extends FilterOption {
  unit: string;
}

export interface TimePeriodOption {
  code: string;
  label: string;
  year: number;
}

export interface GroupedFilterOptions {
  [groupKey: string]: {
    label: string;
    options: FilterOption[];
  };
}

export enum LocationLevel {
  National = 'national',
  Region = 'region',
  LocalAuthority = 'localAuthority',
  School = 'school',
}

export interface MetaSpecification {
  observationalUnits: {
    location: {
      legend: string;
      hint?: string;
      options: {
        [key in LocationLevel]: {
          label: string;
          options: FilterOption[];
        }
      };
    };
    timePeriod: {
      hint?: string;
      legend: string;
      options: TimePeriodOption[];
    };
  };
  categoricalFilters: {
    [key: string]: {
      legend: string;
      hint?: string;
      options: GroupedFilterOptions;
    };
  };
  indicators: {
    [key: string]: {
      label: string;
      options: IndicatorOption[];
      unit: string;
    };
  };
}

const metaSpecification: MetaSpecification = {
  categoricalFilters: {
    characteristics: {
      hint: 'Select at least 1 characteristic',
      legend: 'Characteristics',
      options: {},
    },
    schoolTypes: {
      hint: 'Select at least 1 school type',
      legend: 'School type',
      options: {
        default: {
          label: '',
          options: [
            {
              label: 'Total',
              value: 'Total',
            },
            {
              label: 'Primary schools',
              value: 'State_Funded_Primary',
            },
            {
              label: 'Secondary schools',
              value: 'State_Funded_Secondary',
            },
            {
              label: 'Special schools',
              value: 'Special',
            },
          ],
        },
      },
    },
  },
  indicators: {},
  observationalUnits: {
    location: {
      hint: 'Select a location',
      legend: 'Location',
      options: {
        localAuthority: {
          label: 'Local authority',
          options: [
            { value: 'CAMDEN', label: 'Camden' },
            { value: 'CITY_OF_LONDON', label: 'City of London' },
            { value: 'GREENWICH', label: 'Greenwich' },
          ],
        },
        national: {
          label: 'National',
          options: [],
        },
        region: {
          label: 'Region',
          options: [
            { value: 'INNER_LONDON', label: 'Inner London' },
            { value: 'OUTER_LONDON', label: 'Outer London' },
          ],
        },
        school: {
          label: 'School',
          options: [],
        },
      },
    },
    timePeriod: {
      hint: 'Select a start and end date',
      legend: 'Academic year',
      options: [
        { code: 'AY', label: '2011/12', year: 2011 },
        { code: 'AY', label: '2012/13', year: 2012 },
        { code: 'AY', label: '2013/14', year: 2013 },
        { code: 'AY', label: '2014/15', year: 2014 },
        { code: 'AY', label: '2015/16', year: 2015 },
        { code: 'AY', label: '2016/17', year: 2016 },
      ],
    },
  },
};

export default metaSpecification;
