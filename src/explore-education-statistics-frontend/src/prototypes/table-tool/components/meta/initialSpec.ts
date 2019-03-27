import { SelectOption } from 'src/components/form/FormSelect';
import SchoolType from 'src/services/types/SchoolType';

export interface FilterOption {
  label: string;
  name: string;
}

export interface GroupedFilterOptions {
  [name: string]: FilterOption[];
}

interface PublicationSubjectOption {
  name: string;
  label: string;
  supports: {
    observationalUnits: {
      location: (keyof MetaSpecification['observationalUnits']['location'])[];
    };
  };
}

export interface MetaSpecification {
  observationalUnits: {
    location: {
      national: SelectOption[];
      localAuthority: SelectOption[];
      region: SelectOption[];
      school: SelectOption[];
    };
    startEndDate: {
      hint?: string;
      legend: string;
      options: SelectOption[];
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
  publicationSubject: PublicationSubjectOption[];
}

const metaSpecification: MetaSpecification = {
  categoricalFilters: {
    characteristics: {
      hint: 'Filter by pupil characteristics',
      legend: 'Characteristics',
      options: [],
    },
    schoolTypes: {
      hint: 'Filter by number of pupils in school type(s)',
      legend: 'School type',
      options: [
        {
          label: 'Total',
          name: SchoolType.Total,
        },
        {
          label: 'Primary',
          name: SchoolType.State_Funded_Primary,
        },
        {
          label: 'Secondary',
          name: SchoolType.State_Funded_Secondary,
        },
        {
          label: 'Special',
          name: SchoolType.Special,
        },
      ],
    },
  },
  indicators: {},
  observationalUnits: {
    location: {
      localAuthority: [
        { value: 'CAMDEN', text: 'Camden' },
        { value: 'CITY_OF_LONDON', text: 'City of London' },
        { value: 'GREENWICH', text: 'Greenwich' },
      ],
      national: [{ value: 'ENGLAND', text: 'England' }],
      region: [
        { value: 'INNER_LONDON', text: 'Inner London' },
        { value: 'OUTER_LONDON', text: 'Outer London' },
      ],
      school: [],
    },
    startEndDate: {
      hint: 'Filter statistics by a given start and end date',
      legend: 'Academic Year',
      options: [
        { value: 2011, text: '2011/12' },
        { value: 2012, text: '2012/13' },
        { value: 2013, text: '2013/14' },
        { value: 2014, text: '2014/15' },
        { value: 2015, text: '2015/16' },
        { value: 2016, text: '2016/17' },
      ],
    },
  },
  publicationSubject: [
    {
      label: 'Geographic levels',
      name: 'geoglevels',
      supports: {
        observationalUnits: {
          location: ['national', 'localAuthority', 'region', 'school'],
        },
      },
    },
    {
      label: 'Local authority characteristics',
      name: 'lacharacteristics',
      supports: {
        observationalUnits: { location: ['localAuthority'] },
      },
    },
    {
      label: 'National characteristics',
      name: 'natcharacteristics',
      supports: {
        observationalUnits: {
          location: ['national'],
        },
      },
    },
  ],
};

export default metaSpecification;
