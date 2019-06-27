/* eslint-disable @typescript-eslint/camelcase */
import { ChartProps } from '@common/modules/find-statistics/components/charts/ChartFunctions';
import {
  DataBlockData,
  DataBlockMetadata,
  DataBlockResponse,
  GeographicLevel,
  ResponseMetaData,
} from '@common/services/dataBlockService';

import Features from './testLocationData';

const data: DataBlockData = {
  publicationId: 'test',
  releaseDate: new Date(),
  releaseId: '1',
  subjectId: 1,
  geographicLevel: GeographicLevel.National,
  result: [
    {
      filters: ['1', '2'],
      location: {
        country: {
          country_code: 'E92000001',
          country_name: 'England',
        },
        region: {
          region_code: '',
          region_name: '',
        },
        localAuthority: {
          new_la_code: '',
          old_la_code: '',
          la_name: '',
        },
        localAuthorityDistrict: {
          sch_lad_code: '',
          sch_lad_name: '',
        },
      },
      measures: {
        '28': '5',
        '26': '10',
        '23': '3',
      },
      timeIdentifier: 'HT6',
      year: 2014,
    },
    {
      filters: ['1', '2'],
      location: {
        country: {
          country_code: 'E92000001',
          country_name: 'England',
        },
        region: {
          region_code: '',
          region_name: '',
        },
        localAuthority: {
          new_la_code: '',
          old_la_code: '',
          la_name: '',
        },
        localAuthorityDistrict: {
          sch_lad_code: '',
          sch_lad_name: '',
        },
      },
      measures: {
        '28': '5',
        '26': '10',
        '23': '3',
      },
      timeIdentifier: 'HT6',
      year: 2015,
    },
  ],
};

const multipleData: DataBlockData = {
  publicationId: 'test',
  releaseDate: new Date(),
  releaseId: '1',
  subjectId: 1,
  geographicLevel: GeographicLevel.National,
  result: [
    {
      filters: ['1', '2'],
      location: {
        country: {
          country_code: 'E92000001',
          country_name: 'England',
        },
        region: {
          region_code: '',
          region_name: '',
        },
        localAuthority: {
          new_la_code: '',
          old_la_code: '',
          la_name: '',
        },
        localAuthorityDistrict: {
          sch_lad_code: '',
          sch_lad_name: '',
        },
      },
      measures: {
        '28': '5',
        '26': '10',
        '23': '3',
      },
      timeIdentifier: 'HT6',
      year: 2015,
    },
    {
      filters: ['1', '2'],
      location: {
        country: {
          country_code: 'S92000001',
          country_name: 'Scotland',
        },
        region: {
          region_code: '',
          region_name: '',
        },
        localAuthority: {
          new_la_code: '',
          old_la_code: '',
          la_name: '',
        },
        localAuthorityDistrict: {
          sch_lad_code: '',
          sch_lad_name: '',
        },
      },
      measures: {
        '28': '10',
        '26': '20',
        '23': '4',
      },
      timeIdentifier: 'HT6',
      year: 2015,
    },
  ],
};

const labels = {
  '28': 'Authorised absence rate',
  '26': 'Overall absence rate',
  '23': 'Unauthorised absence rate',
};

const metaData: DataBlockMetadata = {
  filters: {
    '1': {
      label: 'All Schools',
      value: '1',
    },
    '2': {
      label: 'All Pupils',
      value: '1',
    },
  },

  indicators: {
    '23': {
      label: 'Unauthorised absence rate',
      unit: '%',
      value: '23',
    },
    '26': {
      label: 'Overall absence rate',
      unit: '%',
      value: '26',
    },
    '28': {
      label: 'Authorised absence rate',
      unit: '%',
      value: '28',
    },
  },

  timePeriods: {
    '2014_HT6': {
      label: '2014/15',
      value: '2014_HT6',
    },
    '2015_HT6': {
      label: '2015/16',
      value: '2015_HT6',
    },
  },

  locations: {
    E92000001: {
      value: 'E92000001',
      label: 'England',
      geoJson: [Features.E92000001],
    },
    S92000001: {
      value: 'S92000001',
      label: 'Scotland',
      geoJson: [Features.S92000001],
    },
  },
};

const responseMetadata: ResponseMetaData = {
  subject: {
    id: 1,
    label: 'subject 1',
  },
  filters: {
    Characteristic: {
      hint: 'Filter by pupil characteristic',
      legend: 'Characteristic',
      options: {
        AllPupils: {
          label: 'All pupils',
          options: [
            {
              label: 'All pupils',
              value: '1',
            },
          ],
        },
        Gender: {
          label: 'Gender',
          options: [
            {
              label: 'Gender male',
              value: '3',
            },
            {
              label: 'Gender female',
              value: '4',
            },
          ],
        },
        EthnicGroupMajor: {
          label: 'Ethnic group major',
          options: [
            {
              label: 'Ethnicity Major White Total',
              value: '5',
            },
            {
              label: 'Ethnicity Major Unclassified',
              value: '6',
            },
            {
              label: 'Ethnicity Minority Ethnic Group',
              value: '7',
            },
            {
              label: 'Ethnicity Major Any other Ethnic Group',
              value: '8',
            },
            {
              label: 'Ethnicity Major Chinese',
              value: '9',
            },
            {
              label: 'Ethnicity Major Black Total',
              value: '13',
            },
            {
              label: 'Ethnicity Major Asian Total',
              value: '18',
            },
            {
              label: 'Ethnicity Major Mixed Total',
              value: '25',
            },
          ],
        },
        EthnicGroupMinor: {
          label: 'Ethnic group minor',
          options: [
            {
              label: 'Ethnicity Minor Any other Black background',
              value: '10',
            },
            {
              label: 'Ethnicity Minor Black African',
              value: '11',
            },
            {
              label: 'Ethnicity Minor Black Caribbean',
              value: '12',
            },
            {
              label: 'Ethnicity Minor Any other Asian background',
              value: '14',
            },
            {
              label: 'Ethnicity Minor Bangladeshi',
              value: '15',
            },
            {
              label: 'Ethnicity Minor Pakistani',
              value: '16',
            },
            {
              label: 'Ethnicity Minor Indian',
              value: '17',
            },
            {
              label: 'Ethnicity Minor Any other Mixed background',
              value: '19',
            },
            {
              label: 'Ethnicity Minor White and Asian',
              value: '20',
            },
            {
              label: 'Ethnicity Minor White British',
              value: '21',
            },
            {
              label: 'Ethnicity Minor White and Black African',
              value: '22',
            },
            {
              label: 'Ethnicity Minor Irish',
              value: '23',
            },
            {
              label: 'Ethnicity Minor White and Black Caribbean',
              value: '24',
            },
            {
              label: 'Ethnicity Minor Traveller of Irish heritage',
              value: '26',
            },
            {
              label: 'Ethnicity Minor Any other White background',
              value: '27',
            },
            {
              label: 'Ethnicity Minor Gypsy Roma',
              value: '28',
            },
            {
              label: 'Ethnicity Minor Unclassified',
              value: '76',
            },
            {
              label: 'Ethnicity Minor Any other Ethnic Group',
              value: '77',
            },
            {
              label: 'Ethnicity Minor Chinese',
              value: '78',
            },
          ],
        },
        NCYear: {
          label: 'NC year',
          options: [
            {
              label: 'NC Year not followed or missing',
              value: '29',
            },
            {
              label: 'NC Year 12 and above',
              value: '30',
            },
            {
              label: 'NC Year 11',
              value: '31',
            },
            {
              label: 'NC Year 10',
              value: '32',
            },
            {
              label: 'NC Year 9',
              value: '33',
            },
            {
              label: 'NC Year 8',
              value: '34',
            },
            {
              label: 'NC Year 7',
              value: '35',
            },
            {
              label: 'NC Year 6',
              value: '36',
            },
            {
              label: 'NC Year 1 and below',
              value: '37',
            },
            {
              label: 'NC Year 5',
              value: '38',
            },
            {
              label: 'NC Year 2',
              value: '39',
            },
            {
              label: 'NC Year 4',
              value: '40',
            },
            {
              label: 'NC Year 3',
              value: '41',
            },
            {
              label: 'NC Year Unclassified',
              value: '74',
            },
          ],
        },
        FSM: {
          label: 'FSM',
          options: [
            {
              label: 'FSM eligible',
              value: '42',
            },
            {
              label: 'FSM not eligible',
              value: '43',
            },
            {
              label: 'FSM unclassified',
              value: '44',
            },
          ],
        },
        FSMEver6: {
          label: 'FSM ever 6',
          options: [
            {
              label: 'FSM eligible in last 6 years',
              value: '45',
            },
            {
              label: 'FSM not eligible in last 6 years',
              value: '46',
            },
            {
              label: 'FSM unclassified in last 6 years',
              value: '47',
            },
          ],
        },
        SENProvision: {
          label: 'SEN provision',
          options: [
            {
              label: 'SEN provision No identified SEN',
              value: '48',
            },
            {
              label: 'SEN provision Statement or EHCP',
              value: '49',
            },
            {
              label: 'SEN provision SEN Support',
              value: '50',
            },
            {
              label: 'SEN provision Unclassified',
              value: '51',
            },
            {
              label: 'SEN provision SEN without statement',
              value: '67',
            },
            {
              label: 'SEN provision statement',
              value: '69',
            },
            {
              label: 'SEN provision School Action Plus',
              value: '79',
            },
            {
              label: 'SEN provision School Action',
              value: '80',
            },
          ],
        },
        SENPrimaryNeed: {
          label: 'SEN primary need',
          options: [
            {
              label: 'SEN primary need Unclassified',
              value: '52',
            },
            {
              label: 'SEN primary need Visual impairment',
              value: '53',
            },
            {
              label: 'SEN primary need Specific learning difficulty',
              value: '54',
            },
            {
              label: 'SEN primary need Severe learning difficulty',
              value: '55',
            },
            {
              label:
                'SEN primary need Speech language and communications needs',
              value: '56',
            },
            {
              label: 'SEN primary need Social emotional and mental health',
              value: '57',
            },
            {
              label: 'SEN primary need Physical disability',
              value: '58',
            },
            {
              label: 'SEN primary need other difficulty/disability',
              value: '59',
            },
            {
              label:
                'SEN primary need Profound and multiple learning difficulty',
              value: '60',
            },
            {
              label: 'SEN primary need No specialist assessment',
              value: '61',
            },
            {
              label: 'SEN primary need Hearing impairment',
              value: '62',
            },
            {
              label: 'SEN primary need Moderate learning difficulty',
              value: '63',
            },
            {
              label: 'SEN primary need Multi-sensory impairment',
              value: '64',
            },
            {
              label: 'SEN primary need Autistic spectrum disorder',
              value: '65',
            },
          ],
        },
        FirstLanguage: {
          label: 'First language',
          options: [
            {
              label: 'First language Known or believed to be English',
              value: '66',
            },
            {
              label:
                'First language Known or believed to be other than English',
              value: '68',
            },
            {
              label: 'First First language Unclassified',
              value: '70',
            },
            {
              label: 'First language Unclassified',
              value: '75',
            },
          ],
        },
      },
    },
    SchoolType: {
      hint: 'Filter by school type',
      legend: 'School type',
      options: {
        Default: {
          label: 'Default',
          options: [
            {
              label: 'All schools',
              value: '2',
            },
            {
              label: 'Special',
              value: '71',
            },
            {
              label: 'State-funded primary',
              value: '72',
            },
            {
              label: 'State-funded secondary',
              value: '73',
            },
          ],
        },
      },
    },
  },
  indicators: {
    AbsenceFields: {
      label: 'Absence fields',
      options: [
        {
          label: 'Number of pupil enrolments',
          unit: '',
          value: '1',
        },
        {
          label: 'Unauthorised absence rate',
          unit: '%',
          value: '23',
        },
        {
          label: 'Number of unauthorised absence sessions',
          unit: '',
          value: '24',
        },
        {
          label: 'Number of sessions possible',
          unit: '',
          value: '25',
        },
        {
          label: 'Overall absence rate',
          unit: '%',
          value: '26',
        },
        {
          label: 'Number of overall absence sessions',
          unit: '',
          value: '27',
        },
        {
          label: 'Authorised absence rate',
          unit: '%',
          value: '28',
        },
        {
          label: 'Number of authorised absence sessions',
          unit: '',
          value: '29',
        },
        {
          label: 'Percentage of persistent absentees',
          unit: '%',
          value: '30',
        },
        {
          label: 'Number of persistent absentees',
          unit: '',
          value: '31',
        },
      ],
    },
    AbsenceByReason: {
      label: 'Absence by reason',
      options: [
        {
          label: 'Number of unauthorised reasons sessions',
          unit: '',
          value: '7',
        },
        {
          label: 'Number of unauthorised other sessions',
          unit: '',
          value: '8',
        },
        {
          label: 'Number of no reason yet sessions',
          unit: '',
          value: '9',
        },
        {
          label: 'Number of late sessions',
          unit: '',
          value: '10',
        },
        {
          label: 'Number of unauthorised holiday sessions',
          unit: '',
          value: '11',
        },
        {
          label: 'Number of overall reasons sessions',
          unit: '',
          value: '12',
        },
        {
          label: 'Number of traveller sessions',
          unit: '',
          value: '13',
        },
        {
          label: 'Number of authorised reasons sessions',
          unit: '',
          value: '14',
        },
        {
          label: 'Number of study leave sessions',
          unit: '',
          value: '15',
        },
        {
          label: 'Number of religious observance sessions',
          unit: '',
          value: '17',
        },
        {
          label: 'Number of illness sessions',
          unit: '',
          value: '18',
        },
        {
          label: 'Number of authorised holiday sessions',
          unit: '',
          value: '19',
        },
        {
          label: 'Number of extended authorised holiday sessions',
          unit: '',
          value: '20',
        },
        {
          label: 'Number of excluded sessions',
          unit: '',
          value: '21',
        },
        {
          label: 'Number of medical appointments sessions',
          unit: '',
          value: '22',
        },
        {
          label: 'Number of authorised other sessions',
          unit: '',
          value: '32',
        },
      ],
    },
    AbsenceForPersistentAbsentees: {
      label: 'Absence for persistent absentees',
      options: [
        {
          label: 'Number of sessions possible for persistent absentees',
          unit: '',
          value: '2',
        },
        {
          label: 'Overall absence rate',
          unit: '%',
          value: '3',
        },
        {
          label: 'Number of overall absence sessions for persistent absentees',
          unit: '',
          value: '4',
        },
        {
          label: 'Authorised absence rate',
          unit: '%',
          value: '5',
        },
        {
          label:
            'Number of authorised absence sessions for persistent absentees',
          unit: '',
          value: '6',
        },
        {
          label:
            'Number of unauthorised absence sessions for persistent absentees',
          unit: '',
          value: '16',
        },
        {
          label: 'Unauthorised absence rate',
          unit: '%',
          value: '33',
        },
      ],
    },
  },
  locations: {
    LocalAuthority: {
      hint: '',
      legend: 'Local Authority',
      options: [
        {
          label: 'City of London',
          value: '201',
        },
        {
          label: 'Camden',
          value: '202',
        },
        {
          label: 'Greenwich',
          value: '203',
        },
        {
          label: 'Hackney',
          value: '204',
        },
        {
          label: 'Hammersmith and Fulham',
          value: '205',
        },
        {
          label: 'Islington',
          value: '206',
        },
        {
          label: 'Kensington and Chelsea',
          value: '207',
        },
        {
          label: 'Lambeth',
          value: '208',
        },
        {
          label: 'Lewisham',
          value: '209',
        },
        {
          label: 'Southwark',
          value: '210',
        },
        {
          label: 'Tower Hamlets',
          value: '211',
        },
        {
          label: 'Wandsworth',
          value: '212',
        },
        {
          label: 'Westminster',
          value: '213',
        },
        {
          label: 'Barking and Dagenham',
          value: '301',
        },
        {
          label: 'Barnet',
          value: '302',
        },
        {
          label: 'Bexley',
          value: '303',
        },
        {
          label: 'Brent',
          value: '304',
        },
        {
          label: 'Bromley',
          value: '305',
        },
        {
          label: 'Croydon',
          value: '306',
        },
        {
          label: 'Ealing',
          value: '307',
        },
        {
          label: 'Enfield',
          value: '308',
        },
        {
          label: 'Haringey',
          value: '309',
        },
        {
          label: 'Harrow',
          value: '310',
        },
        {
          label: 'Havering',
          value: '311',
        },
        {
          label: 'Hillingdon',
          value: '312',
        },
        {
          label: 'Hounslow',
          value: '313',
        },
        {
          label: 'Kingston upon Thames',
          value: '314',
        },
        {
          label: 'Merton',
          value: '315',
        },
        {
          label: 'Newham',
          value: '316',
        },
        {
          label: 'Redbridge',
          value: '317',
        },
        {
          label: 'Richmond upon Thames',
          value: '318',
        },
        {
          label: 'Sutton',
          value: '319',
        },
        {
          label: 'Waltham Forest',
          value: '320',
        },
        {
          label: 'Birmingham',
          value: '330',
        },
        {
          label: 'Coventry',
          value: '331',
        },
        {
          label: 'Dudley',
          value: '332',
        },
        {
          label: 'Sandwell',
          value: '333',
        },
        {
          label: 'Solihull',
          value: '334',
        },
        {
          label: 'Walsall',
          value: '335',
        },
        {
          label: 'Wolverhampton',
          value: '336',
        },
        {
          label: 'Knowsley',
          value: '340',
        },
        {
          label: 'Liverpool',
          value: '341',
        },
        {
          label: 'St. Helens',
          value: '342',
        },
        {
          label: 'Sefton',
          value: '343',
        },
        {
          label: 'Wirral',
          value: '344',
        },
        {
          label: 'Bolton',
          value: '350',
        },
        {
          label: 'Bury',
          value: '351',
        },
        {
          label: 'Manchester',
          value: '352',
        },
        {
          label: 'Oldham',
          value: '353',
        },
        {
          label: 'Rochdale',
          value: '354',
        },
        {
          label: 'Salford',
          value: '355',
        },
        {
          label: 'Stockport',
          value: '356',
        },
        {
          label: 'Tameside',
          value: '357',
        },
        {
          label: 'Trafford',
          value: '358',
        },
        {
          label: 'Wigan',
          value: '359',
        },
        {
          label: 'Barnsley',
          value: '370',
        },
        {
          label: 'Doncaster',
          value: '371',
        },
        {
          label: 'Rotherham',
          value: '372',
        },
        {
          label: 'Sheffield',
          value: '373',
        },
        {
          label: 'Bradford',
          value: '380',
        },
        {
          label: 'Calderdale',
          value: '381',
        },
        {
          label: 'Kirklees',
          value: '382',
        },
        {
          label: 'Leeds',
          value: '383',
        },
        {
          label: 'Wakefield',
          value: '384',
        },
        {
          label: 'Gateshead',
          value: '390',
        },
        {
          label: 'Newcastle upon Tyne',
          value: '391',
        },
        {
          label: 'North Tyneside',
          value: '392',
        },
        {
          label: 'South Tyneside',
          value: '393',
        },
        {
          label: 'Sunderland',
          value: '394',
        },
        {
          label: 'Isles Of Scilly',
          value: '420',
        },
        {
          label: 'Bath and North East Somerset',
          value: '800',
        },
        {
          label: 'Bristol City of',
          value: '801',
        },
        {
          label: 'North Somerset',
          value: '802',
        },
        {
          label: 'South Gloucestershire',
          value: '803',
        },
        {
          label: 'Hartlepool',
          value: '805',
        },
        {
          label: 'Middlesbrough',
          value: '806',
        },
        {
          label: 'Redcar and Cleveland',
          value: '807',
        },
        {
          label: 'Stockton-on-Tees',
          value: '808',
        },
        {
          label: 'Kingston upon Hull City of',
          value: '810',
        },
        {
          label: 'East Riding of Yorkshire',
          value: '811',
        },
        {
          label: 'North East Lincolnshire',
          value: '812',
        },
        {
          label: 'North Lincolnshire',
          value: '813',
        },
        {
          label: 'North Yorkshire',
          value: '815',
        },
        {
          label: 'York',
          value: '816',
        },
        {
          label: 'Luton',
          value: '821',
        },
        {
          label: 'Bedford',
          value: '822',
        },
        {
          label: 'Central Bedfordshire',
          value: '823',
        },
        {
          label: 'Buckinghamshire',
          value: '825',
        },
        {
          label: 'Milton Keynes',
          value: '826',
        },
        {
          label: 'Derbyshire',
          value: '830',
        },
        {
          label: 'Derby',
          value: '831',
        },
        {
          label: 'Dorset',
          value: '835',
        },
        {
          label: 'Poole',
          value: '836',
        },
        {
          label: 'Bournemouth',
          value: '837',
        },
        {
          label: 'Durham',
          value: '840',
        },
        {
          label: 'Darlington',
          value: '841',
        },
        {
          label: 'East Sussex',
          value: '845',
        },
        {
          label: 'Brighton and Hove',
          value: '846',
        },
        {
          label: 'Hampshire',
          value: '850',
        },
        {
          label: 'Portsmouth',
          value: '851',
        },
        {
          label: 'Southampton',
          value: '852',
        },
        {
          label: 'Leicestershire',
          value: '855',
        },
        {
          label: 'Leicester',
          value: '856',
        },
        {
          label: 'Rutland',
          value: '857',
        },
        {
          label: 'Staffordshire',
          value: '860',
        },
        {
          label: 'Stoke-on-Trent',
          value: '861',
        },
        {
          label: 'Wiltshire',
          value: '865',
        },
        {
          label: 'Swindon',
          value: '866',
        },
        {
          label: 'Bracknell Forest',
          value: '867',
        },
        {
          label: 'Windsor and Maidenhead',
          value: '868',
        },
        {
          label: 'West Berkshire',
          value: '869',
        },
        {
          label: 'Reading',
          value: '870',
        },
        {
          label: 'Slough',
          value: '871',
        },
        {
          label: 'Wokingham',
          value: '872',
        },
        {
          label: 'Cambridgeshire',
          value: '873',
        },
        {
          label: 'Peterborough',
          value: '874',
        },
        {
          label: 'Halton',
          value: '876',
        },
        {
          label: 'Warrington',
          value: '877',
        },
        {
          label: 'Devon',
          value: '878',
        },
        {
          label: 'Plymouth',
          value: '879',
        },
        {
          label: 'Torbay',
          value: '880',
        },
        {
          label: 'Essex',
          value: '881',
        },
        {
          label: 'Southend-on-Sea',
          value: '882',
        },
        {
          label: 'Thurrock',
          value: '883',
        },
        {
          label: 'Herefordshire',
          value: '884',
        },
        {
          label: 'Worcestershire',
          value: '885',
        },
        {
          label: 'Kent',
          value: '886',
        },
        {
          label: 'Medway',
          value: '887',
        },
        {
          label: 'Lancashire',
          value: '888',
        },
        {
          label: 'Blackburn with Darwen',
          value: '889',
        },
        {
          label: 'Blackpool',
          value: '890',
        },
        {
          label: 'Nottinghamshire',
          value: '891',
        },
        {
          label: 'Nottingham',
          value: '892',
        },
        {
          label: 'Shropshire',
          value: '893',
        },
        {
          label: 'Telford and Wrekin',
          value: '894',
        },
        {
          label: 'Cheshire East',
          value: '895',
        },
        {
          label: 'Cheshire West and Chester',
          value: '896',
        },
        {
          label: 'Cornwall',
          value: '908',
        },
        {
          label: 'Cumbria',
          value: '909',
        },
        {
          label: 'Gloucestershire',
          value: '916',
        },
        {
          label: 'Hertfordshire',
          value: '919',
        },
        {
          label: 'Isle of Wight',
          value: '921',
        },
        {
          label: 'Lincolnshire',
          value: '925',
        },
        {
          label: 'Norfolk',
          value: '926',
        },
        {
          label: 'Northamptonshire',
          value: '928',
        },
        {
          label: 'Northumberland',
          value: '929',
        },
        {
          label: 'Oxfordshire',
          value: '931',
        },
        {
          label: 'Somerset',
          value: '933',
        },
        {
          label: 'Suffolk',
          value: '935',
        },
        {
          label: 'Surrey',
          value: '936',
        },
        {
          label: 'Warwickshire',
          value: '937',
        },
        {
          label: 'West Sussex',
          value: '938',
        },
      ],
    },
    LocalAuthorityDistrict: {
      hint: '',
      legend: 'Local Authority District',
      options: [
        {
          label: 'Hartlepool',
          value: 'E06000001',
        },
        {
          label: 'Middlesbrough',
          value: 'E06000002',
        },
        {
          label: 'Redcar and Cleveland',
          value: 'E06000003',
        },
        {
          label: 'Stockton-on-Tees',
          value: 'E06000004',
        },
        {
          label: 'Darlington',
          value: 'E06000005',
        },
        {
          label: 'County Durham',
          value: 'E06000047',
        },
        {
          label: 'Northumberland',
          value: 'E06000057',
        },
        {
          label: 'Newcastle upon Tyne',
          value: 'E08000021',
        },
        {
          label: 'North Tyneside',
          value: 'E08000022',
        },
        {
          label: 'South Tyneside',
          value: 'E08000023',
        },
        {
          label: 'Sunderland',
          value: 'E08000024',
        },
        {
          label: 'Gateshead',
          value: 'E08000037',
        },
        {
          label: 'Halton',
          value: 'E06000006',
        },
        {
          label: 'Warrington',
          value: 'E06000007',
        },
        {
          label: 'Blackburn with Darwen',
          value: 'E06000008',
        },
        {
          label: 'Blackpool',
          value: 'E06000009',
        },
        {
          label: 'Cheshire East',
          value: 'E06000049',
        },
        {
          label: 'Cheshire West and Chester',
          value: 'E06000050',
        },
        {
          label: 'Allerdale',
          value: 'E07000026',
        },
        {
          label: 'Barrow-in-Furness',
          value: 'E07000027',
        },
        {
          label: 'Carlisle',
          value: 'E07000028',
        },
        {
          label: 'Copeland',
          value: 'E07000029',
        },
        {
          label: 'Eden',
          value: 'E07000030',
        },
        {
          label: 'South Lakeland',
          value: 'E07000031',
        },
        {
          label: 'Burnley',
          value: 'E07000117',
        },
        {
          label: 'Chorley',
          value: 'E07000118',
        },
        {
          label: 'Fylde',
          value: 'E07000119',
        },
        {
          label: 'Hyndburn',
          value: 'E07000120',
        },
        {
          label: 'Lancaster',
          value: 'E07000121',
        },
        {
          label: 'Pendle',
          value: 'E07000122',
        },
        {
          label: 'Preston',
          value: 'E07000123',
        },
        {
          label: 'Ribble Valley',
          value: 'E07000124',
        },
        {
          label: 'Rossendale',
          value: 'E07000125',
        },
        {
          label: 'South Ribble',
          value: 'E07000126',
        },
        {
          label: 'West Lancashire',
          value: 'E07000127',
        },
        {
          label: 'Wyre',
          value: 'E07000128',
        },
        {
          label: 'Bolton',
          value: 'E08000001',
        },
        {
          label: 'Bury',
          value: 'E08000002',
        },
        {
          label: 'Manchester',
          value: 'E08000003',
        },
        {
          label: 'Oldham',
          value: 'E08000004',
        },
        {
          label: 'Rochdale',
          value: 'E08000005',
        },
        {
          label: 'Salford',
          value: 'E08000006',
        },
        {
          label: 'Stockport',
          value: 'E08000007',
        },
        {
          label: 'Tameside',
          value: 'E08000008',
        },
        {
          label: 'Trafford',
          value: 'E08000009',
        },
        {
          label: 'Wigan',
          value: 'E08000010',
        },
        {
          label: 'Knowsley',
          value: 'E08000011',
        },
        {
          label: 'Liverpool',
          value: 'E08000012',
        },
        {
          label: 'St. Helens',
          value: 'E08000013',
        },
        {
          label: 'Sefton',
          value: 'E08000014',
        },
        {
          label: 'Wirral',
          value: 'E08000015',
        },
        {
          label: 'Kingston upon Hull City of',
          value: 'E06000010',
        },
        {
          label: 'East Riding of Yorkshire',
          value: 'E06000011',
        },
        {
          label: 'North East Lincolnshire',
          value: 'E06000012',
        },
        {
          label: 'North Lincolnshire',
          value: 'E06000013',
        },
        {
          label: 'York',
          value: 'E06000014',
        },
        {
          label: 'Craven',
          value: 'E07000163',
        },
        {
          label: 'Hambleton',
          value: 'E07000164',
        },
        {
          label: 'Harrogate',
          value: 'E07000165',
        },
        {
          label: 'Richmondshire',
          value: 'E07000166',
        },
        {
          label: 'Ryedale',
          value: 'E07000167',
        },
        {
          label: 'Scarborough',
          value: 'E07000168',
        },
        {
          label: 'Selby',
          value: 'E07000169',
        },
        {
          label: 'Barnsley',
          value: 'E08000016',
        },
        {
          label: 'Doncaster',
          value: 'E08000017',
        },
        {
          label: 'Rotherham',
          value: 'E08000018',
        },
        {
          label: 'Sheffield',
          value: 'E08000019',
        },
        {
          label: 'Bradford',
          value: 'E08000032',
        },
        {
          label: 'Calderdale',
          value: 'E08000033',
        },
        {
          label: 'Kirklees',
          value: 'E08000034',
        },
        {
          label: 'Leeds',
          value: 'E08000035',
        },
        {
          label: 'Wakefield',
          value: 'E08000036',
        },
        {
          label: 'Derby',
          value: 'E06000015',
        },
        {
          label: 'Leicester',
          value: 'E06000016',
        },
        {
          label: 'Rutland',
          value: 'E06000017',
        },
        {
          label: 'Nottingham',
          value: 'E06000018',
        },
        {
          label: 'Amber Valley',
          value: 'E07000032',
        },
        {
          label: 'Bolsover',
          value: 'E07000033',
        },
        {
          label: 'Chesterfield',
          value: 'E07000034',
        },
        {
          label: 'Derbyshire Dales',
          value: 'E07000035',
        },
        {
          label: 'Erewash',
          value: 'E07000036',
        },
        {
          label: 'High Peak',
          value: 'E07000037',
        },
        {
          label: 'North East Derbyshire',
          value: 'E07000038',
        },
        {
          label: 'South Derbyshire',
          value: 'E07000039',
        },
        {
          label: 'Blaby',
          value: 'E07000129',
        },
        {
          label: 'Charnwood',
          value: 'E07000130',
        },
        {
          label: 'Harborough',
          value: 'E07000131',
        },
        {
          label: 'Hinckley and Bosworth',
          value: 'E07000132',
        },
        {
          label: 'Melton',
          value: 'E07000133',
        },
        {
          label: 'North West Leicestershire',
          value: 'E07000134',
        },
        {
          label: 'Oadby and Wigston',
          value: 'E07000135',
        },
        {
          label: 'Boston',
          value: 'E07000136',
        },
        {
          label: 'East Lindsey',
          value: 'E07000137',
        },
        {
          label: 'Lincoln',
          value: 'E07000138',
        },
        {
          label: 'North Kesteven',
          value: 'E07000139',
        },
        {
          label: 'South Holland',
          value: 'E07000140',
        },
        {
          label: 'South Kesteven',
          value: 'E07000141',
        },
        {
          label: 'West Lindsey',
          value: 'E07000142',
        },
        {
          label: 'Corby',
          value: 'E07000150',
        },
        {
          label: 'Daventry',
          value: 'E07000151',
        },
        {
          label: 'East Northamptonshire',
          value: 'E07000152',
        },
        {
          label: 'Kettering',
          value: 'E07000153',
        },
        {
          label: 'Northampton',
          value: 'E07000154',
        },
        {
          label: 'South Northamptonshire',
          value: 'E07000155',
        },
        {
          label: 'Wellingborough',
          value: 'E07000156',
        },
        {
          label: 'Ashfield',
          value: 'E07000170',
        },
        {
          label: 'Bassetlaw',
          value: 'E07000171',
        },
        {
          label: 'Broxtowe',
          value: 'E07000172',
        },
        {
          label: 'Gedling',
          value: 'E07000173',
        },
        {
          label: 'Mansfield',
          value: 'E07000174',
        },
        {
          label: 'Newark and Sherwood',
          value: 'E07000175',
        },
        {
          label: 'Rushcliffe',
          value: 'E07000176',
        },
        {
          label: 'Herefordshire County of',
          value: 'E06000019',
        },
        {
          label: 'Telford and Wrekin',
          value: 'E06000020',
        },
        {
          label: 'Stoke-on-Trent',
          value: 'E06000021',
        },
        {
          label: 'Shropshire',
          value: 'E06000051',
        },
        {
          label: 'Cannock Chase',
          value: 'E07000192',
        },
        {
          label: 'East Staffordshire',
          value: 'E07000193',
        },
        {
          label: 'Lichfield',
          value: 'E07000194',
        },
        {
          label: 'Newcastle-under-Lyme',
          value: 'E07000195',
        },
        {
          label: 'South Staffordshire',
          value: 'E07000196',
        },
        {
          label: 'Stafford',
          value: 'E07000197',
        },
        {
          label: 'Staffordshire Moorlands',
          value: 'E07000198',
        },
        {
          label: 'Tamworth',
          value: 'E07000199',
        },
        {
          label: 'North Warwickshire',
          value: 'E07000218',
        },
        {
          label: 'Nuneaton and Bedworth',
          value: 'E07000219',
        },
        {
          label: 'Rugby',
          value: 'E07000220',
        },
        {
          label: 'Stratford-on-Avon',
          value: 'E07000221',
        },
        {
          label: 'Warwick',
          value: 'E07000222',
        },
        {
          label: 'Bromsgrove',
          value: 'E07000234',
        },
        {
          label: 'Malvern Hills',
          value: 'E07000235',
        },
        {
          label: 'Redditch',
          value: 'E07000236',
        },
        {
          label: 'Worcester',
          value: 'E07000237',
        },
        {
          label: 'Wychavon',
          value: 'E07000238',
        },
        {
          label: 'Wyre Forest',
          value: 'E07000239',
        },
        {
          label: 'Birmingham',
          value: 'E08000025',
        },
        {
          label: 'Coventry',
          value: 'E08000026',
        },
        {
          label: 'Dudley',
          value: 'E08000027',
        },
        {
          label: 'Sandwell',
          value: 'E08000028',
        },
        {
          label: 'Solihull',
          value: 'E08000029',
        },
        {
          label: 'Walsall',
          value: 'E08000030',
        },
        {
          label: 'Wolverhampton',
          value: 'E08000031',
        },
        {
          label: 'Peterborough',
          value: 'E06000031',
        },
        {
          label: 'Luton',
          value: 'E06000032',
        },
        {
          label: 'Southend-on-Sea',
          value: 'E06000033',
        },
        {
          label: 'Thurrock',
          value: 'E06000034',
        },
        {
          label: 'Bedford',
          value: 'E06000055',
        },
        {
          label: 'Central Bedfordshire',
          value: 'E06000056',
        },
        {
          label: 'Cambridge',
          value: 'E07000008',
        },
        {
          label: 'East Cambridgeshire',
          value: 'E07000009',
        },
        {
          label: 'Fenland',
          value: 'E07000010',
        },
        {
          label: 'Huntingdonshire',
          value: 'E07000011',
        },
        {
          label: 'South Cambridgeshire',
          value: 'E07000012',
        },
        {
          label: 'Basildon',
          value: 'E07000066',
        },
        {
          label: 'Braintree',
          value: 'E07000067',
        },
        {
          label: 'Brentwood',
          value: 'E07000068',
        },
        {
          label: 'Castle Point',
          value: 'E07000069',
        },
        {
          label: 'Chelmsford',
          value: 'E07000070',
        },
        {
          label: 'Colchester',
          value: 'E07000071',
        },
        {
          label: 'Epping Forest',
          value: 'E07000072',
        },
        {
          label: 'Harlow',
          value: 'E07000073',
        },
        {
          label: 'Maldon',
          value: 'E07000074',
        },
        {
          label: 'Rochford',
          value: 'E07000075',
        },
        {
          label: 'Tendring',
          value: 'E07000076',
        },
        {
          label: 'Uttlesford',
          value: 'E07000077',
        },
        {
          label: 'Broxbourne',
          value: 'E07000095',
        },
        {
          label: 'Dacorum',
          value: 'E07000096',
        },
        {
          label: 'Hertsmere',
          value: 'E07000098',
        },
        {
          label: 'North Hertfordshire',
          value: 'E07000099',
        },
        {
          label: 'Three Rivers',
          value: 'E07000102',
        },
        {
          label: 'Watford',
          value: 'E07000103',
        },
        {
          label: 'Breckland',
          value: 'E07000143',
        },
        {
          label: 'Broadland',
          value: 'E07000144',
        },
        {
          label: 'Great Yarmouth',
          value: 'E07000145',
        },
        {
          label: "King's Lynn and West Norfolk",
          value: 'E07000146',
        },
        {
          label: 'North Norfolk',
          value: 'E07000147',
        },
        {
          label: 'Norwich',
          value: 'E07000148',
        },
        {
          label: 'South Norfolk',
          value: 'E07000149',
        },
        {
          label: 'Babergh',
          value: 'E07000200',
        },
        {
          label: 'Forest Heath',
          value: 'E07000201',
        },
        {
          label: 'Ipswich',
          value: 'E07000202',
        },
        {
          label: 'Mid Suffolk',
          value: 'E07000203',
        },
        {
          label: 'St Edmundsbury',
          value: 'E07000204',
        },
        {
          label: 'Suffolk Coastal',
          value: 'E07000205',
        },
        {
          label: 'Waveney',
          value: 'E07000206',
        },
        {
          label: 'St Albans',
          value: 'E07000240',
        },
        {
          label: 'Welwyn Hatfield',
          value: 'E07000241',
        },
        {
          label: 'East Hertfordshire',
          value: 'E07000242',
        },
        {
          label: 'Stevenage',
          value: 'E07000243',
        },
        {
          label: 'Medway',
          value: 'E06000035',
        },
        {
          label: 'Bracknell Forest',
          value: 'E06000036',
        },
        {
          label: 'West Berkshire',
          value: 'E06000037',
        },
        {
          label: 'Reading',
          value: 'E06000038',
        },
        {
          label: 'Slough',
          value: 'E06000039',
        },
        {
          label: 'Windsor and Maidenhead',
          value: 'E06000040',
        },
        {
          label: 'Wokingham',
          value: 'E06000041',
        },
        {
          label: 'Milton Keynes',
          value: 'E06000042',
        },
        {
          label: 'Brighton and Hove',
          value: 'E06000043',
        },
        {
          label: 'Portsmouth',
          value: 'E06000044',
        },
        {
          label: 'Southampton',
          value: 'E06000045',
        },
        {
          label: 'Isle of Wight',
          value: 'E06000046',
        },
        {
          label: 'Aylesbury Vale',
          value: 'E07000004',
        },
        {
          label: 'Chiltern',
          value: 'E07000005',
        },
        {
          label: 'South Bucks',
          value: 'E07000006',
        },
        {
          label: 'Wycombe',
          value: 'E07000007',
        },
        {
          label: 'Eastbourne',
          value: 'E07000061',
        },
        {
          label: 'Hastings',
          value: 'E07000062',
        },
        {
          label: 'Lewes',
          value: 'E07000063',
        },
        {
          label: 'Rother',
          value: 'E07000064',
        },
        {
          label: 'Wealden',
          value: 'E07000065',
        },
        {
          label: 'Basingstoke and Deane',
          value: 'E07000084',
        },
        {
          label: 'East Hampshire',
          value: 'E07000085',
        },
        {
          label: 'Eastleigh',
          value: 'E07000086',
        },
        {
          label: 'Fareham',
          value: 'E07000087',
        },
        {
          label: 'Gosport',
          value: 'E07000088',
        },
        {
          label: 'Hart',
          value: 'E07000089',
        },
        {
          label: 'Havant',
          value: 'E07000090',
        },
        {
          label: 'New Forest',
          value: 'E07000091',
        },
        {
          label: 'Rushmoor',
          value: 'E07000092',
        },
        {
          label: 'Test Valley',
          value: 'E07000093',
        },
        {
          label: 'Winchester',
          value: 'E07000094',
        },
        {
          label: 'Ashford',
          value: 'E07000105',
        },
        {
          label: 'Canterbury',
          value: 'E07000106',
        },
        {
          label: 'Dartford',
          value: 'E07000107',
        },
        {
          label: 'Dover',
          value: 'E07000108',
        },
        {
          label: 'Gravesham',
          value: 'E07000109',
        },
        {
          label: 'Maidstone',
          value: 'E07000110',
        },
        {
          label: 'Sevenoaks',
          value: 'E07000111',
        },
        {
          label: 'Shepway',
          value: 'E07000112',
        },
        {
          label: 'Swale',
          value: 'E07000113',
        },
        {
          label: 'Thanet',
          value: 'E07000114',
        },
        {
          label: 'Tonbridge and Malling',
          value: 'E07000115',
        },
        {
          label: 'Tunbridge Wells',
          value: 'E07000116',
        },
        {
          label: 'Cherwell',
          value: 'E07000177',
        },
        {
          label: 'Oxford',
          value: 'E07000178',
        },
        {
          label: 'South Oxfordshire',
          value: 'E07000179',
        },
        {
          label: 'Vale of White Horse',
          value: 'E07000180',
        },
        {
          label: 'West Oxfordshire',
          value: 'E07000181',
        },
        {
          label: 'Elmbridge',
          value: 'E07000207',
        },
        {
          label: 'Epsom and Ewell',
          value: 'E07000208',
        },
        {
          label: 'Guildford',
          value: 'E07000209',
        },
        {
          label: 'Mole Valley',
          value: 'E07000210',
        },
        {
          label: 'Reigate and Banstead',
          value: 'E07000211',
        },
        {
          label: 'Runnymede',
          value: 'E07000212',
        },
        {
          label: 'Spelthorne',
          value: 'E07000213',
        },
        {
          label: 'Surrey Heath',
          value: 'E07000214',
        },
        {
          label: 'Tandridge',
          value: 'E07000215',
        },
        {
          label: 'Waverley',
          value: 'E07000216',
        },
        {
          label: 'Woking',
          value: 'E07000217',
        },
        {
          label: 'Adur',
          value: 'E07000223',
        },
        {
          label: 'Arun',
          value: 'E07000224',
        },
        {
          label: 'Chichester',
          value: 'E07000225',
        },
        {
          label: 'Crawley',
          value: 'E07000226',
        },
        {
          label: 'Horsham',
          value: 'E07000227',
        },
        {
          label: 'Mid Sussex',
          value: 'E07000228',
        },
        {
          label: 'Worthing',
          value: 'E07000229',
        },
        {
          label: 'Bath and North East Somerset',
          value: 'E06000022',
        },
        {
          label: 'Bristol City of',
          value: 'E06000023',
        },
        {
          label: 'North Somerset',
          value: 'E06000024',
        },
        {
          label: 'South Gloucestershire',
          value: 'E06000025',
        },
        {
          label: 'Plymouth',
          value: 'E06000026',
        },
        {
          label: 'Torbay',
          value: 'E06000027',
        },
        {
          label: 'Bournemouth',
          value: 'E06000028',
        },
        {
          label: 'Poole',
          value: 'E06000029',
        },
        {
          label: 'Swindon',
          value: 'E06000030',
        },
        {
          label: 'Cornwall',
          value: 'E06000052',
        },
        {
          label: 'Isles of Scilly',
          value: 'E06000053',
        },
        {
          label: 'Wiltshire',
          value: 'E06000054',
        },
        {
          label: 'East Devon',
          value: 'E07000040',
        },
        {
          label: 'Exeter',
          value: 'E07000041',
        },
        {
          label: 'Mid Devon',
          value: 'E07000042',
        },
        {
          label: 'North Devon',
          value: 'E07000043',
        },
        {
          label: 'South Hams',
          value: 'E07000044',
        },
        {
          label: 'Teignbridge',
          value: 'E07000045',
        },
        {
          label: 'Torridge',
          value: 'E07000046',
        },
        {
          label: 'West Devon',
          value: 'E07000047',
        },
        {
          label: 'Christchurch',
          value: 'E07000048',
        },
        {
          label: 'East Dorset',
          value: 'E07000049',
        },
        {
          label: 'North Dorset',
          value: 'E07000050',
        },
        {
          label: 'Purbeck',
          value: 'E07000051',
        },
        {
          label: 'West Dorset',
          value: 'E07000052',
        },
        {
          label: 'Weymouth and Portland',
          value: 'E07000053',
        },
        {
          label: 'Cheltenham',
          value: 'E07000078',
        },
        {
          label: 'Cotswold',
          value: 'E07000079',
        },
        {
          label: 'Forest of Dean',
          value: 'E07000080',
        },
        {
          label: 'Gloucester',
          value: 'E07000081',
        },
        {
          label: 'Stroud',
          value: 'E07000082',
        },
        {
          label: 'Tewkesbury',
          value: 'E07000083',
        },
        {
          label: 'Mendip',
          value: 'E07000187',
        },
        {
          label: 'Sedgemoor',
          value: 'E07000188',
        },
        {
          label: 'South Somerset',
          value: 'E07000189',
        },
        {
          label: 'Taunton Deane',
          value: 'E07000190',
        },
        {
          label: 'West Somerset',
          value: 'E07000191',
        },
        {
          label: 'City of London',
          value: 'E09000001',
        },
        {
          label: 'Camden',
          value: 'E09000007',
        },
        {
          label: 'Hackney',
          value: 'E09000012',
        },
        {
          label: 'Hammersmith and Fulham',
          value: 'E09000013',
        },
        {
          label: 'Haringey',
          value: 'E09000014',
        },
        {
          label: 'Islington',
          value: 'E09000019',
        },
        {
          label: 'Kensington and Chelsea',
          value: 'E09000020',
        },
        {
          label: 'Lambeth',
          value: 'E09000022',
        },
        {
          label: 'Lewisham',
          value: 'E09000023',
        },
        {
          label: 'Newham',
          value: 'E09000025',
        },
        {
          label: 'Southwark',
          value: 'E09000028',
        },
        {
          label: 'Tower Hamlets',
          value: 'E09000030',
        },
        {
          label: 'Wandsworth',
          value: 'E09000032',
        },
        {
          label: 'Westminster',
          value: 'E09000033',
        },
        {
          label: 'Barking and Dagenham',
          value: 'E09000002',
        },
        {
          label: 'Barnet',
          value: 'E09000003',
        },
        {
          label: 'Bexley',
          value: 'E09000004',
        },
        {
          label: 'Brent',
          value: 'E09000005',
        },
        {
          label: 'Bromley',
          value: 'E09000006',
        },
        {
          label: 'Croydon',
          value: 'E09000008',
        },
        {
          label: 'Ealing',
          value: 'E09000009',
        },
        {
          label: 'Enfield',
          value: 'E09000010',
        },
        {
          label: 'Greenwich',
          value: 'E09000011',
        },
        {
          label: 'Harrow',
          value: 'E09000015',
        },
        {
          label: 'Havering',
          value: 'E09000016',
        },
        {
          label: 'Hillingdon',
          value: 'E09000017',
        },
        {
          label: 'Hounslow',
          value: 'E09000018',
        },
        {
          label: 'Kingston upon Thames',
          value: 'E09000021',
        },
        {
          label: 'Merton',
          value: 'E09000024',
        },
        {
          label: 'Redbridge',
          value: 'E09000026',
        },
        {
          label: 'Richmond upon Thames',
          value: 'E09000027',
        },
        {
          label: 'Sutton',
          value: 'E09000029',
        },
        {
          label: 'Waltham Forest',
          value: 'E09000031',
        },
      ],
    },
    National: {
      hint: '',
      legend: 'National',
      options: [
        {
          label: 'England',
          value: 'E92000001',
        },
      ],
    },
    Regional: {
      hint: '',
      legend: 'Region',
      options: [
        {
          label: 'Inner London',
          value: 'E13000001',
        },
        {
          label: 'Outer London',
          value: 'E13000002',
        },
        {
          label: 'West Midlands',
          value: 'E12000005',
        },
        {
          label: 'North West',
          value: 'E12000002',
        },
        {
          label: 'Yorkshire and the Humber',
          value: 'E12000003',
        },
        {
          label: 'North East',
          value: 'E12000001',
        },
        {
          label: 'South West',
          value: 'E12000009',
        },
        {
          label: 'East of England',
          value: 'E12000006',
        },
        {
          label: 'South East',
          value: 'E12000008',
        },
        {
          label: 'East Midlands',
          value: 'E12000004',
        },
      ],
    },
  },
  timePeriod: {
    hint: 'Filter statistics by a given start and end date',
    legend: 'Academic Year',
    options: [
      {
        code: 'HT6',
        label: '2012',
        year: 2012,
      },
      {
        code: 'HT6',
        label: '2013',
        year: 2013,
      },
      {
        code: 'HT6',
        label: '2014',
        year: 2014,
      },
      {
        code: 'HT6',
        label: '2015',
        year: 2015,
      },
      {
        code: 'HT6',
        label: '2016',
        year: 2016,
      },
    ],
  },
};

const AbstractChartProps: ChartProps = {
  data,
  meta: metaData,
  labels,
  dataSets: [
    { indicator: '23', filters: ['1', '2'] },
    { indicator: '26', filters: ['1', '2'] },
    { indicator: '28', filters: ['1', '2'] },
  ],
  xAxis: { title: 'test x axis' },
  yAxis: { title: 'test y axis' },
};

const AbstractMultipleChartProps: ChartProps = {
  data: multipleData,
  meta: metaData,
  labels,
  dataSets: [
    { indicator: '23', filters: ['1', '2'] },
    { indicator: '26', filters: ['1', '2'] },
    { indicator: '28', filters: ['1', '2'] },
  ],
  xAxis: { title: 'test x axis' },
  yAxis: { title: 'test y axis' },
};

const response: DataBlockResponse = {
  ...data,
  metaData,
};

export default {
  AbstractChartProps,
  AbstractMultipleChartProps,
  testBlockData: data,
  testBlockMetaData: responseMetadata,
  labels,
  response,
};
