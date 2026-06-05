import render from '@common-test/render';
import { screen } from '@testing-library/react';
import React from 'react';
import ThemesAndReleasesFilterGroupReleases from '@frontend/modules/search-data/components/ThemesAndReleasesFilterGroupReleases';
import { testPublicationTree } from '@frontend/modules/search-data/__tests__/__data__/testPublicationTree';

describe('ThemesAndReleasesFilterGroupReleases', () => {
  const testPublications = testPublicationTree[0].publications;
  const testThemeId = 'theme-id-1';
  const testThemeTitle = 'Theme 1';

  test('renders collapsed by default with show button', () => {
    render(
      <ThemesAndReleasesFilterGroupReleases
        publicationIds={[]}
        publications={testPublications}
        themeId={testThemeId}
        themeTitle={testThemeTitle}
        onChange={jest.fn()}
      />,
    );

    expect(
      screen.getByRole('button', {
        name: 'Show 2 statistical releases for Theme 1',
      }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('group', { name: 'Statistical releases for Theme 1' }),
    ).not.toBeInTheDocument();
  });

  test('clicking show button expands and shows publications', async () => {
    const { user } = render(
      <ThemesAndReleasesFilterGroupReleases
        publicationIds={[]}
        publications={testPublications}
        themeId={testThemeId}
        themeTitle={testThemeTitle}
        onChange={jest.fn()}
      />,
    );

    await user.click(
      screen.getByRole('button', {
        name: 'Show 2 statistical releases for Theme 1',
      }),
    );

    expect(
      screen.getByRole('group', { name: 'Statistical releases for Theme 1' }),
    ).toBeInTheDocument();

    expect(screen.getByLabelText('Publication Title 1')).toBeInTheDocument();
    expect(screen.getByLabelText('Publication Title 2')).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Close statistical releases for Theme 1',
      }),
    ).toBeInTheDocument();

    await user.click(
      screen.getByRole('button', {
        name: 'Close statistical releases for Theme 1',
      }),
    );

    expect(
      screen.queryByRole('group', { name: 'Statistical releases for Theme 1' }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Show 2 statistical releases for Theme 1',
      }),
    ).toBeInTheDocument();
  });

  test('renders all publications as unchecked by default', async () => {
    const { user } = render(
      <ThemesAndReleasesFilterGroupReleases
        publicationIds={[]}
        publications={testPublications}
        themeId={testThemeId}
        themeTitle={testThemeTitle}
        onChange={jest.fn()}
      />,
    );

    await user.click(
      screen.getByRole('button', {
        name: 'Show 2 statistical releases for Theme 1',
      }),
    );

    expect(screen.getByLabelText('Publication Title 1')).not.toBeChecked();
    expect(screen.getByLabelText('Publication Title 2')).not.toBeChecked();
  });

  test('renders publications as checked when in publicationIds', async () => {
    const { user } = render(
      <ThemesAndReleasesFilterGroupReleases
        publicationIds={['publication-id-1']}
        publications={testPublications}
        themeId={testThemeId}
        themeTitle={testThemeTitle}
        onChange={jest.fn()}
      />,
    );

    await user.click(
      screen.getByRole('button', {
        name: 'Show 2 statistical releases for Theme 1',
      }),
    );

    expect(screen.getByLabelText('Publication Title 1')).toBeChecked();
    expect(screen.getByLabelText('Publication Title 2')).not.toBeChecked();
  });

  test('checking a publication calls onChange with correct arguments', async () => {
    const testOnChange = jest.fn();

    const { user } = render(
      <ThemesAndReleasesFilterGroupReleases
        publicationIds={[]}
        publications={testPublications}
        themeId={testThemeId}
        themeTitle={testThemeTitle}
        onChange={testOnChange}
      />,
    );

    await user.click(
      screen.getByRole('button', {
        name: 'Show 2 statistical releases for Theme 1',
      }),
    );

    expect(testOnChange).not.toHaveBeenCalled();

    await user.click(screen.getByLabelText('Publication Title 1'));

    expect(testOnChange).toHaveBeenCalledWith(
      'theme-id-1',
      'publication-id-1',
      true,
    );
  });

  test('unchecking a publication calls onChange with correct arguments', async () => {
    const testOnChange = jest.fn();

    const { user } = render(
      <ThemesAndReleasesFilterGroupReleases
        publicationIds={['publication-id-1']}
        publications={testPublications}
        themeId={testThemeId}
        themeTitle={testThemeTitle}
        onChange={testOnChange}
      />,
    );

    await user.click(
      screen.getByRole('button', {
        name: 'Show 2 statistical releases for Theme 1',
      }),
    );

    await user.click(screen.getByLabelText('Publication Title 1'));

    expect(testOnChange).toHaveBeenCalledWith(
      'theme-id-1',
      'publication-id-1',
      false,
    );
  });
});
