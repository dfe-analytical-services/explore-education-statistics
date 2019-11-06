import {
  Footnote,
  FootnoteMeta,
} from '@admin/services/release/edit-release/footnotes/types';

export const dummyFootnoteMeta: FootnoteMeta = {
  '1': {
    filters: {
      1: {
        hint: 'Filter by pupil characteristic',
        legend: 'Characteristic',
        options: {
          1: {
            label: 'Total',
            options: {
              1: {
                label: 'Total',
                value: '1',
              },
            },
          },
          2: {
            label: 'SEN primary need',
            options: {
              24: {
                label: 'SEN primary need Unclassified',
                value: '24',
              },
              25: {
                label: 'SEN primary need Visual impairment',
                value: '25',
              },
              26: {
                label: 'SEN primary need Specific learning difficulty',
                value: '26',
              },
              27: {
                label: 'SEN primary need Severe learning difficulty',
                value: '27',
              },
              28: {
                label:
                  'SEN primary need Speech language and communications needs',
                value: '28',
              },
              29: {
                label: 'SEN primary need Social emotional and mental health',
                value: '29',
              },
              30: {
                label:
                  'SEN primary need Profound and multiple learning difficulty',
                value: '30',
              },
              31: {
                label: 'SEN primary need Physical disability',
                value: '31',
              },
              32: {
                label: 'SEN primary need other difficulty/disability',
                value: '32',
              },
              33: {
                label: 'SEN primary need No specialist assessment',
                value: '33',
              },
              34: {
                label: 'SEN primary need Multi-sensory impairment',
                value: '34',
              },
              35: {
                label: 'SEN primary need Moderate learning difficulty',
                value: '35',
              },
              36: {
                label: 'SEN primary need Hearing impairment',
                value: '36',
              },
              37: {
                label: 'SEN primary need Autistic spectrum disorder',
                value: '37',
              },
            },
          },
          3: {
            label: 'SEN provision',
            options: {
              38: {
                label: 'SEN provision School Action Plus',
                value: '38',
              },
              2: {
                label: 'SEN provision SEN Support',
                value: '2',
              },
              3: {
                label: 'SEN provision statement or EHCP',
                value: '3',
              },
              4: {
                label: 'SEN provision No identified SEN',
                value: '4',
              },
              9: {
                label: 'SEN provision Unclassified',
                value: '9',
              },
              19: {
                label: 'SEN provision statement',
                value: '19',
              },
              20: {
                label: 'SEN provision SEN without statement',
                value: '20',
              },
              21: {
                label: 'SEN provision School Action',
                value: '21',
              },
            },
          },
          4: {
            label: 'FSM ever 6',
            options: {
              5: {
                label: 'FSM unclassified in last 6 years',
                value: '5',
              },
              6: {
                label: 'FSM not eligible in last 6 years',
                value: '6',
              },
              7: {
                label: 'FSM eligible in last 6 years',
                value: '7',
              },
            },
          },
          5: {
            label: 'FSM',
            options: {
              8: {
                label: 'FSM unclassified',
                value: '8',
              },
              10: {
                label: 'FSM not eligible',
                value: '10',
              },
              18: {
                label: 'FSM eligible',
                value: '18',
              },
            },
          },
          6: {
            label: 'NC year',
            options: {
              41: {
                label: 'NC Year 6',
                value: '41',
              },
              42: {
                label: 'NC Year 4',
                value: '42',
              },
              80: {
                label: 'NC Year 5',
                value: '80',
              },
              11: {
                label: 'NC Year Unclassified',
                value: '11',
              },
              12: {
                label: 'NC Year not followed or missing',
                value: '12',
              },
              13: {
                label: 'NC Year 12 and above',
                value: '13',
              },
              14: {
                label: 'NC Year 11',
                value: '14',
              },
              15: {
                label: 'NC Year 10',
                value: '15',
              },
              16: {
                label: 'NC Year 9',
                value: '16',
              },
              17: {
                label: 'NC Year 8',
                value: '17',
              },
              39: {
                label: 'NC Year 7',
                value: '39',
              },
              63: {
                label: 'NC Year 3',
                value: '63',
              },
              64: {
                label: 'NC Year 2',
                value: '64',
              },
              65: {
                label: 'NC Year 1 and below',
                value: '65',
              },
            },
          },
          7: {
            label: 'First language',
            options: {
              22: {
                label:
                  'First language Known or believed to be other than English',
                value: '22',
              },
              23: {
                label: 'First language Known or believed to be English',
                value: '23',
              },
              40: {
                label: 'First First language Unclassified',
                value: '40',
              },
              81: {
                label: 'First language Unclassified',
                value: '81',
              },
            },
          },
          8: {
            label: 'Ethnic group major',
            options: {
              59: {
                label: 'Ethnicity Major White Total',
                value: '59',
              },
              44: {
                label: 'Ethnicity Major Unclassified',
                value: '44',
              },
              45: {
                label: 'Ethnicity Minority Ethnic Group',
                value: '45',
              },
              46: {
                label: 'Ethnicity Major Any other Ethnic Group',
                value: '46',
              },
              47: {
                label: 'Ethnicity Major Chinese',
                value: '47',
              },
              48: {
                label: 'Ethnicity Major Black Total',
                value: '48',
              },
              49: {
                label: 'Ethnicity Major Asian Total',
                value: '49',
              },
              51: {
                label: 'Ethnicity Major Mixed Total',
                value: '51',
              },
            },
          },
          9: {
            label: 'Gender',
            options: {
              52: {
                label: 'Gender female',
                value: '52',
              },
              53: {
                label: 'Gender male',
                value: '53',
              },
            },
          },
          10: {
            label: 'Ethnic group minor',
            options: {
              50: {
                label: 'Ethnicity Minor Irish',
                value: '50',
              },
              66: {
                label: 'Ethnicity Minor Unclassified',
                value: '66',
              },
              67: {
                label: 'Ethnicity Minor Chinese',
                value: '67',
              },
              68: {
                label: 'Ethnicity Minor Any other Ethnic Group',
                value: '68',
              },
              69: {
                label: 'Ethnicity Minor Any other Black background',
                value: '69',
              },
              70: {
                label: 'Ethnicity Minor Black African',
                value: '70',
              },
              71: {
                label: 'Ethnicity Minor Black Caribbean',
                value: '71',
              },
              72: {
                label: 'Ethnicity Minor Any other Asian background',
                value: '72',
              },
              73: {
                label: 'Ethnicity Minor Bangladeshi',
                value: '73',
              },
              74: {
                label: 'Ethnicity Minor Pakistani',
                value: '74',
              },
              75: {
                label: 'Ethnicity Minor Indian',
                value: '75',
              },
              76: {
                label: 'Ethnicity Minor Any other Mixed background',
                value: '76',
              },
              77: {
                label: 'Ethnicity Minor White and Asian',
                value: '77',
              },
              78: {
                label: 'Ethnicity Minor White and Black African',
                value: '78',
              },
              79: {
                label: 'Ethnicity Minor White and Black Caribbean',
                value: '79',
              },
              60: {
                label: 'Ethnicity Minor Traveller of Irish heritage',
                value: '60',
              },
              61: {
                label: 'Ethnicity Minor Gypsy Roma',
                value: '61',
              },
              62: {
                label: 'Ethnicity Minor Any other White background',
                value: '62',
              },
              43: {
                label: 'Ethnicity Minor White British',
                value: '43',
              },
            },
          },
        },
      },
      2: {
        hint: 'Filter by school type',
        legend: 'School type',
        options: {
          11: {
            label: 'Default',
            options: {
              55: {
                label: 'Special',
                value: '55',
              },
              56: {
                label: 'State-funded secondary',
                value: '56',
              },
              57: {
                label: 'State-funded primary',
                value: '57',
              },
              58: {
                label: 'Total',
                value: '58',
              },
            },
          },
        },
      },
      3: {
        hint: 'Filter by the length of time the data was collected',
        legend: 'Year breakdown',
        options: {
          12: {
            label: 'Default',
            options: {
              54: {
                label: 'six half terms',
                value: '54',
              },
            },
          },
        },
      },
    },
    indicators: {
      1: {
        label: 'Absence fields',
        options: {
          1: {
            label: 'Number of pupil enrolments',
            unit: '',
            value: '1',
          },
          23: {
            label: 'Unauthorised absence rate',
            unit: '%',
            value: '23',
          },
          24: {
            label: 'Number of unauthorised absence sessions',
            unit: '',
            value: '24',
          },
          25: {
            label: 'Number of sessions possible',
            unit: '',
            value: '25',
          },
          26: {
            label: 'Overall absence rate',
            unit: '%',
            value: '26',
          },
          27: {
            label: 'Number of overall absence sessions',
            unit: '',
            value: '27',
          },
          28: {
            label: 'Authorised absence rate',
            unit: '%',
            value: '28',
          },
          29: {
            label: 'Number of authorised absence sessions',
            unit: '',
            value: '29',
          },
          30: {
            label: 'Percentage of persistent absentees',
            unit: '%',
            value: '30',
          },
          31: {
            label: 'Number of persistent absentees',
            unit: '',
            value: '31',
          },
        },
      },
      2: {
        label: 'Absence by reason',
        options: {
          32: {
            label: 'Number of authorised other sessions',
            unit: '',
            value: '32',
          },
          17: {
            label: 'Number of religious observance sessions',
            unit: '',
            value: '17',
          },
          18: {
            label: 'Number of illness sessions',
            unit: '',
            value: '18',
          },
          19: {
            label: 'Number of authorised holiday sessions',
            unit: '',
            value: '19',
          },
          20: {
            label: 'Number of extended authorised holiday sessions',
            unit: '',
            value: '20',
          },
          21: {
            label: 'Number of excluded sessions',
            unit: '',
            value: '21',
          },
          22: {
            label: 'Number of medical appointments sessions',
            unit: '',
            value: '22',
          },
          7: {
            label: 'Number of unauthorised reasons sessions',
            unit: '',
            value: '7',
          },
          8: {
            label: 'Number of unauthorised other sessions',
            unit: '',
            value: '8',
          },
          9: {
            label: 'Number of no reason yet sessions',
            unit: '',
            value: '9',
          },
          10: {
            label: 'Number of late sessions',
            unit: '',
            value: '10',
          },
          11: {
            label: 'Number of unauthorised holiday sessions',
            unit: '',
            value: '11',
          },
          12: {
            label: 'Number of overall reasons sessions',
            unit: '',
            value: '12',
          },
          13: {
            label: 'Number of traveller sessions',
            unit: '',
            value: '13',
          },
          14: {
            label: 'Number of authorised reasons sessions',
            unit: '',
            value: '14',
          },
          15: {
            label: 'Number of study leave sessions',
            unit: '',
            value: '15',
          },
        },
      },
      3: {
        label: 'Absence for persistent absentees',
        options: {
          16: {
            label:
              'Number of unauthorised absence sessions for persistent absentees',
            unit: '',
            value: '16',
          },
          2: {
            label: 'Number of sessions possible for persistent absentees',
            unit: '',
            value: '2',
          },
          3: {
            label: 'Overall absence rate exact',
            unit: '%',
            value: '3',
          },
          4: {
            label:
              'Number of overall absence sessions for persistent absentees',
            unit: '',
            value: '4',
          },
          5: {
            label: 'Authorised absence rate exact',
            unit: '%',
            value: '5',
          },
          6: {
            label:
              'Number of authorised absence sessions for persistent absentees',
            unit: '',
            value: '6',
          },
          33: {
            label: 'Unauthorised absence rate exact',
            unit: '%',
            value: '33',
          },
        },
      },
    },
    subjectId: 1,
    subjectName: 'Absence by characteristic',
  },
};

export const dummyFootnotes: Footnote[] = [
  {
    id: 1,
    content:
      'State-funded primary schools include all primary academies, including free schools.',
    indicators: [],
    filters: [],
    filterGroups: [],
    filterItems: [57, 83, 88, 97, 101, 180],
    subjects: [1, 2, 3, 4, 5, 6, 7],
  },
  {
    id: 2,
    content:
      'State-funded secondary schools include city technology colleges and all secondary academies, including all-through academies and free schools.',
    indicators: [],
    filters: [],
    filterGroups: [],
    filterItems: [56, 84, 89, 95, 102, 179],
    subjects: [1, 2, 3, 4, 5, 6, 7],
  },
  {
    id: 3,
    content:
      'Special schools include maintained special schools, non-maintained special schools and special academies.  Excludes general hospital schools, independent special schools and independent schools approved for SEN pupils.',
    indicators: [],
    filters: [],
    filterGroups: [],
    filterItems: [55, 85, 90, 96, 103, 178],
    subjects: [1, 2, 3, 4, 5, 6, 7],
  },
  {
    id: 4,
    content:
      'Totals may not appear to equal the sum of component parts because numbers have been rounded to the nearest 5.',
    indicators: [],
    filters: [],
    filterGroups: [],
    filterItems: [],
    subjects: [1, 2, 3, 4, 5, 6, 7],
  },
  {
    id: 5,
    content:
      'x - 1 or 2 enrolments, or a percentage based on 1 or 2 enrolments.',
    indicators: [],
    filters: [],
    filterGroups: [],
    filterItems: [],
    subjects: [1, 2, 3, 4, 5, 6, 7],
  },
  {
    id: 6,
    content:
      'There may be discrepancies between totals and the sum of constituent parts  as national and regional totals and totals across school types have been rounded to the nearest 5.',
    indicators: [],
    filters: [],
    filterGroups: [],
    filterItems: [],
    subjects: [1, 2, 3, 4, 5, 6, 7],
  },
  {
    id: 7,
    content:
      'Absence rates are the number of absence sessions expressed as a percentage of the total number of possible sessions.',
    indicators: [23, 26, 28, 57, 59, 66, 83, 84, 85, 95, 113, 114, 115],
    filters: [],
    filterGroups: [],
    filterItems: [],
    subjects: [1, 2, 3, 4, 5, 6, 7],
  },
];
