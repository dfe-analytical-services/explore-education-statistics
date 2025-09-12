import render from '@common-test/render';
import Filters from '@frontend/modules/find-statistics/components/Filters';
import { testThemeSummaries } from '@frontend/modules/find-statistics/__tests__/__data__/testThemeData';
import { screen, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';
import { ReleaseType, releaseTypes } from '@common/services/types/releaseType';
import { SortOption } from '@frontend/components/SortControls';

const releaseTypesForSelect = Object.keys(releaseTypes).map(type => {
  const title = releaseTypes[type as ReleaseType];
  return {
    label: title,
    value: type,
  };
});
const themesForSelect = testThemeSummaries.map(theme => ({
  label: theme.title,
  value: theme.id,
}));
const sortOptions = [
  { label: 'Newest', value: 'newest' },
  { label: 'Oldest', value: 'oldest' },
  { label: 'A to Z', value: 'title' },
] as SortOption[];

describe('Filters', () => {
  test('renders the filters', () => {
    render(
      <Filters
        themes={testThemeSummaries}
        onChange={noop}
        sortBy="newest"
        onSortChange={noop}
        sortOptions={[
          { label: 'Newest', value: 'newest' },
          { label: 'Oldest', value: 'oldest' },
          { label: 'A to Z', value: 'title' },
        ]}
        releaseTypesWithResultCounts={releaseTypesForSelect}
        themesWithResultCounts={themesForSelect}
      />,
    );

    const sortSelect = screen.getByLabelText('Sort by');
    const sortSelectOptions = within(sortSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];
    expect(sortSelectOptions).toHaveLength(3);
    expect(sortSelectOptions[0]).toHaveTextContent('Newest');
    expect(sortSelectOptions[0]).toHaveValue('newest');
    expect(sortSelectOptions[0].selected).toBe(true);
    expect(sortSelectOptions[1]).toHaveTextContent('Oldest');
    expect(sortSelectOptions[1]).toHaveValue('oldest');
    expect(sortSelectOptions[1].selected).toBe(false);
    expect(sortSelectOptions[2]).toHaveTextContent('A to Z');
    expect(sortSelectOptions[2]).toHaveValue('title');
    expect(sortSelectOptions[2].selected).toBe(false);

    const themesSelect = screen.getByLabelText('Filter by Theme');
    const themes = within(themesSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];
    expect(themes).toHaveLength(5);

    expect(themes[0]).toHaveTextContent('All');
    expect(themes[0]).toHaveValue('all');
    expect(themes[0].selected).toBe(true);

    expect(themes[1]).toHaveValue('theme-1');
    expect(themes[1].selected).toBe(false);

    expect(themes[2]).toHaveValue('theme-2');
    expect(themes[2].selected).toBe(false);

    expect(themes[3]).toHaveValue('theme-3');
    expect(themes[3].selected).toBe(false);

    expect(themes[4]).toHaveValue('theme-4');
    expect(themes[4].selected).toBe(false);

    const releaseTypesSelect = screen.getByLabelText('Filter by Release type');
    const releaseTypeOptions = within(releaseTypesSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];
    expect(releaseTypeOptions).toHaveLength(7);

    expect(releaseTypeOptions[0]).toHaveTextContent('All release types');
    expect(releaseTypeOptions[0]).toHaveValue('all');
    expect(releaseTypeOptions[0].selected).toBe(true);

    expect(releaseTypeOptions[1]).toHaveTextContent(
      'Accredited official statistics',
    );
    expect(releaseTypeOptions[1]).toHaveValue('AccreditedOfficialStatistics');
    expect(releaseTypeOptions[1].selected).toBe(false);

    expect(releaseTypeOptions[2]).toHaveTextContent('Official statistics');
    expect(releaseTypeOptions[2]).toHaveValue('OfficialStatistics');
    expect(releaseTypeOptions[2].selected).toBe(false);

    expect(releaseTypeOptions[3]).toHaveTextContent(
      'Official statistics in development',
    );
    expect(releaseTypeOptions[3]).toHaveValue(
      'OfficialStatisticsInDevelopment',
    );
    expect(releaseTypeOptions[3].selected).toBe(false);

    expect(releaseTypeOptions[4]).toHaveTextContent('Experimental statistics');
    expect(releaseTypeOptions[4]).toHaveValue('ExperimentalStatistics');
    expect(releaseTypeOptions[4].selected).toBe(false);

    expect(releaseTypeOptions[5]).toHaveTextContent('Ad hoc statistics');
    expect(releaseTypeOptions[5]).toHaveValue('AdHocStatistics');
    expect(releaseTypeOptions[5].selected).toBe(false);

    expect(releaseTypeOptions[6]).toHaveTextContent('Management information');
    expect(releaseTypeOptions[6]).toHaveValue('ManagementInformation');
    expect(releaseTypeOptions[6].selected).toBe(false);
  });

  test('calls the onChange handler when the theme filter is changed', async () => {
    const handleChange = jest.fn();
    const { user } = render(
      <Filters
        themes={testThemeSummaries}
        onChange={handleChange}
        sortBy="newest"
        onSortChange={noop}
        sortOptions={sortOptions}
        releaseTypesWithResultCounts={releaseTypesForSelect}
        themesWithResultCounts={themesForSelect}
      />,
    );

    expect(handleChange).not.toHaveBeenCalled();

    await user.selectOptions(screen.getByLabelText('Filter by Theme'), [
      'theme-1',
    ]);

    expect(handleChange).toHaveBeenCalledWith({
      filterType: 'themeId',
      nextValue: 'theme-1',
    });
  });

  test('calls the onChange handler when the release type filter is changed', async () => {
    const handleChange = jest.fn();
    const { user } = render(
      <Filters
        themes={testThemeSummaries}
        onChange={handleChange}
        sortBy="newest"
        onSortChange={noop}
        sortOptions={sortOptions}
        releaseTypesWithResultCounts={releaseTypesForSelect}
        themesWithResultCounts={themesForSelect}
      />,
    );

    expect(handleChange).not.toHaveBeenCalled();

    await user.selectOptions(screen.getByLabelText('Filter by Release type'), [
      'ManagementInformation',
    ]);

    expect(handleChange).toHaveBeenCalledWith({
      filterType: 'releaseType',
      nextValue: 'ManagementInformation',
    });
  });

  test('sets the initial value for the theme filter', () => {
    render(
      <Filters
        themes={testThemeSummaries}
        themeId="theme-2"
        onChange={noop}
        sortBy="newest"
        onSortChange={noop}
        sortOptions={sortOptions}
        releaseTypesWithResultCounts={releaseTypesForSelect}
        themesWithResultCounts={themesForSelect}
      />,
    );

    expect(screen.getByLabelText('Filter by Theme')).toHaveValue('theme-2');
  });

  test('sets the initial value for the release type filter', () => {
    render(
      <Filters
        themes={testThemeSummaries}
        releaseType="ManagementInformation"
        onChange={noop}
        sortBy="newest"
        onSortChange={noop}
        sortOptions={sortOptions}
        releaseTypesWithResultCounts={releaseTypesForSelect}
        themesWithResultCounts={themesForSelect}
      />,
    );

    expect(screen.getByLabelText('Filter by Release type')).toHaveValue(
      'ManagementInformation',
    );
  });
});
