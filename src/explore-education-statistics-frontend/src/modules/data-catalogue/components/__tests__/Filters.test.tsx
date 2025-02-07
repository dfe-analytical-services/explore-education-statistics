import Filters from '@frontend/modules/data-catalogue/components/Filters';
import { testReleases } from '@frontend/modules/data-catalogue/__data__/testReleases';
import { testThemes } from '@frontend/modules/data-catalogue/__data__/testThemes';
import { screen, within } from '@testing-library/react';
import render from '@common-test/render';
import React from 'react';
import noop from 'lodash/noop';

describe('Filters', () => {
  test('renders the default filters', () => {
    render(<Filters themes={testThemes} onChange={noop} />);

    const themesSelect = screen.getByLabelText('Filter by Theme');
    const themes = within(themesSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];
    expect(themes).toHaveLength(3);

    expect(themes[0]).toHaveTextContent('All');
    expect(themes[0]).toHaveValue('all');
    expect(themes[0].selected).toBe(true);

    expect(themes[1]).toHaveTextContent('Theme title 1');
    expect(themes[1]).toHaveValue('theme-1');
    expect(themes[1].selected).toBe(false);

    expect(themes[2]).toHaveTextContent('Theme title 2');
    expect(themes[2]).toHaveValue('theme-2');
    expect(themes[2].selected).toBe(false);

    const publicationsSelect = screen.getByLabelText('Filter by Publication');
    const publications = within(publicationsSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];
    expect(publications).toHaveLength(1);

    expect(publications[0]).toHaveTextContent('All');
    expect(publications[0]).toHaveValue('all');
    expect(publications[0].selected).toBe(true);

    const releasesSelect = screen.getByLabelText('Filter by Releases');
    const releases = within(releasesSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];
    expect(releases).toHaveLength(2);

    expect(releases[0]).toHaveTextContent('Latest releases');
    expect(releases[0]).toHaveValue('latest');
    expect(releases[0].selected).toBe(true);

    expect(releases[1]).toHaveTextContent('All releases');
    expect(releases[1]).toHaveValue('all');
    expect(releases[1].selected).toBe(false);
  });

  test('renders the data set type filter when showTypeFilter is true', async () => {
    render(<Filters showTypeFilter themes={testThemes} onChange={noop} />);

    const apiFilter = within(
      screen.getByRole('group', {
        name: 'Type of data',
      }),
    );
    expect(apiFilter.getByLabelText('All data')).toBeChecked();
    expect(apiFilter.getByLabelText('API data sets only')).not.toBeChecked();
  });

  test('does not render the data set type filter when showTypeFilter is false', async () => {
    render(<Filters themes={testThemes} onChange={noop} />);

    expect(
      screen.queryByRole('group', { name: 'Type of data' }),
    ).not.toBeInTheDocument();
  });

  test('populates the release filter with all & releases when there is a publicationId', () => {
    render(
      <Filters
        publicationId="publication-1"
        releases={testReleases}
        themes={testThemes}
        themeId="theme-2"
        onChange={noop}
      />,
    );

    const releasesSelect = screen.getByLabelText('Filter by Releases');
    const releases = within(releasesSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];
    expect(releases).toHaveLength(4);

    expect(releases[0]).toHaveTextContent('All releases');
    expect(releases[0]).toHaveValue('all');

    expect(releases[1]).toHaveTextContent('Release title 3');
    expect(releases[1]).toHaveValue('release-3');

    expect(releases[2]).toHaveTextContent('Release title 2');
    expect(releases[2]).toHaveValue('release-2');

    expect(releases[3]).toHaveTextContent('Release title 1');
    expect(releases[3]).toHaveValue('release-1');
  });

  test('disables the publication filter when there is no themeId', () => {
    render(<Filters themes={testThemes} onChange={noop} />);

    expect(screen.getByLabelText('Filter by Publication')).toBeDisabled();
  });

  test('enables the publication filter when there is a themeId', () => {
    render(<Filters themes={testThemes} themeId="theme-2" onChange={noop} />);

    expect(screen.getByLabelText('Filter by Publication')).not.toBeDisabled();
  });

  test('calls the onChange handler when the theme filter is changed', async () => {
    const handleChange = jest.fn();
    const { user } = render(
      <Filters themes={testThemes} onChange={handleChange} />,
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

  test('calls the onChange handler when the publication filter is changed', async () => {
    const handleChange = jest.fn();
    const { user } = render(
      <Filters
        publications={testThemes[1].publications}
        themeId="theme-2"
        themes={testThemes}
        onChange={handleChange}
      />,
    );

    expect(handleChange).not.toHaveBeenCalled();

    await user.selectOptions(screen.getByLabelText('Filter by Publication'), [
      'publication-2',
    ]);

    expect(handleChange).toHaveBeenCalledWith({
      filterType: 'publicationId',
      nextValue: 'publication-2',
    });
  });

  test('calls the onChange handler when the release filter is changed', async () => {
    const handleChange = jest.fn();
    const { user } = render(
      <Filters
        publicationId="publication-2"
        releases={testReleases}
        themes={testThemes}
        themeId="theme-2"
        onChange={handleChange}
      />,
    );

    expect(handleChange).not.toHaveBeenCalled();

    await user.selectOptions(screen.getByLabelText('Filter by Releases'), [
      'release-1',
    ]);

    expect(handleChange).toHaveBeenCalledWith({
      filterType: 'releaseVersionId',
      nextValue: 'release-1',
    });
  });

  test('calls the onChange handler when the type of data filter is changed', async () => {
    const handleChange = jest.fn();
    const { user } = render(
      <Filters
        publicationId="publication-2"
        releases={testReleases}
        showTypeFilter
        themes={testThemes}
        themeId="theme-2"
        onChange={handleChange}
      />,
    );

    expect(handleChange).not.toHaveBeenCalled();

    await user.click(screen.getByLabelText('API data sets only'));

    expect(handleChange).toHaveBeenCalledWith({
      filterType: 'dataSetType',
      nextValue: 'api',
    });
  });

  test('sets the initial value for the theme filter', () => {
    render(<Filters themes={testThemes} themeId="theme-2" onChange={noop} />);

    expect(screen.getByLabelText('Filter by Theme')).toHaveValue('theme-2');
  });

  test('sets the initial value for the publication filter', () => {
    render(
      <Filters
        publicationId="publication-2"
        publications={testThemes[1].publications}
        themes={testThemes}
        themeId="theme-2"
        onChange={noop}
      />,
    );

    expect(screen.getByLabelText('Filter by Publication')).toHaveValue(
      'publication-2',
    );
  });

  test('sets the initial value for the release filter', () => {
    render(
      <Filters
        publicationId="publication-1"
        releases={testReleases}
        releaseVersionId="release-2"
        themes={testThemes}
        themeId="theme-2"
        onChange={noop}
      />,
    );

    expect(screen.getByLabelText('Filter by Releases')).toHaveValue(
      'release-2',
    );
  });
});
