import ThemeFilters from '@frontend/modules/find-statistics/components/ThemeFilters';
import { testThemeSummaries } from '@frontend/modules/find-statistics/__tests__/__data__/testThemeData';
import { render, screen, within } from '@testing-library/react';
import noop from 'lodash/noop';
import userEvent from '@testing-library/user-event';
import React from 'react';

describe('ThemeFilters', () => {
  test('renders correctly', () => {
    render(<ThemeFilters themes={testThemeSummaries} onChange={noop} />);

    const themeFilterGroup = within(
      screen.getByRole('group', { name: 'Filter by theme' }),
    );
    const themeOptions = themeFilterGroup.getAllByRole('radio');
    expect(themeOptions).toHaveLength(5);
    expect(themeOptions[0]).toEqual(
      themeFilterGroup.getByLabelText('All themes'),
    );
    expect(themeOptions[0]).toBeChecked();
    expect(themeOptions[1]).toEqual(themeFilterGroup.getByLabelText('Theme 1'));
    expect(themeOptions[1]).not.toBeChecked();
    expect(themeOptions[2]).toEqual(themeFilterGroup.getByLabelText('Theme 2'));
    expect(themeOptions[2]).not.toBeChecked();
    expect(themeOptions[3]).toEqual(themeFilterGroup.getByLabelText('Theme 3'));
    expect(themeOptions[3]).not.toBeChecked();
    expect(themeOptions[4]).toEqual(themeFilterGroup.getByLabelText('Theme 4'));
    expect(themeOptions[4]).not.toBeChecked();

    expect(
      screen.getByRole('button', { name: 'What are themes?' }),
    ).toBeInTheDocument();
  });

  test('selects the correct option when themeId is set', () => {
    render(
      <ThemeFilters
        themes={testThemeSummaries}
        themeId="theme-3"
        onChange={noop}
      />,
    );

    expect(screen.getByLabelText('Theme 3')).toBeChecked();
  });

  test('calls onChange when a theme is selected', () => {
    const handleChange = jest.fn();
    render(
      <ThemeFilters themes={testThemeSummaries} onChange={handleChange} />,
    );

    expect(handleChange).not.toHaveBeenCalled();

    userEvent.click(screen.getByLabelText('Theme 1'));

    expect(handleChange).toHaveBeenCalledWith({
      filterType: 'themeId',
      nextValue: 'theme-1',
    });
  });

  test('shows the guidance modal', () => {
    render(<ThemeFilters themes={testThemeSummaries} onChange={noop} />);

    userEvent.click(screen.getByRole('button', { name: 'What are themes?' }));

    const modal = within(screen.getByRole('dialog'));
    expect(
      modal.getByRole('heading', { name: 'Themes guidance' }),
    ).toBeInTheDocument();
  });
});
