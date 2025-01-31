import {
  SubjectMeta,
  TableDataResponse,
} from '@common/services/tableBuilderService';

export const testSubjectMeta: SubjectMeta = {
  filters: {
    Characteristic: {
      id: 'characteristic',
      totalValue: '',
      hint: 'Filter by pupil characteristic',
      legend: 'Characteristic',
      name: 'characteristic',
      options: {
        Gender: {
          id: 'gender',
          label: 'Gender',
          options: [
            {
              label: 'Gender female',
              value: 'gender-female',
            },
          ],
          order: 0,
        },
      },
      order: 0,
    },
  },
  indicators: {
    AbsenceFields: {
      id: 'absence',
      label: 'Absence fields',
      options: [
        {
          value: 'authorised-absence-sessions',
          label: 'Number of authorised absence sessions',
          unit: '',
          name: 'sess_authorised',
          decimalPlaces: 2,
        },
      ],
      order: 0,
    },
  },
  locations: {
    country: {
      legend: 'Country',
      options: [{ id: 'england', value: 'england', label: 'England' }],
    },
    localAuthority: {
      legend: 'Local authority',
      options: [{ id: 'barnet', value: 'barnet', label: 'Barnet' }],
    },
  },
  timePeriod: {
    legend: 'Time period',
    options: [{ label: '2020/21', code: 'AY', year: 2020 }],
  },
};

export const testTableData: TableDataResponse = {
  subjectMeta: {
    publicationName: '',
    boundaryLevels: [],
    footnotes: [],
    subjectName: 'Subject 1',
    geoJsonAvailable: false,
    locations: {
      localAuthority: [
        {
          id: 'barnet',
          label: 'Barnet',
          value: 'barnet',
        },
      ],
    },
    timePeriodRange: [{ code: 'AY', year: 2020, label: '2020/21' }],
    indicators: [
      {
        value: 'authorised-absence-sessions',
        label: 'Number of authorised absence sessions',
        unit: '',
        name: 'sess_authorised',
        decimalPlaces: 2,
      },
    ],
    filters: {
      Characteristic: {
        totalValue: '',
        hint: 'Filter by pupil characteristic',
        legend: 'Characteristic',
        name: 'characteristic',
        options: {
          Gender: {
            id: 'gender',
            label: 'Gender',
            options: [
              {
                label: 'Gender female',
                value: 'gender-female',
              },
            ],
            order: 0,
          },
        },
        order: 0,
      },
    },
  },
  results: [
    {
      timePeriod: '2020_AY',
      measures: {
        'authorised-absence-sessions': '123',
      },
      locationId: 'barnet',
      geographicLevel: 'localAuthority',
      filters: ['gender-female'],
    },
  ],
};
