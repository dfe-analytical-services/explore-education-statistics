import { SelectOption } from 'src/components/form/FormSelect';
import { PublicationSubjectOption } from 'src/modules/table-tool/components/PublicationSubjectMenu';
import SchoolType from 'src/services/types/SchoolType';

export interface FilterOption {
  label: string;
  name: string;
}

export interface GroupedFilterOptions {
  [name: string]: FilterOption[];
}

export interface MetaSpecification {
  observationalUnits: {
    country: SelectOption[];
    localAuthority: SelectOption[];
    region: SelectOption[];
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
    country: [{ value: 'ENGLAND', text: 'England' }],
    localAuthority: [
      { value: 'CAMDEN', text: 'Camden' },
      { value: 'CITY_OF_LONDON', text: 'City of London' },
      { value: 'GREENWICH', text: 'Greenwich' },
    ],
    region: [
      { value: 'INNER_LONDON', text: 'Inner London' },
      { value: 'OUTER_LONDON', text: 'Outer London' },
    ],
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
      label: 'National characteristics',
      name: 'natcharacteristics',
    },
    {
      label: 'Local authority characteristics',
      name: 'lacharacteristics',
    },
    {
      label: 'Geographic levels',
      name: 'geoglevels',
    },
  ],
};

export default metaSpecification;
