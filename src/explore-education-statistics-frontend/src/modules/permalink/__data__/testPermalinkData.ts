import { Permalink } from '@common/services/permalinkService';

export const testPermalink: Permalink = {
  id: 'permalink-1',
  status: 'Current',
  created: '2020-10-07T12:00:00.00Z',
  configuration: {
    tableHeaders: {
      columnGroups: [[{ type: 'TimePeriod', value: '2020_AY' }]],
      columns: [{ type: 'Filter', value: 'gender-female' }],
      rowGroups: [
        [{ type: 'Location', value: 'barnet-id', level: 'localAuthority' }],
      ],
      rows: [{ type: 'Indicator', value: 'authorised-absence-sessions' }],
    },
  },
  fullTable: {
    subjectMeta: {
      publicationName: 'Test publication',
      boundaryLevels: [],
      footnotes: [],
      subjectName: 'Subject 1',
      geoJsonAvailable: false,
      locations: {
        localAuthority: [
          {
            id: 'barnet-id',
            label: 'Barnet',
            value: 'E09000003',
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
        locationId: 'barnet-id',
        geographicLevel: 'localAuthority',
        filters: ['gender-female'],
      },
    ],
  },
};

export const testPermalinkWithHierarchicalLocations: Permalink = {
  id: 'permalink-1',
  status: 'Current',
  created: '2020-10-07T12:00:00.00Z',
  configuration: {
    tableHeaders: {
      columnGroups: [[{ type: 'TimePeriod', value: '2020_AY' }]],
      columns: [{ type: 'Filter', value: 'gender-female' }],
      rowGroups: [
        [
          {
            level: 'localAuthority',
            value: 'barnet-id',
            type: 'Location',
          },
          {
            level: 'localAuthority',
            value: 'bolton-id',
            type: 'Location',
          },
        ],
      ],
      rows: [{ type: 'Indicator', value: 'authorised-absence-sessions' }],
    },
  },
  fullTable: {
    subjectMeta: {
      publicationName: 'Test publication',
      boundaryLevels: [],
      footnotes: [],
      subjectName: 'Subject 1',
      geoJsonAvailable: false,
      locations: {
        localAuthority: [
          {
            label: 'North West',
            level: 'region',
            value: 'E12000002',
            options: [
              {
                id: 'bolton-id',
                label: 'Bolton',
                value: 'E08000001',
              },
            ],
          },
          {
            label: 'Outer London',
            level: 'region',
            value: 'E13000002',
            options: [
              {
                id: 'barnet-id',
                label: 'Barnet',
                value: 'E09000003',
              },
            ],
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
        locationId: 'barnet-id',
        geographicLevel: 'localAuthority',
        filters: ['gender-female'],
      },
      {
        timePeriod: '2020_AY',
        measures: {
          'authorised-absence-sessions': '456',
        },
        locationId: 'bolton-id',
        geographicLevel: 'localAuthority',
        filters: ['gender-female'],
      },
    ],
  },
};

// This Permalink has no location id's. This is the case for the majority of Permalinks created prior to
// EES-2955 which switched over to using location id's.
export const testPermalinkWithLocationCodes: Permalink = {
  id: 'permalink-1',
  status: 'Current',
  created: '2020-10-07T12:00:00.00Z',
  configuration: {
    tableHeaders: {
      columnGroups: [[{ type: 'TimePeriod', value: '2020_AY' }]],
      columns: [{ type: 'Filter', value: 'gender-female' }],
      rowGroups: [
        [{ type: 'Location', value: 'E09000003', level: 'localAuthority' }],
      ],
      rows: [{ type: 'Indicator', value: 'authorised-absence-sessions' }],
    },
  },
  fullTable: {
    subjectMeta: {
      publicationName: 'Test publication',
      boundaryLevels: [],
      footnotes: [],
      subjectName: 'Subject 1',
      geoJsonAvailable: false,
      locations: {
        localAuthority: [
          {
            label: 'Barnet',
            value: 'E09000003',
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
        location: {
          localAuthority: {
            name: 'Barnet',
            code: 'E09000003',
          },
        },
        geographicLevel: 'localAuthority',
        filters: ['gender-female'],
      },
    ],
  },
};

// This Permalink has location id's in results. It still has location codes in subject meta data and results.
// This was a possible state after EES-3203 added 'locationId' to results to make testing easier,
// but before EES-2955 switched over to using location id's.
export const testPermalinkWithResultLocationIds: Permalink = {
  id: 'permalink-1',
  status: 'Current',
  created: '2020-10-07T12:00:00.00Z',
  configuration: {
    tableHeaders: {
      columnGroups: [[{ type: 'TimePeriod', value: '2020_AY' }]],
      columns: [{ type: 'Filter', value: 'gender-female' }],
      rowGroups: [
        [{ type: 'Location', value: 'E09000003', level: 'localAuthority' }],
      ],
      rows: [{ type: 'Indicator', value: 'authorised-absence-sessions' }],
    },
  },
  fullTable: {
    subjectMeta: {
      publicationName: 'Test publication',
      boundaryLevels: [],
      footnotes: [],
      subjectName: 'Subject 1',
      geoJsonAvailable: false,
      locations: {
        localAuthority: [
          {
            label: 'Barnet',
            value: 'E09000003',
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
        locationId: 'barnet-id',
        location: {
          localAuthority: {
            name: 'Barnet',
            code: 'E09000003',
          },
        },
        geographicLevel: 'localAuthority',
        filters: ['gender-female'],
      },
    ],
  },
};

export default testPermalink;
