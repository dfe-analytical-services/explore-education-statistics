import SchoolType from 'src/services/types/SchoolType';

export interface FilterOption {
  label: string;
  value: string;
}

export interface GroupedFilterOptions {
  [name: string]: {
    label: string;
    options: FilterOption[];
  };
}

export enum LocationLevel {
  National = 'national',
  Region = 'region',
  Local_Authority = 'localAuthority',
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
    startEndDate: {
      hint?: string;
      legend: string;
      options: FilterOption[];
    };
  };
  categoricalFilters: {
    [key: string]: {
      legend: string;
      hint?: string;
      options: FilterOption[] | GroupedFilterOptions;
    };
  };
  indicators: GroupedFilterOptions;
}

const metaSpecification: MetaSpecification = {
  categoricalFilters: {
    characteristics: {
      hint: 'Filter by pupil characteristics',
      legend: 'Characteristics',
      options: {},
    },
    schoolTypes: {
      hint: 'Filter by number of pupils in school type(s)',
      legend: 'School type',
      options: [
        {
          label: 'Total',
          value: SchoolType.Total,
        },
        {
          label: 'Primary',
          value: SchoolType.State_Funded_Primary,
        },
        {
          label: 'Secondary',
          value: SchoolType.State_Funded_Secondary,
        },
        {
          label: 'Special',
          value: SchoolType.Special,
        },
      ],
    },
  },
  indicators: {},
  observationalUnits: {
    location: {
      hint: 'Filter statistics by location level',
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
    startEndDate: {
      hint: 'Filter statistics by a given start and end date',
      legend: 'Academic Year',
      options: [
        { value: '2011', label: '2011/12' },
        { value: '2012', label: '2012/13' },
        { value: '2013', label: '2013/14' },
        { value: '2014', label: '2014/15' },
        { value: '2015', label: '2015/16' },
        { value: '2016', label: '2016/17' },
      ],
    },
  },
};

export default metaSpecification;
