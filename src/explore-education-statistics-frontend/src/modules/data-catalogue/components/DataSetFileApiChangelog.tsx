import ApiDataSetChangelog from '@common/modules/data-catalogue/components/ApiDataSetChangelog';
import { ApiDataSetVersionChanges } from '@common/services/types/apiDataSetChanges';
import DataSetFilePageSection from '@frontend/modules/data-catalogue/components/DataSetFilePageSection';
import { pageSections } from '@frontend/modules/data-catalogue/DataSetFilePage';
import React from 'react';

const changes: ApiDataSetVersionChanges = {
  majorChanges: {
    filters: [
      {
        previousState: { id: 'filter_3', label: 'Filter 3', hint: '' },
      },
    ],
    filterOptions: [
      {
        filter: { id: 'filter_2', label: 'Filter 2', hint: '' },
        options: [
          { previousState: { id: 'abc1', label: 'Filter option 1' } },
          { previousState: { id: 'abc2', label: 'Filter option 2' } },
        ],
      },
      {
        filter: { id: 'filter_1', label: 'Filter 1', hint: '' },
        options: [
          { previousState: { id: 'abc3', label: 'Filter option 3' } },
          { previousState: { id: 'abc4', label: 'Filter option 4' } },
        ],
      },
    ],
    geographicLevels: [
      { previousState: { code: 'LAD', label: 'Local Authority District' } },
    ],
    indicators: [
      { previousState: { id: 'indicator_1', label: 'Indicator 1' } },
    ],
    locationGroups: [
      {
        previousState: {
          level: { code: 'LAD', label: 'Local Authority District' },
        },
      },
    ],
    locationOptions: [
      {
        level: { code: 'LA', label: 'Local Authority' },
        options: [
          {
            previousState: {
              id: 'def1',
              code: 'E09000004',
              label: 'Richmond upon Thames',
            },
          },
        ],
      },
      {
        level: { code: 'SCH', label: 'School' },
        options: [
          {
            previousState: {
              id: 'def2',
              urn: '101269',
              laEstab: '3022014',
              label: 'Colindale Primary School',
            },
          },
        ],
      },
    ],
    timePeriods: [
      { previousState: { code: 'AY', label: '2017/18', period: '2017/2018' } },
    ],
  },
  minorChanges: {
    filters: [
      {
        currentState: { id: 'filter_1', label: 'Filter 1A', hint: '' },
        previousState: {
          id: 'filter_1',
          label: 'Filter 1',
          hint: '',
        },
      },
      {
        currentState: { id: 'filter_4', label: 'Filter 4', hint: '' },
      },
    ],
    filterOptions: [
      {
        filter: { id: 'filter_1', label: 'Filter 1A', hint: '' },
        options: [
          {
            previousState: { id: 'abc5', label: 'Filter option 5' },
            currentState: { id: 'abc5', label: 'Filter option 5A' },
          },
        ],
      },
      {
        filter: { id: 'filter_2', label: 'Filter 2', hint: '' },
        options: [{ currentState: { id: 'abc6', label: 'Filter option 6' } }],
      },
    ],
    geographicLevels: [
      { currentState: { code: 'LAD', label: 'Local Authority' } },
    ],
    indicators: [
      {
        previousState: { id: 'indicator_2', label: 'Indicator 2' },
        currentState: { id: 'indicator_2', label: 'Indicator 2A' },
      },
    ],
    locationGroups: [
      {
        currentState: {
          level: { code: 'LAD', label: 'Local Authority' },
        },
      },
    ],
    locationOptions: [
      {
        level: { code: 'LA', label: 'Local Authority' },
        options: [
          {
            currentState: { id: 'def3', code: 'E08000019', label: 'Sheffield' },
          },
          {
            currentState: {
              id: 'ghi1',
              code: 'E09000003',
              label: 'Kingston upon Thames / Richmond upon Thames',
            },
            previousState: {
              id: 'ghi1',
              code: 'E09000003',
              label: 'Kingston upon Thames',
            },
          },
        ],
      },
      {
        level: { code: 'SCH', label: 'School' },
        options: [
          {
            currentState: {
              id: 'ghi2',
              urn: '135507',
              laEstab: '3026906',
              label: 'Wren Academy Finchley',
            },
            previousState: {
              id: 'ghi2',
              urn: '135507',
              laEstab: '3026906',
              label: 'Wren Academy',
            },
          },
          {
            currentState: {
              id: 'ghi3',
              urn: '141862',
              laEstab: '3144001',
              label: 'The Kingston Academy',
            },
          },
        ],
      },
    ],
    timePeriods: [
      { currentState: { code: 'AY', label: '2023/24', period: '2023/2024' } },
    ],
  },
};

interface Props {
  version: string;
}

export default function DataSetFileApiChangelog({ version }: Props) {
  return (
    <DataSetFilePageSection
      heading={pageSections.apiChangelog}
      id="apiChangelog"
    >
      <ApiDataSetChangelog
        majorChanges={changes.majorChanges}
        minorChanges={changes.minorChanges}
      />
    </DataSetFilePageSection>
  );
}
