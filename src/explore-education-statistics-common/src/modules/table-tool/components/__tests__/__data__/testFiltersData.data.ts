import {
  Subject,
  SubjectMeta,
  SubjectMetaFilterHierarchy,
} from '@common/services/tableBuilderService';
import { OptionLabelsMap } from '../../utils/getFilterHierarchyLabelsMap';

export const testSubjectMeta: SubjectMeta = {
  filters: {
    SchoolType: {
      id: 'school-type',
      autoSelectFilterItemId: '',
      hint: 'Filter by school type',
      legend: 'School type',
      options: {
        Default: {
          id: 'default',
          label: 'Default',
          options: [
            {
              label: 'State-funded secondary',
              value: 'state-funded-secondary',
            },
            {
              label: 'Special',
              value: 'special',
            },
          ],
          order: 0,
        },
      },
      name: 'school_type',
      order: 0,
    },
    Characteristic: {
      id: 'characteristic',
      autoSelectFilterItemId: '',
      hint: 'Filter by pupil characteristic',
      legend: 'Characteristic',
      options: {
        EthnicGroupMajor: {
          id: 'ethnic-group-major',
          label: 'Ethnic group major',
          options: [
            {
              label: 'Ethnicity Major Black Total',
              value: 'ethnicity-major-black-total',
            },
            {
              label: 'Ethnicity Major Mixed Total',
              value: 'ethnicity-major-mixed-total',
            },
            {
              label: 'Ethnicity Major Asian Total',
              value: 'ethnicity-major-asian-total',
            },
          ],
          order: 0,
        },
        Gender: {
          id: 'Gender',
          label: 'Gender',
          options: [
            {
              label: 'Gender female',
              value: 'gender-female',
            },
            {
              label: 'Gender male',
              value: 'gender-male',
            },
          ],
          order: 1,
        },
        Total: {
          id: 'total',
          label: 'Total',
          options: [
            {
              label: 'Total',
              value: 'total',
            },
          ],
          order: 2,
        },
      },
      name: 'characteristic',
      order: 1,
    },
  },
  indicators: {
    AbsenceByReason: {
      id: 'absence-by-reason',
      label: 'Absence by reason',
      options: [
        {
          label: 'Number of excluded sessions',
          unit: '',
          value: 'number-excluded-sessions',
          name: 'sess_auth_excluded',
        },
        {
          label: 'Number of unauthorised reasons sessions',
          unit: '',
          value: 'number-unauthorised-reasons-sessions',
          name: 'sess_unauth_totalreasons',
        },
      ],
      order: 0,
    },
    AbsenceFields: {
      id: 'absence-fields',
      label: 'Absence fields',
      options: [
        {
          label: 'Authorised absence rate',
          unit: '%',
          value: 'authorised-absence-rate',
          name: 'sess_authorised_percent',
        },
        {
          label: 'Number of overall absence sessions',
          unit: '',
          value: 'number-overall-absence-sessions',
          name: 'sess_overall',
        },
        {
          label: 'Unauthorised absence rate',
          unit: '%',
          value: 'unauthorised-absence-rate',
          name: 'sess_unauthorised_percent',
        },
      ],
      order: 1,
    },
  },
  locations: {},
  timePeriod: {
    hint: '',
    legend: '',
    options: [],
  },
};

export const testSubjectMetaSingleFilters: SubjectMeta = {
  ...testSubjectMeta,
  filters: {
    Characteristic: {
      id: 'characteristic',
      autoSelectFilterItemId: '',
      hint: 'Filter by pupil characteristic',
      legend: 'Characteristic',
      options: {
        EthnicGroupMajor: {
          id: 'ethnic-group-major',
          label: 'Ethnic group major',
          options: [
            {
              label: 'Ethnicity Major Black Total',
              value: 'ethnicity-major-black-total',
            },
          ],
          order: 0,
        },
      },
      name: 'characteristic',
      order: 0,
    },
    SchoolType: {
      id: 'school-type',
      autoSelectFilterItemId: '',
      hint: 'Filter by school type',
      legend: 'School type',
      options: {
        Default: {
          id: 'default',
          label: 'Default',
          options: [
            {
              label: 'State-funded secondary',
              value: 'state-funded-secondary',
            },
          ],
          order: 0,
        },
      },
      name: 'school_type',
      order: 1,
    },
    FilterWithMultipleOptions: {
      id: 'filter-with-multiple-options',
      autoSelectFilterItemId: '',
      hint: 'Filter by Filter With Multiple Options',
      legend: 'Filter With Multiple Options',
      options: {
        OptionGroup1: {
          id: 'option-group-1',
          label: 'Option group 1',
          options: [
            {
              label: 'Option group 1 option 1',
              value: 'option-group-1-option-1',
            },
          ],
          order: 0,
        },
        OptionGroup2: {
          id: 'option-group-2',
          label: 'Option group 2',
          options: [
            {
              label: 'Option group 2 option 1',
              value: 'option-group-2-option-1',
            },
            {
              label: 'Option group 2 option 2',
              value: 'option-group-2-option-2',
            },
          ],
          order: 1,
        },
      },
      name: 'characteristic',
      order: 2,
    },
  },
};

export const testSubjectMetaOneIndicator: SubjectMeta = {
  ...testSubjectMeta,
  indicators: {
    AbsenceByReason: {
      id: 'absence-by-reason',
      label: 'Absence by reason',
      options: [
        {
          label: 'Number of excluded sessions',
          unit: '',
          value: 'number-excluded-sessions',
          name: 'sess_auth_excluded',
        },
      ],
      order: 0,
    },
  },
};

export const testSubject = {
  id: 'subject-1',
  name: 'Subject 1',
  file: {
    id: 'file-1',
    fileName: 'File 1',
    extension: 'csv',
    name: 'File 1',
    size: '100mb',
    type: 'Data',
  },
} as Subject;

export const testSubjectMetaWithFilterHierarchy: SubjectMeta = {
  filters: {
    LevelOfQualification: {
      id: 'filter-1',
      legend: 'Level of qualification',
      options: {
        Default: {
          id: 'id-1',
          label: 'Default',
          options: [
            {
              label: 'Total',
              value: 'option-5',
            },
            {
              label: 'Entry level',
              value: 'option-1',
            },
            {
              label: 'Higher',
              value: 'option-6',
            },
          ],
          order: 0,
        },
      },
      name: 'qualification_level',
      autoSelectFilterItemId: 'option-5',
      order: 0,
    },
    NameOfCourseBeingStudied: {
      id: 'filter-3',
      legend: 'Name of course being studied',
      options: {
        Default: {
          id: 'id-2',
          label: 'Default',
          options: [
            {
              label: 'Total',
              value: 'option-11',
            },
            {
              label: 'Biochemistry',
              value: 'option-21',
            },
            {
              label: 'Bricklaying',
              value: 'option-20',
            },
            {
              label: 'Bricklaying01',
              value: 'option-19',
            },
            {
              label: 'Bricklaying02',
              value: 'option-18',
            },
            {
              label: 'Bricklaying03',
              value: 'option-15',
            },
            {
              label: 'Bricklaying04',
              value: 'option-14',
            },
            {
              label: 'Bricklaying05',
              value: 'option-12',
            },
            {
              label: 'Bricklaying06',
              value: 'option-17',
            },
            {
              label: 'Bricklaying07',
              value: 'option-16',
            },
            {
              label: 'Civil engineering',
              value: 'option-10',
            },
            {
              label: 'Electrical engineering',
              value: 'option-9',
            },
            {
              label: 'Forestry',
              value: 'option-24',
            },
            {
              label: 'Hedge rows',
              value: 'option-23',
            },
            {
              label: 'Physics',
              value: 'option-22',
            },
            {
              label: 'Plastering',
              value: 'option-13',
            },
          ],
          order: 0,
        },
      },
      name: 'course_title',
      autoSelectFilterItemId: 'option-11',
      order: 1,
    },
    SectorSubjectArea: {
      id: 'filter-2',
      legend: 'Sector subject area',
      options: {
        Default: {
          id: 'id-3',
          label: 'Default',
          options: [
            {
              label: 'Total',
              value: 'option-4',
            },
            {
              label: 'Construction',
              value: 'option-7',
            },
            {
              label: 'Engineering',
              value: 'option-2',
            },
            {
              label: 'Horticulture',
              value: 'option-8',
            },
            {
              label: 'Science',
              value: 'option-3',
            },
          ],
          order: 0,
        },
      },
      name: 'subject_area',
      autoSelectFilterItemId: 'option-4',
      order: 2,
    },
  },
  indicators: {
    Default: {
      id: 'indicator-group-1',
      label: 'Default',
      options: [
        {
          label: 'Number of students enrolled',
          unit: '',
          value: 'indicator-1',
          name: 'enrollment_count',
        },
      ],
      order: 0,
    },
  },
  locations: {},
  timePeriod: {
    hint: '',
    legend: '',
    options: [],
  },
  filterHierarchies: [
    [
      {
        level: 0,
        filterId: 'filter-1',
        childFilterId: 'filter-2',
        hierarchy: {
          'option-1': ['option-2', 'option-3', 'option-4'],
          'option-5': ['option-4'],
          'option-6': ['option-7', 'option-4', 'option-8'],
        },
      },
      {
        level: 1,
        filterId: 'filter-2',
        childFilterId: 'filter-3',
        hierarchy: {
          'option-2': ['option-9', 'option-10', 'option-11'],
          'option-7': [
            'option-12',
            'option-13',
            'option-14',
            'option-15',
            'option-16',
            'option-17',
            'option-11',
            'option-18',
            'option-19',
            'option-20',
          ],
          'option-3': ['option-21', 'option-22', 'option-11'],
          'option-4': ['option-11'],
          'option-8': ['option-23', 'option-24', 'option-11'],
        },
      },
    ],
  ],
};

export const testOptionsLabelsMap: OptionLabelsMap = {
  'filter-1': 'Level of qualification',
  'id-1': 'Default',
  'option-5': 'Total',
  'option-1': 'Entry level',
  'option-6': 'Higher',
  'filter-3': 'Name of course being studied',
  'id-2': 'Default',
  'option-11': 'Total',
  'option-21': 'Biochemistry',
  'option-20': 'Bricklaying',
  'option-19': 'Bricklaying01',
  'option-18': 'Bricklaying02',
  'option-15': 'Bricklaying03',
  'option-14': 'Bricklaying04',
  'option-12': 'Bricklaying05',
  'option-17': 'Bricklaying06',
  'option-16': 'Bricklaying07',
  'option-10': 'Civil engineering',
  'option-9': 'Electrical engineering',
  'option-24': 'Forestry',
  'option-23': 'Hedge rows',
  'option-22': 'Physics',
  'option-13': 'Plastering',
  'filter-2': 'Sector subject area',
  'id-3': 'Default',
  'option-4': 'Total',
  'option-7': 'Construction',
  'option-2': 'Engineering',
  'option-8': 'Horticulture',
  'option-3': 'Science',
};

export const testFilterHierarchy: SubjectMetaFilterHierarchy = [
  {
    level: 0,
    filterId: 'filter-1',
    childFilterId: 'filter-2',
    hierarchy: {
      'option-1': ['option-2', 'option-3', 'option-4'],
      'option-5': ['option-4'],
      'option-6': ['option-7', 'option-4', 'option-8'],
    },
  },
  {
    level: 1,
    filterId: 'filter-2',
    childFilterId: 'filter-3',
    hierarchy: {
      'option-2': ['option-9', 'option-10', 'option-11'],
      'option-7': [
        'option-12',
        'option-13',
        'option-14',
        'option-15',
        'option-16',
        'option-17',
        'option-11',
        'option-18',
        'option-19',
        'option-20',
      ],
      'option-3': ['option-21', 'option-22', 'option-11'],
      'option-4': ['option-11'],
      'option-8': ['option-23', 'option-24', 'option-11'],
    },
  },
];
