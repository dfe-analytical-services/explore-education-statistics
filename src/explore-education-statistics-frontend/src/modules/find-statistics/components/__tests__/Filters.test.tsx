import render from '@common-test/render';
import Filters from '@frontend/modules/find-statistics/components/Filters';
import { testThemeSummaries } from '@frontend/modules/find-statistics/__tests__/__data__/testThemeData';
import { screen, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

describe('Filters', () => {
  test('renders the filters', () => {
    render(<Filters themes={testThemeSummaries} onChange={noop} />);

    const themesSelect = screen.getByLabelText('Filter by Theme');
    const themes = within(themesSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];
    expect(themes).toHaveLength(5);

    expect(themes[0]).toHaveTextContent('All');
    expect(themes[0]).toHaveValue('all');
    expect(themes[0].selected).toBe(true);

    expect(themes[1]).toHaveTextContent('Theme 1');
    expect(themes[1]).toHaveValue('theme-1');
    expect(themes[1].selected).toBe(false);

    expect(themes[2]).toHaveTextContent('Theme 2');
    expect(themes[2]).toHaveValue('theme-2');
    expect(themes[2].selected).toBe(false);

    expect(themes[3]).toHaveTextContent('Theme 3');
    expect(themes[3]).toHaveValue('theme-3');
    expect(themes[3].selected).toBe(false);

    expect(themes[4]).toHaveTextContent('Theme 4');
    expect(themes[4]).toHaveValue('theme-4');
    expect(themes[4].selected).toBe(false);

    const releaseTypesSelect = screen.getByLabelText('Filter by Release type');
    const releaseTypes = within(releaseTypesSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];
    expect(releaseTypes).toHaveLength(7);

    expect(releaseTypes[0]).toHaveTextContent('All release types');
    expect(releaseTypes[0]).toHaveValue('all');
    expect(releaseTypes[0].selected).toBe(true);

    expect(releaseTypes[1]).toHaveTextContent('Accredited official statistics');
    expect(releaseTypes[1]).toHaveValue('AccreditedOfficialStatistics');
    expect(releaseTypes[1].selected).toBe(false);

    expect(releaseTypes[2]).toHaveTextContent('Official statistics');
    expect(releaseTypes[2]).toHaveValue('OfficialStatistics');
    expect(releaseTypes[2].selected).toBe(false);

    expect(releaseTypes[3]).toHaveTextContent(
      'Official statistics in development',
    );
    expect(releaseTypes[3]).toHaveValue('OfficialStatisticsInDevelopment');
    expect(releaseTypes[3].selected).toBe(false);

    expect(releaseTypes[4]).toHaveTextContent('Experimental statistics');
    expect(releaseTypes[4]).toHaveValue('ExperimentalStatistics');
    expect(releaseTypes[4].selected).toBe(false);

    expect(releaseTypes[5]).toHaveTextContent('Ad hoc statistics');
    expect(releaseTypes[5]).toHaveValue('AdHocStatistics');
    expect(releaseTypes[5].selected).toBe(false);

    expect(releaseTypes[6]).toHaveTextContent('Management information');
    expect(releaseTypes[6]).toHaveValue('ManagementInformation');
    expect(releaseTypes[6].selected).toBe(false);
  });

  test('calls the onChange handler when the theme filter is changed', async () => {
    const handleChange = jest.fn();
    const { user } = render(
      <Filters themes={testThemeSummaries} onChange={handleChange} />,
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
      <Filters themes={testThemeSummaries} onChange={handleChange} />,
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
      <Filters themes={testThemeSummaries} themeId="theme-2" onChange={noop} />,
    );

    expect(screen.getByLabelText('Filter by Theme')).toHaveValue('theme-2');
  });

  test('sets the initial value for the release type filter', () => {
    render(
      <Filters
        themes={testThemeSummaries}
        releaseType="ManagementInformation"
        onChange={noop}
      />,
    );

    expect(screen.getByLabelText('Filter by Release type')).toHaveValue(
      'ManagementInformation',
    );
  });
});
