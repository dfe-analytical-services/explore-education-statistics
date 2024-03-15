import { SubjectMeta } from '@common/services/tableBuilderService';

export const testLocationsFlat: SubjectMeta['locations'] = {
  country: {
    legend: 'Country',
    options: [
      {
        id: 'country-1',
        label: 'Country 1',
        value: 'country-1',
      },
    ],
  },
  localAuthority: {
    legend: 'Local authority',
    options: [
      {
        id: 'local-authority-1',
        label: 'Local authority 1',
        value: 'local-authority-1',
      },
      {
        id: 'local-authority-2',
        label: 'Local authority 2',
        value: 'local-authority-2',
      },
      {
        id: 'local-authority-3',
        label: 'Local authority 3',
        value: 'local-authority-3',
      },
    ],
  },
  region: {
    legend: 'Region',
    options: [
      {
        id: 'region-1',
        label: 'Region 1',
        value: 'region-1',
      },
      {
        id: 'region-2',
        label: 'Region 2',
        value: 'region-2',
      },
    ],
  },
};

export const testLocationsNested: SubjectMeta['locations'] = {
  country: {
    legend: 'Country',
    options: [
      {
        id: 'country-1',
        label: 'Country 1',
        value: 'country-1',
      },
    ],
  },
  localAuthority: {
    legend: 'Local authority',
    options: [
      {
        label: 'Region 1',
        value: 'region-1',
        level: 'Region',
        options: [
          {
            id: 'local-authority-1',
            label: 'Local authority 1',
            value: 'local-authority-1',
          },
          {
            id: 'local-authority-2',
            label: 'Local authority 2',
            value: 'local-authority-2',
          },
        ],
      },
      {
        label: 'Region 2',
        value: 'region-2',
        level: 'Region',
        options: [
          {
            id: 'local-authority-3',
            label: 'Local authority 3',
            value: 'local-authority-3',
          },
          {
            id: 'local-authority-4',
            label: 'Local authority 4',
            value: 'local-authority-4',
          },
        ],
      },
    ],
  },
};

export const testSchools: SubjectMeta['locations'] = {
  school: {
    legend: 'Schools',
    options: [
      {
        label: 'LA 1',
        level: 'localAuthority',
        options: [
          { id: 'school-id-1', label: 'School 1', value: '000001' },
          { id: 'school-id-2', label: 'School 2', value: '000002' },
        ],
        value: 'la1',
      },
      {
        label: 'LA 2',
        level: 'localAuthority',
        options: [{ id: 'school-id-3', label: 'School 3', value: '000003' }],
        value: 'la2',
      },
    ],
  },
};
