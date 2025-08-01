import DataSetFileApiChangelog from '@frontend/modules/data-catalogue/components/DataSetFileApiChangelog';
import { render, screen, within } from '@testing-library/react';

describe('DataSetFileApiChangelog', () => {
  test('renders correctly with major and minor changes and public guidance notes', () => {
    render(
      <DataSetFileApiChangelog
        patchHistory={[]}
        changes={{
          versionNumber: '1.0.0',
          notes: '',
          patchHistory: [],
          majorChanges: {
            filters: [
              {
                previousState: {
                  id: 'filter-1',
                  column: 'filter_1',
                  label: 'Filter 1',
                  hint: '',
                },
              },
            ],
          },
          minorChanges: {
            filters: [
              {
                currentState: {
                  id: 'filter-2',
                  column: 'filter_2',
                  label: 'Filter 2',
                  hint: '',
                },
              },
            ],
          },
        }}
        guidanceNotes={'Guidance notes.\nMultiline content.'}
        version="2.0"
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'API data set changelog' }),
    ).toBeInTheDocument();

    expect(screen.getByTestId('public-guidance-notes')).toHaveTextContent(
      'Guidance notes. Multiline content.',
    );

    expect(
      screen.getByRole('heading', { name: 'Major changes for version 2.0' }),
    ).toBeInTheDocument();

    const majorChanges = within(screen.getByTestId('major-changes'));

    expect(majorChanges.getByTestId('deleted-filters')).toHaveTextContent(
      'Filter 1 (id: filter-1, column: filter_1)',
    );

    const minorChanges = within(screen.getByTestId('minor-changes'));

    expect(minorChanges.getByTestId('added-filters')).toHaveTextContent(
      'Filter 2 (id: filter-2, column: filter_2)',
    );
  });

  test('renders correctly with only major changes', () => {
    render(
      <DataSetFileApiChangelog
        patchHistory={[]}
        changes={{
          versionNumber: '1.0.0',
          notes: '',
          patchHistory: [],
          majorChanges: {
            filters: [
              {
                previousState: {
                  id: 'filter-1',
                  column: 'filter_1',
                  label: 'Filter 1',
                  hint: '',
                },
              },
            ],
          },
          minorChanges: {},
        }}
        guidanceNotes=""
        version="2.0"
      />,
    );

    const majorChanges = within(screen.getByTestId('major-changes'));

    expect(majorChanges.getByTestId('deleted-filters')).toHaveTextContent(
      'Filter 1 (id: filter-1, column: filter_1)',
    );

    expect(screen.queryByTestId('minor-changes')).not.toBeInTheDocument();
  });

  test('renders correctly with only minor changes', () => {
    render(
      <DataSetFileApiChangelog
        patchHistory={[]}
        changes={{
          versionNumber: '1.0.0',
          notes: '',
          patchHistory: [],
          majorChanges: {},
          minorChanges: {
            filters: [
              {
                currentState: {
                  id: 'filter-2',
                  column: 'filter_2',
                  label: 'Filter 2',
                  hint: '',
                },
              },
            ],
          },
        }}
        guidanceNotes=""
        version="2.0"
      />,
    );

    expect(screen.queryByTestId('major-changes')).not.toBeInTheDocument();

    const minorChanges = within(screen.getByTestId('minor-changes'));

    expect(minorChanges.getByTestId('added-filters')).toHaveTextContent(
      'Filter 2 (id: filter-2, column: filter_2)',
    );
  });

  test('renders correctly with no public data guidance', () => {
    render(
      <DataSetFileApiChangelog
        patchHistory={[]}
        changes={{
          versionNumber: '1.0.0',
          notes: '',
          patchHistory: [],
          majorChanges: {},
          minorChanges: {
            filters: [
              {
                currentState: {
                  id: 'filter-2',
                  column: 'filter_2',
                  label: 'Filter 2',
                  hint: '',
                },
              },
            ],
          },
        }}
        guidanceNotes=""
        version="2.0"
      />,
    );

    expect(screen.queryByTestId('data-guidance-notes')).not.toBeInTheDocument();
  });

  test('does not render if empty changes', () => {
    render(
      <DataSetFileApiChangelog
        patchHistory={[]}
        changes={{
          versionNumber: '1.0.0',
          notes: '',
          patchHistory: [],
          majorChanges: {},
          minorChanges: {},
        }}
        guidanceNotes=""
        version="2.0"
      />,
    );

    expect(
      screen.queryByRole('heading', { name: 'API data set changelog' }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByTestId('public-guidance-notes'),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('heading', { name: 'Major changes for version 2.0' }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('heading', { name: 'Minor changes for version 2.0' }),
    ).not.toBeInTheDocument();
  });
});
