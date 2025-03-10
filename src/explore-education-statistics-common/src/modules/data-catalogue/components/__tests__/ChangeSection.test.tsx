import ChangeSection from '@common/modules/data-catalogue/components/ChangeSection';
import { ChangeSet } from '@common/services/types/apiDataSetChanges';
import { render, screen, within } from '@testing-library/react';
import React from 'react';

describe('ChangeSection', () => {
  test('renders filter changes only', () => {
    const testChanges: ChangeSet = {
      filters: [
        {
          previousState: {
            id: 'filter-1',
            column: 'filter_1',
            label: 'Filter 1',
            hint: '',
          },
        },
        {
          previousState: {
            id: 'filter-2',
            column: 'filter_2',
            label: 'Filter 2',
            hint: '',
          },
        },
        {
          previousState: {
            id: 'filter-3',
            column: 'filter_3',
            label: 'Filter 3',
            hint: '',
          },
          currentState: {
            id: 'filter-3-updated',
            column: 'filter_3_updated',
            label: 'Filter 3',
            hint: '',
          },
        },
        {
          previousState: {
            id: 'filter-4',
            column: 'filter_4',
            label: 'Filter 4',
            hint: 'Filter 4 hint',
          },
          currentState: {
            id: 'filter-4',
            column: 'filter_4',
            label: 'Filter 4 updated',
            hint: 'Filter 4 hint updated',
          },
        },
        {
          currentState: {
            id: 'filter-5',
            column: 'filter_5',
            label: 'Filter 5',
            hint: '',
          },
        },
        {
          currentState: {
            id: 'filter-6',
            column: 'filter_6',
            label: 'Filter 6',
            hint: '',
          },
        },
      ],
    };

    render(<ChangeSection changes={testChanges} />);

    expect(
      screen.getByRole('heading', { name: 'Changes to filters' }),
    ).toBeInTheDocument();

    const deleted = within(screen.getByTestId('deleted-filters')).getAllByRole(
      'listitem',
    );

    expect(deleted).toHaveLength(2);
    expect(deleted[0]).toHaveTextContent(
      'Filter 1 (id: filter-1, column: filter_1)',
    );
    expect(deleted[1]).toHaveTextContent(
      'Filter 2 (id: filter-2, column: filter_2)',
    );

    const updated = within(
      screen.getByTestId('updated-filters'),
    ).getAllByTestId('updated-item');

    expect(updated).toHaveLength(2);
    expect(updated[0]).toHaveTextContent(
      'Filter 3 (id: filter-3, column: filter_3)',
    );

    const updated1Changes = within(updated[0]).getAllByRole('listitem');

    expect(updated1Changes).toHaveLength(2);
    expect(updated1Changes[0]).toHaveTextContent(
      'id changed to: filter-3-updated',
    );
    expect(updated1Changes[1]).toHaveTextContent(
      'column changed to: filter_3_updated',
    );

    expect(updated[1]).toHaveTextContent(
      'Filter 4 (id: filter-4, column: filter_4)',
    );

    const updated2Changes = within(updated[1]).getAllByRole('listitem');

    expect(updated2Changes).toHaveLength(2);
    expect(updated2Changes[0]).toHaveTextContent(
      'label changed to: Filter 4 updated',
    );
    expect(updated2Changes[1]).toHaveTextContent(
      'hint changed to: Filter 4 hint',
    );

    const added = within(screen.getByTestId('added-filters')).getAllByRole(
      'listitem',
    );

    expect(added).toHaveLength(2);
    expect(added[0]).toHaveTextContent(
      'Filter 5 (id: filter-5, column: filter_5)',
    );
    expect(added[1]).toHaveTextContent(
      'Filter 6 (id: filter-6, column: filter_6)',
    );
  });

  test('renders filter option changes only', () => {
    const testChanges: ChangeSet = {
      filterOptions: [
        {
          filter: {
            id: 'filter-1',
            column: 'filter_1',
            label: 'Filter 1',
            hint: '',
          },
          options: [
            { previousState: { id: 'filter-opt-1', label: 'Filter option 1' } },
            { previousState: { id: 'filter-opt-2', label: 'Filter option 2' } },
            {
              previousState: { id: 'filter-opt-3', label: 'Filter option 3' },
              currentState: {
                id: 'filter-opt-3-updated',
                label: 'Filter option 3',
              },
            },
            {
              previousState: { id: 'filter-opt-4', label: 'Filter option 4' },
              currentState: {
                id: 'filter-opt-4',
                label: 'Filter option 4 updated',
              },
            },
            { currentState: { id: 'filter-opt-5', label: 'Filter option 5' } },
            { currentState: { id: 'filter-opt-6', label: 'Filter option 6' } },
          ],
        },
        {
          filter: {
            id: 'filter-2',
            column: 'filter_2',
            label: 'Filter 2',
            hint: '',
          },
          options: [
            { previousState: { id: 'filter-opt-7', label: 'Filter option 7' } },
            {
              previousState: { id: 'filter-opt-8', label: 'Filter option 8' },
              currentState: {
                id: 'filter-opt-8',
                label: 'Filter option 8',
              },
            },
            { currentState: { id: 'filter-opt-9', label: 'Filter option 9' } },
          ],
        },
      ],
    };

    render(<ChangeSection changes={testChanges} />);

    expect(
      screen.getByRole('heading', {
        name: 'Changes to Filter 1 filter options',
      }),
    ).toBeInTheDocument();

    const deletedFilter1Options = within(
      screen.getByTestId('deleted-filterOptions-filter-1'),
    ).getAllByRole('listitem');

    expect(deletedFilter1Options).toHaveLength(2);
    expect(deletedFilter1Options[0]).toHaveTextContent(
      'Filter option 1 (id: filter-opt-1)',
    );
    expect(deletedFilter1Options[1]).toHaveTextContent(
      'Filter option 2 (id: filter-opt-2)',
    );

    const updatedFilter1Options = within(
      screen.getByTestId('updated-filterOptions-filter-1'),
    ).getAllByTestId('updated-item');

    expect(updatedFilter1Options).toHaveLength(2);
    expect(updatedFilter1Options[0]).toHaveTextContent(
      'Filter option 3 (id: filter-opt-3)',
    );

    const updatedFilter1Option1Changes = within(
      updatedFilter1Options[0],
    ).getAllByRole('listitem');

    expect(updatedFilter1Option1Changes).toHaveLength(1);
    expect(updatedFilter1Option1Changes[0]).toHaveTextContent(
      'id changed to: filter-opt-3-updated',
    );

    expect(updatedFilter1Options[1]).toHaveTextContent(
      'Filter option 4 (id: filter-opt-4)',
    );

    const updatedFilter1Option2Changes = within(
      updatedFilter1Options[1],
    ).getAllByRole('listitem');

    expect(updatedFilter1Option2Changes).toHaveLength(1);
    expect(updatedFilter1Option2Changes[0]).toHaveTextContent(
      'label changed to: Filter option 4 updated',
    );

    const addedFilter1Options = within(
      screen.getByTestId('added-filterOptions-filter-1'),
    ).getAllByRole('listitem');

    expect(addedFilter1Options).toHaveLength(2);
    expect(addedFilter1Options[0]).toHaveTextContent(
      'Filter option 5 (id: filter-opt-5)',
    );
    expect(addedFilter1Options[1]).toHaveTextContent(
      'Filter option 6 (id: filter-opt-6)',
    );

    const deletedFilter2Options = within(
      screen.getByTestId('deleted-filterOptions-filter-2'),
    ).getAllByRole('listitem');

    expect(deletedFilter2Options).toHaveLength(1);
    expect(deletedFilter2Options[0]).toHaveTextContent(
      'Filter option 7 (id: filter-opt-7)',
    );

    const updatedFilter2Options = within(
      screen.getByTestId('updated-filterOptions-filter-2'),
    ).getAllByTestId('updated-item');

    expect(updatedFilter2Options).toHaveLength(1);
    expect(updatedFilter2Options[0]).toHaveTextContent(
      'Filter option 8 (id: filter-opt-8)',
    );

    const updatedFilter2Option1Changes = within(
      updatedFilter2Options[0],
    ).queryAllByRole('listitem');

    expect(updatedFilter2Option1Changes).toHaveLength(0);

    const addedFilter2Options = within(
      screen.getByTestId('added-filterOptions-filter-2'),
    ).getAllByRole('listitem');

    expect(addedFilter2Options).toHaveLength(1);
    expect(addedFilter2Options[0]).toHaveTextContent(
      'Filter option 9 (id: filter-opt-9)',
    );
  });

  test('renders geographic level changes only', () => {
    const testChanges: ChangeSet = {
      geographicLevels: [
        { previousState: { code: 'NAT', label: 'National' } },
        { previousState: { code: 'WARD', label: 'Ward' } },
        {
          previousState: { code: 'LA', label: 'Local authority' },
          currentState: { code: 'LAD', label: 'Local authority district' },
        },
        {
          previousState: { code: 'OA', label: 'Opportunity area' },
          currentState: { code: 'PA', label: 'Planning area' },
        },
        { currentState: { code: 'NAT', label: 'National' } },
        { currentState: { code: 'REG', label: 'Regional' } },
      ],
    };

    render(<ChangeSection changes={testChanges} />);

    expect(
      screen.getByRole('heading', {
        name: 'Changes to geographic level options',
      }),
    ).toBeInTheDocument();

    const deleted = within(
      screen.getByTestId('deleted-geographicLevels'),
    ).getAllByRole('listitem');

    expect(deleted).toHaveLength(2);
    expect(deleted[0]).toHaveTextContent('National (code: NAT)');
    expect(deleted[1]).toHaveTextContent('Ward (code: WARD)');

    const updated = within(
      screen.getByTestId('updated-geographicLevels'),
    ).getAllByTestId('updated-item');

    expect(updated).toHaveLength(2);
    expect(updated[0]).toHaveTextContent('Local authority (code: LA)');

    const updated1Changes = within(updated[0]).getAllByRole('listitem');

    expect(updated1Changes).toHaveLength(1);
    expect(updated1Changes[0]).toHaveTextContent(
      'changed to: Local authority district (code: LAD)',
    );

    expect(updated[1]).toHaveTextContent('Opportunity area (code: OA)');

    const updated2Changes = within(updated[1]).getAllByRole('listitem');

    expect(updated2Changes).toHaveLength(1);
    expect(updated2Changes[0]).toHaveTextContent(
      'changed to: Planning area (code: PA)',
    );

    const added = within(
      screen.getByTestId('added-geographicLevels'),
    ).getAllByRole('listitem');

    expect(added).toHaveLength(2);
    expect(added[0]).toHaveTextContent('National (code: NAT)');
    expect(added[1]).toHaveTextContent('Regional (code: REG)');
  });

  test('renders indicators changes only', () => {
    const testChanges: ChangeSet = {
      indicators: [
        {
          previousState: {
            id: 'indicator-1',
            column: 'indicator_1',
            label: 'Indicator 1',
          },
        },
        {
          previousState: {
            id: 'indicator-2',
            column: 'indicator_2',
            label: 'Indicator 2',
          },
        },
        {
          currentState: {
            id: 'indicator-3-updated',
            column: 'indicator_3_updated',
            label: 'Indicator 3',
          },
          previousState: {
            id: 'indicator-3',
            column: 'indicator_3',
            label: 'Indicator 3',
          },
        },
        {
          currentState: {
            id: 'indicator-4',
            column: 'indicator_4',
            label: 'Indicator 4 updated',
            unit: '%',
          },
          previousState: {
            id: 'indicator-4',
            column: 'indicator_4',
            label: 'Indicator 4',
          },
        },
        {
          currentState: {
            id: 'indicator-6',
            column: 'indicator_6',
            label: 'Indicator 6',
          },
        },
        {
          currentState: {
            id: 'indicator-7',
            column: 'indicator_7',
            label: 'Indicator 7',
          },
        },
      ],
    };

    render(<ChangeSection changes={testChanges} />);

    expect(
      screen.getByRole('heading', { name: 'Changes to indicators' }),
    ).toBeInTheDocument();

    const deleted = within(
      screen.getByTestId('deleted-indicators'),
    ).getAllByRole('listitem');

    expect(deleted).toHaveLength(2);
    expect(deleted[0]).toHaveTextContent('Indicator 1 (id: indicator-1)');
    expect(deleted[1]).toHaveTextContent('Indicator 2 (id: indicator-2)');

    const updated = within(
      screen.getByTestId('updated-indicators'),
    ).getAllByTestId('updated-item');

    expect(updated).toHaveLength(2);
    expect(updated[0]).toHaveTextContent(
      'Indicator 3 (id: indicator-3, column: indicator_3)',
    );

    const updated1Changes = within(updated[0]).getAllByRole('listitem');

    expect(updated1Changes).toHaveLength(2);
    expect(updated1Changes[0]).toHaveTextContent(
      'id changed to: indicator-3-updated',
    );
    expect(updated1Changes[1]).toHaveTextContent(
      'column changed to: indicator_3_updated',
    );

    expect(updated[1]).toHaveTextContent(
      'Indicator 4 (id: indicator-4, column: indicator_4)',
    );

    const updated2Changes = within(updated[1]).getAllByRole('listitem');

    expect(updated2Changes).toHaveLength(2);
    expect(updated2Changes[0]).toHaveTextContent(
      'label changed to: Indicator 4 updated',
    );
    expect(updated2Changes[1]).toHaveTextContent('unit changed to: %');

    const added = within(screen.getByTestId('added-indicators')).getAllByRole(
      'listitem',
    );

    expect(added).toHaveLength(2);
    expect(added[0]).toHaveTextContent('Indicator 6 (id: indicator-6)');
    expect(added[1]).toHaveTextContent('Indicator 7 (id: indicator-7)');
  });

  test('renders location group changes only', () => {
    const testChanges: ChangeSet = {
      locationGroups: [
        { previousState: { level: { code: 'INST', label: 'Institution' } } },
        { previousState: { level: { code: 'WARD', label: 'Ward' } } },
        {
          previousState: { level: { code: 'LA', label: 'Local authority' } },
          currentState: {
            level: { code: 'LAD', label: 'Local authority district' },
          },
        },
        {
          previousState: { level: { code: 'OA', label: 'Opportunity area' } },
          currentState: { level: { code: 'PA', label: 'Planning area' } },
        },
        { currentState: { level: { code: 'NAT', label: 'National' } } },
        { currentState: { level: { code: 'REG', label: 'Regional' } } },
      ],
    };

    render(<ChangeSection changes={testChanges} />);

    expect(
      screen.getByRole('heading', { name: 'Changes to location groups' }),
    ).toBeInTheDocument();

    const deleted = within(
      screen.getByTestId('deleted-locationGroups'),
    ).getAllByRole('listitem');

    expect(deleted).toHaveLength(2);

    expect(deleted[0]).toHaveTextContent('Institution (code: INST)');
    expect(deleted[1]).toHaveTextContent('Ward (code: WARD)');

    const updated = within(
      screen.getByTestId('updated-locationGroups'),
    ).getAllByTestId('updated-item');

    expect(updated).toHaveLength(2);

    expect(updated[0]).toHaveTextContent('Local authority (code: LA)');

    const updated1Changes = within(updated[0]).getAllByRole('listitem');

    expect(updated1Changes).toHaveLength(1);
    expect(updated1Changes[0]).toHaveTextContent(
      'changed to: Local authority district (code: LAD)',
    );

    expect(updated[1]).toHaveTextContent('Opportunity area (code: OA)');

    const updated2Changes = within(updated[1]).getAllByRole('listitem');

    expect(updated2Changes).toHaveLength(1);
    expect(updated2Changes[0]).toHaveTextContent(
      'changed to: Planning area (code: PA)',
    );

    const added = within(
      screen.getByTestId('added-locationGroups'),
    ).getAllByRole('listitem');

    expect(added).toHaveLength(2);

    expect(added[0]).toHaveTextContent('National (code: NAT)');
    expect(added[1]).toHaveTextContent('Regional (code: REG)');
  });

  test('renders location option changes only', () => {
    const testChanges: ChangeSet = {
      locationOptions: [
        {
          level: { code: 'REG', label: 'Regional' },
          options: [
            {
              previousState: {
                id: 'location-1',
                code: 'location-1-code',
                label: 'Location 1',
              },
            },
            {
              previousState: {
                id: 'location-2',
                code: 'location-2-code',
                label: 'Location 2',
              },
            },
            {
              previousState: {
                id: 'location-3',
                code: 'location-3-code',
                label: 'Location 3',
                oldCode: 'location-3-oldCode',
              },
              currentState: {
                id: 'location-3-updated',
                label: 'Location 3',
                code: 'location-3-code-updated',
                oldCode: 'location-3-oldCode-updated',
              },
            },
            {
              previousState: {
                id: 'location-4',
                label: 'Location 4',
                ukprn: 'location-4-ukprn',
              },
              currentState: {
                id: 'location-4',
                label: 'Location 4 updated',
                ukprn: 'location-4-ukprn-updated',
              },
            },
            {
              currentState: {
                id: 'location-5',
                code: 'location-5-code',
                label: 'Location 5',
              },
            },
            {
              currentState: {
                id: 'location-6',
                code: 'location-6-code',
                label: 'Location 6',
              },
            },
          ],
        },
        {
          level: { code: 'SCH', label: 'School' },
          options: [
            {
              previousState: {
                id: 'location-7',
                urn: 'location-7-urn',
                laEstab: 'location-7-laEstab',
                label: 'Location 7',
              },
            },
            {
              currentState: {
                id: 'location-8-updated',
                label: 'Location 8 updated',
                laEstab: 'location-8-laEstab-updated',
                urn: 'location-8-urn-updated',
              },
              previousState: {
                id: 'location-8',
                label: 'Location 8',
                laEstab: 'location-8-laEstab',
                urn: 'location-8-urn',
              },
            },
            {
              currentState: {
                id: 'location-9',
                label: 'Location 9',
                code: 'location-9-code',
              },
            },
          ],
        },
      ],
    };

    render(<ChangeSection changes={testChanges} />);

    expect(
      screen.getByRole('heading', {
        name: 'Changes to Regional location options',
      }),
    ).toBeInTheDocument();

    const deletedRegionOptions = within(
      screen.getByTestId('deleted-locationOptions-REG'),
    ).getAllByRole('listitem');

    expect(deletedRegionOptions).toHaveLength(2);
    expect(deletedRegionOptions[0]).toHaveTextContent(
      'Location 1 (id: location-1, code: location-1-code)',
    );
    expect(deletedRegionOptions[1]).toHaveTextContent(
      'Location 2 (id: location-2, code: location-2-code)',
    );

    const updatedRegionOptions = within(
      screen.getByTestId('updated-locationOptions-REG'),
    ).getAllByTestId('updated-item');

    expect(updatedRegionOptions).toHaveLength(2);

    expect(updatedRegionOptions[0]).toHaveTextContent(
      'Location 3 (id: location-3, code: location-3-code, old code: location-3-oldCode)',
    );

    const updatedRegionOption1Changes = within(
      updatedRegionOptions[0],
    ).getAllByRole('listitem');

    expect(updatedRegionOption1Changes).toHaveLength(3);
    expect(updatedRegionOption1Changes[0]).toHaveTextContent(
      'id changed to: location-3-updated',
    );
    expect(updatedRegionOption1Changes[1]).toHaveTextContent(
      'code changed to: location-3-code-updated',
    );
    expect(updatedRegionOption1Changes[2]).toHaveTextContent(
      'old code changed to: location-3-oldCode-updated',
    );

    expect(updatedRegionOptions[1]).toHaveTextContent(
      'Location 4 (id: location-4, UKPRN: location-4-ukprn)',
    );

    const updatedRegionOption2Changes = within(
      updatedRegionOptions[1],
    ).getAllByRole('listitem');

    expect(updatedRegionOption2Changes).toHaveLength(2);
    expect(updatedRegionOption2Changes[0]).toHaveTextContent(
      'label changed to: Location 4 updated',
    );
    expect(updatedRegionOption2Changes[1]).toHaveTextContent(
      'UKPRN changed to: location-4-ukprn-updated',
    );

    const addedRegionOptions = within(
      screen.getByTestId('added-locationOptions-REG'),
    ).getAllByRole('listitem');

    expect(addedRegionOptions).toHaveLength(2);

    expect(addedRegionOptions[0]).toHaveTextContent(
      'Location 5 (id: location-5, code: location-5-code)',
    );
    expect(addedRegionOptions[1]).toHaveTextContent(
      'Location 6 (id: location-6, code: location-6-code)',
    );

    expect(
      screen.getByRole('heading', {
        name: 'Changes to School location options',
      }),
    ).toBeInTheDocument();

    const deletedSchoolOptions = within(
      screen.getByTestId('deleted-locationOptions-SCH'),
    ).getAllByRole('listitem');

    expect(deletedSchoolOptions).toHaveLength(1);
    expect(deletedSchoolOptions[0]).toHaveTextContent(
      'Location 7 (id: location-7, URN: location-7-urn, LAESTAB: location-7-laEstab)',
    );

    const updatedSchoolOptions = within(
      screen.getByTestId('updated-locationOptions-SCH'),
    ).getAllByTestId('updated-item');

    expect(updatedSchoolOptions).toHaveLength(1);
    expect(updatedSchoolOptions[0]).toHaveTextContent(
      'Location 8 (id: location-8, URN: location-8-urn, LAESTAB: location-8-laEstab)',
    );

    const updatedSchoolOption1Changes = within(
      updatedSchoolOptions[0],
    ).getAllByRole('listitem');

    expect(updatedSchoolOption1Changes).toHaveLength(4);
    expect(updatedSchoolOption1Changes[0]).toHaveTextContent(
      'label changed to: Location 8 updated',
    );
    expect(updatedSchoolOption1Changes[1]).toHaveTextContent(
      'id changed to: location-8-updated',
    );
    expect(updatedSchoolOption1Changes[2]).toHaveTextContent(
      'URN changed to: location-8-urn-updated',
    );
    expect(updatedSchoolOption1Changes[3]).toHaveTextContent(
      'LAESTAB changed to: location-8-laEstab-updated',
    );

    const addedSchoolOptions = within(
      screen.getByTestId('added-locationOptions-SCH'),
    ).getAllByRole('listitem');

    expect(addedSchoolOptions).toHaveLength(1);
    expect(addedSchoolOptions[0]).toHaveTextContent(
      'Location 9 (id: location-9, code: location-9-code)',
    );
  });

  test('renders time period changes only', () => {
    const testChanges: ChangeSet = {
      timePeriods: [
        {
          previousState: { code: 'AY', label: '2017/18', period: '2017/2018' },
        },
        {
          previousState: { code: 'AY', label: '2018/19', period: '2018/2019' },
        },
        {
          previousState: { code: 'AY', label: '2019/20', period: '2019/2020' },
          currentState: { code: 'AY', label: '2020/21', period: '2020/2021' },
        },
        {
          previousState: { code: 'RY', label: '2021', period: '2021' },
          currentState: { code: 'CY', label: '2021', period: '2021' },
        },
        { currentState: { code: 'AY', label: '2022/23', period: '2022/2023' } },
        { currentState: { code: 'AY', label: '2023/24', period: '2023/2024' } },
      ],
    };

    render(<ChangeSection changes={testChanges} />);

    expect(
      screen.getByRole('heading', { name: 'Changes to time periods' }),
    ).toBeInTheDocument();

    const deleted = within(
      screen.getByTestId('deleted-timePeriods'),
    ).getAllByRole('listitem');

    expect(deleted).toHaveLength(2);
    expect(deleted[0]).toHaveTextContent('2017/18 (code: AY)');
    expect(deleted[1]).toHaveTextContent('2018/19 (code: AY)');

    const updated = within(
      screen.getByTestId('updated-timePeriods'),
    ).getAllByTestId('updated-item');

    expect(updated).toHaveLength(2);

    expect(updated[0]).toHaveTextContent('2019/20 (code: AY)');
    expect(updated[0]).toHaveTextContent('changed to: 2020/21 (code: AY)');

    expect(updated[1]).toHaveTextContent('2021 (code: RY)');
    expect(updated[1]).toHaveTextContent('changed to: 2021 (code: CY)');

    const added = within(screen.getByTestId('added-timePeriods')).getAllByRole(
      'listitem',
    );

    expect(added).toHaveLength(2);

    expect(added[0]).toHaveTextContent('2022/23 (code: AY)');
    expect(added[1]).toHaveTextContent('2023/24 (code: AY)');
  });

  test('renders changes for each meta type', () => {
    const testChanges: ChangeSet = {
      filters: [
        {
          previousState: {
            id: 'filter-1',
            column: 'filter_1',
            label: 'Filter 1',
            hint: '',
          },
          currentState: {
            id: 'filter-1',
            column: 'filter_1_updated',
            label: 'Filter 1 updated',
            hint: '',
          },
        },
      ],
      filterOptions: [
        {
          filter: {
            id: 'filter-1',
            column: 'filter_1',
            label: 'Filter 1',
            hint: '',
          },
          options: [
            { previousState: { id: 'filter-opt-1', label: 'Filter option 1' } },
          ],
        },
      ],
      geographicLevels: [
        { previousState: { code: 'LAD', label: 'Local authority district' } },
      ],
      indicators: [
        {
          currentState: {
            id: 'indicator-1',
            column: 'indicator_1_updated',
            label: 'Indicator 1 updated',
          },
          previousState: {
            id: 'indicator-1',
            column: 'indicator_1',
            label: 'Indicator 1',
          },
        },
      ],
      locationGroups: [
        { currentState: { level: { code: 'INST', label: 'Institution' } } },
      ],
      locationOptions: [
        {
          level: { code: 'REG', label: 'Regional' },
          options: [
            {
              currentState: {
                id: 'location-1',
                code: 'location-1-code',
                label: 'Location 1',
              },
            },
          ],
        },
      ],
      timePeriods: [
        {
          currentState: { code: 'AY', label: '2017/18', period: '2017/2018' },
        },
      ],
    };

    render(<ChangeSection changes={testChanges} />);

    expect(
      screen.getByRole('heading', { name: 'Updated filters' }),
    ).toBeInTheDocument();

    const updatedFilters = within(
      screen.getByTestId('updated-filters'),
    ).getAllByTestId('updated-item');

    expect(updatedFilters).toHaveLength(1);

    expect(updatedFilters[0]).toHaveTextContent(
      'Filter 1 (id: filter-1, column: filter_1)',
    );

    const updatedFilterChanges = within(updatedFilters[0]).getAllByRole(
      'listitem',
    );
    expect(updatedFilterChanges).toHaveLength(2);
    expect(updatedFilterChanges[0]).toHaveTextContent(
      'label changed to: Filter 1 updated',
    );
    expect(updatedFilterChanges[1]).toHaveTextContent(
      'column changed to: filter_1_updated',
    );

    expect(
      screen.getByRole('heading', { name: 'Deleted Filter 1 filter options' }),
    ).toBeInTheDocument();

    const deletedFilterOptions = within(
      screen.getByTestId('deleted-filterOptions-filter-1'),
    ).getAllByRole('listitem');

    expect(deletedFilterOptions).toHaveLength(1);
    expect(deletedFilterOptions[0]).toHaveTextContent(
      'Filter option 1 (id: filter-opt-1)',
    );

    expect(
      screen.getByRole('heading', { name: 'Deleted geographic level options' }),
    ).toBeInTheDocument();

    const deletedGeographicLevels = within(
      screen.getByTestId('deleted-geographicLevels'),
    ).getAllByRole('listitem');

    expect(deletedGeographicLevels).toHaveLength(1);
    expect(deletedGeographicLevels[0]).toHaveTextContent(
      'Local authority district (code: LAD)',
    );

    expect(
      screen.getByRole('heading', { name: 'Updated indicators' }),
    ).toBeInTheDocument();

    const updatedIndicators = within(
      screen.getByTestId('updated-indicators'),
    ).getAllByTestId('updated-item');

    expect(updatedIndicators).toHaveLength(1);
    expect(updatedIndicators[0]).toHaveTextContent(
      'Indicator 1 (id: indicator-1, column: indicator_1)',
    );

    const updatedIndicatorChanges = within(updatedIndicators[0]).getAllByRole(
      'listitem',
    );
    expect(updatedIndicatorChanges).toHaveLength(2);
    expect(updatedIndicatorChanges[0]).toHaveTextContent(
      'label changed to: Indicator 1 updated',
    );
    expect(updatedIndicatorChanges[1]).toHaveTextContent(
      'column changed to: indicator_1_updated',
    );

    expect(
      screen.getByRole('heading', { name: 'New location groups' }),
    ).toBeInTheDocument();

    const addedLocationGroups = within(
      screen.getByTestId('added-locationGroups'),
    ).getAllByRole('listitem');

    expect(addedLocationGroups).toHaveLength(1);
    expect(addedLocationGroups[0]).toHaveTextContent(
      'Institution (code: INST)',
    );

    expect(
      screen.getByRole('heading', { name: 'New Regional location options' }),
    ).toBeInTheDocument();

    const addedLocationOptions = within(
      screen.getByTestId('added-locationOptions-REG'),
    ).getAllByRole('listitem');

    expect(addedLocationOptions).toHaveLength(1);
    expect(addedLocationOptions[0]).toHaveTextContent(
      'Location 1 (id: location-1, code: location-1-code)',
    );

    expect(
      screen.getByRole('heading', { name: 'New time periods' }),
    ).toBeInTheDocument();

    const addedTimePeriods = within(
      screen.getByTestId('added-timePeriods'),
    ).getAllByRole('listitem');

    expect(addedTimePeriods).toHaveLength(1);
    expect(addedTimePeriods[0]).toHaveTextContent('2017/18 (code: AY)');
  });
});
