import render from '@common-test/render';
import { screen, waitFor } from '@testing-library/react';
import React from 'react';
import ThemesAndReleasesFilterGroup from '@frontend/modules/search-data/components/ThemesAndReleasesFilterGroup';
import { testPublicationTree } from '@frontend/modules/search-data/__tests__/__data__/testPublicationTree';

describe('ThemesAndReleasesFilterGroup', () => {
  test('renders with search input and themes', () => {
    render(
      <ThemesAndReleasesFilterGroup
        publicationTree={testPublicationTree}
        onChangeBatch={jest.fn()}
      />,
    );

    expect(
      screen.getByLabelText('Find themes and releases'),
    ).toBeInTheDocument();

    const themesGroup = screen.getByRole('group', { name: 'Themes' });
    expect(themesGroup).toBeInTheDocument();

    expect(screen.getByLabelText('Theme 1')).toBeInTheDocument();
    expect(screen.getByLabelText('Theme 2')).toBeInTheDocument();
    expect(screen.getByLabelText('Theme 3')).toBeInTheDocument();
  });

  test('renders all themes unchecked by default', () => {
    render(
      <ThemesAndReleasesFilterGroup
        publicationTree={testPublicationTree}
        onChangeBatch={jest.fn()}
      />,
    );

    expect(screen.getByLabelText('Theme 1')).not.toBeChecked();
    expect(screen.getByLabelText('Theme 2')).not.toBeChecked();
    expect(screen.getByLabelText('Theme 3')).not.toBeChecked();
  });

  test('renders themes checked when themeIds prop is provided', () => {
    render(
      <ThemesAndReleasesFilterGroup
        publicationTree={testPublicationTree}
        themeIds={['theme-id-1', 'theme-id-3']}
        onChangeBatch={jest.fn()}
      />,
    );

    expect(screen.getByLabelText('Theme 1')).toBeChecked();
    expect(screen.getByLabelText('Theme 2')).not.toBeChecked();
    expect(screen.getByLabelText('Theme 3')).toBeChecked();
  });

  test('renders show publications buttons for themes with publications', () => {
    render(
      <ThemesAndReleasesFilterGroup
        publicationTree={testPublicationTree}
        onChangeBatch={jest.fn()}
      />,
    );

    expect(
      screen.getByRole('button', {
        name: 'Show 2 statistical releases for Theme 1',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Show 2 statistical releases for Theme 2',
      }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('button', {
        name: /Show.*statistical releases for Theme 3/,
      }),
    ).not.toBeInTheDocument();
  });

  test('checking a theme calls onChangeBatch with theme ID and empty publication IDs', async () => {
    const testOnChangeBatch = jest.fn();

    const { user } = render(
      <ThemesAndReleasesFilterGroup
        publicationTree={testPublicationTree}
        onChangeBatch={testOnChangeBatch}
      />,
    );

    expect(testOnChangeBatch).not.toHaveBeenCalled();

    await user.click(screen.getByLabelText('Theme 1'));

    expect(testOnChangeBatch).toHaveBeenCalledWith(['theme-id-1'], []);
  });

  test('unchecking a theme calls onChangeBatch with theme ID removed', async () => {
    const testOnChangeBatch = jest.fn();

    const { user } = render(
      <ThemesAndReleasesFilterGroup
        publicationTree={testPublicationTree}
        themeIds={['theme-id-1']}
        onChangeBatch={testOnChangeBatch}
      />,
    );

    await user.click(screen.getByLabelText('Theme 1'));

    expect(testOnChangeBatch).toHaveBeenCalledWith([], []);
  });

  test('checking a theme removes any selected publications under that theme', async () => {
    const testOnChangeBatch = jest.fn();

    const { user } = render(
      <ThemesAndReleasesFilterGroup
        publicationTree={testPublicationTree}
        publicationIds={['publication-id-1', 'publication-id-2']}
        onChangeBatch={testOnChangeBatch}
      />,
    );

    await user.click(screen.getByLabelText('Theme 1'));

    expect(testOnChangeBatch).toHaveBeenCalledWith(['theme-id-1'], []);
  });

  test('checking a theme removes only publications under that theme', async () => {
    const testOnChangeBatch = jest.fn();

    const { user } = render(
      <ThemesAndReleasesFilterGroup
        publicationTree={testPublicationTree}
        publicationIds={['publication-id-1', 'publication-id-3']}
        onChangeBatch={testOnChangeBatch}
      />,
    );

    await user.click(screen.getByLabelText('Theme 1'));

    expect(testOnChangeBatch).toHaveBeenCalledWith(
      ['theme-id-1'],
      ['publication-id-3'],
    );
  });

  test('filtering by search term shows only matching themes', async () => {
    const { user } = render(
      <ThemesAndReleasesFilterGroup
        publicationTree={testPublicationTree}
        onChangeBatch={jest.fn()}
      />,
    );

    const searchInput = screen.getByLabelText('Find themes and releases');

    await user.type(searchInput, 'theme 3');

    await waitFor(() => {
      expect(screen.queryByLabelText('Theme 1')).not.toBeInTheDocument();
    });

    expect(screen.queryByLabelText('Theme 2')).not.toBeInTheDocument();
    expect(screen.getByLabelText('Theme 3')).toBeInTheDocument();
  });

  test('filtering by search term shows themes with matching publications', async () => {
    const { user } = render(
      <ThemesAndReleasesFilterGroup
        publicationTree={testPublicationTree}
        onChangeBatch={jest.fn()}
      />,
    );

    const searchInput = screen.getByLabelText('Find themes and releases');

    await user.type(searchInput, 'tle 1');

    expect(screen.getByLabelText('Theme 1')).toBeInTheDocument();
    await waitFor(() => {
      expect(screen.queryByLabelText('Theme 2')).not.toBeInTheDocument();
    });
    expect(screen.queryByLabelText('Theme 3')).not.toBeInTheDocument();

    await user.click(
      screen.getByRole('button', {
        name: 'Show 1 statistical release for Theme 1',
      }),
    );
    expect(screen.getByLabelText('Publication Title 1')).toBeInTheDocument();
    expect(
      screen.queryByLabelText('Publication Title 2'),
    ).not.toBeInTheDocument();
  });

  test('displays correct count message for search results', async () => {
    const { user } = render(
      <ThemesAndReleasesFilterGroup
        publicationTree={testPublicationTree}
        onChangeBatch={jest.fn()}
      />,
    );

    const searchInput = screen.getByLabelText('Find themes and releases');

    await user.type(searchInput, 'theme');

    await waitFor(() => {
      expect(
        screen.getByText('3 options found', { selector: 'span[aria-live]' }),
      ).toBeInTheDocument();
    });

    await user.type(searchInput, ' 3');

    await waitFor(() => {
      expect(
        screen.getByText('1 option found', { selector: 'span[aria-live]' }),
      ).toBeInTheDocument();
    });
  });

  test('displays count message including publications in search results', async () => {
    const { user } = render(
      <ThemesAndReleasesFilterGroup
        publicationTree={testPublicationTree}
        onChangeBatch={jest.fn()}
      />,
    );

    const searchInput = screen.getByLabelText('Find themes and releases');

    await user.type(searchInput, 'title');

    await waitFor(() => {
      expect(
        screen.getByText('6 options found', { selector: 'span[aria-live]' }),
      ).toBeInTheDocument();
    });
  });

  test('displays no options message when search has no results', async () => {
    const { user } = render(
      <ThemesAndReleasesFilterGroup
        publicationTree={testPublicationTree}
        onChangeBatch={jest.fn()}
      />,
    );

    const searchInput = screen.getByLabelText('Find themes and releases');

    await user.type(searchInput, 'xyz999');

    await waitFor(() => {
      expect(screen.getByText('No options available.')).toBeInTheDocument();
    });

    expect(screen.queryByLabelText('Theme 1')).not.toBeInTheDocument();
    expect(
      screen.getByText('0 options found', { selector: 'span[aria-live]' }),
    ).toBeInTheDocument();
  });

  test('pressing Enter in search input does not submit form', async () => {
    const testOnSubmit = jest.fn(e => e.preventDefault());

    const { user } = render(
      <form onSubmit={testOnSubmit}>
        <ThemesAndReleasesFilterGroup
          publicationTree={testPublicationTree}
          onChangeBatch={jest.fn()}
        />
      </form>,
    );

    const searchInput = screen.getByLabelText('Find themes and releases');

    await user.clear(searchInput);
    await user.type(searchInput, 'Theme{Enter}');

    expect(searchInput).toHaveValue('Theme');
    expect(testOnSubmit).not.toHaveBeenCalled();
  });

  test('checking a publication calls onChangeBatch correctly', async () => {
    const testOnChangeBatch = jest.fn();

    const { user } = render(
      <ThemesAndReleasesFilterGroup
        publicationTree={testPublicationTree}
        onChangeBatch={testOnChangeBatch}
      />,
    );

    await user.click(
      screen.getByRole('button', {
        name: 'Show 2 statistical releases for Theme 1',
      }),
    );

    expect(testOnChangeBatch).not.toHaveBeenCalled();

    await user.click(screen.getByLabelText('Publication Title 1'));

    expect(testOnChangeBatch).toHaveBeenCalledWith([], ['publication-id-1']);
  });

  test('checking a publication removes parent theme if selected', async () => {
    const testOnChangeBatch = jest.fn();

    const { user } = render(
      <ThemesAndReleasesFilterGroup
        publicationTree={testPublicationTree}
        themeIds={['theme-id-1']}
        onChangeBatch={testOnChangeBatch}
      />,
    );

    await user.click(
      screen.getByRole('button', {
        name: 'Show 2 statistical releases for Theme 1',
      }),
    );

    await user.click(screen.getByLabelText('Publication Title 1'));

    expect(testOnChangeBatch).toHaveBeenCalledWith([], ['publication-id-1']);
  });

  test('checking a publication does not remove other selected themes', async () => {
    const testOnChangeBatch = jest.fn();

    const { user } = render(
      <ThemesAndReleasesFilterGroup
        publicationTree={testPublicationTree}
        themeIds={['theme-id-1', 'theme-id-2']}
        onChangeBatch={testOnChangeBatch}
      />,
    );

    await user.click(
      screen.getByRole('button', {
        name: 'Show 2 statistical releases for Theme 1',
      }),
    );

    await user.click(screen.getByLabelText('Publication Title 1'));

    expect(testOnChangeBatch).toHaveBeenCalledWith(
      ['theme-id-2'],
      ['publication-id-1'],
    );
  });

  test('unchecking a publication calls onChangeBatch correctly', async () => {
    const testOnChangeBatch = jest.fn();

    const { user } = render(
      <ThemesAndReleasesFilterGroup
        publicationTree={testPublicationTree}
        publicationIds={['publication-id-1']}
        onChangeBatch={testOnChangeBatch}
      />,
    );

    await user.click(
      screen.getByRole('button', {
        name: 'Show 2 statistical releases for Theme 1',
      }),
    );

    await user.click(screen.getByLabelText('Publication Title 1'));

    expect(testOnChangeBatch).toHaveBeenCalledWith([], []);
  });

  test('publications are checked when publicationIds prop is provided', async () => {
    const { user } = render(
      <ThemesAndReleasesFilterGroup
        publicationTree={testPublicationTree}
        publicationIds={['publication-id-1', 'publication-id-3']}
        onChangeBatch={jest.fn()}
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
});
