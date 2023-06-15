import ReleaseFootnotesPage from '@admin/pages/release/footnotes/ReleaseFootnotesPage';
import {
  releaseFootnotesRoute,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import _footnoteService, {
  Footnote,
  FootnoteMeta,
} from '@admin/services/footnoteService';
import _permissionService from '@admin/services/permissionService';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { createMemoryHistory } from 'history';
import { generatePath, Route, Router } from 'react-router-dom';
import React from 'react';

jest.mock('@admin/services/footnoteService');
const footnoteService = _footnoteService as jest.Mocked<
  typeof _footnoteService
>;

jest.mock('@admin/services/permissionService');
const permissionService = _permissionService as jest.Mocked<
  typeof _permissionService
>;

describe('ReleaseFootnotesPage', () => {
  const testFootnoteMeta: FootnoteMeta = {
    subjects: {
      'test-subject-1': {
        subjectId: 'test-subject-1',
        subjectName: 'Test subject 1',
        indicators: {},
        filters: {},
      },
    },
  };

  const testFootnotes: Footnote[] = [
    { id: 'footnote-1', content: 'Footnote 1 content', subjects: {} },
    { id: 'footnote-2', content: 'Footnote 2 content', subjects: {} },
    { id: 'footnote-3', content: 'Footnote 3 content', subjects: {} },
  ];

  describe('with footnotes', () => {
    test('renders the page with footnotes', async () => {
      permissionService.canUpdateRelease.mockResolvedValue(true);
      footnoteService.getFootnoteMeta.mockResolvedValue(testFootnoteMeta);
      footnoteService.getFootnotes.mockResolvedValue(testFootnotes);
      renderPage();

      await waitFor(() => {
        expect(screen.getByText('Footnotes')).toBeInTheDocument();
      });

      expect(
        screen.getByRole('link', { name: 'Create footnote' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Reorder footnotes' }),
      ).toBeInTheDocument();

      const footnote1 = within(
        screen.getByTestId('Footnote - Footnote 1 content'),
      );
      expect(footnote1.getByText('Footnote 1 content')).toBeInTheDocument();
      expect(
        footnote1.getByRole('link', { name: 'Edit footnote' }),
      ).toBeInTheDocument();
      expect(
        footnote1.getByRole('button', { name: 'Delete footnote' }),
      ).toBeInTheDocument();
      expect(
        footnote1.getByRole('button', {
          name: 'See matching criteria',
        }),
      ).toBeInTheDocument();

      const footnote2 = within(
        screen.getByTestId('Footnote - Footnote 2 content'),
      );
      expect(footnote2.getByText('Footnote 2 content')).toBeInTheDocument();
      expect(
        footnote2.getByRole('link', { name: 'Edit footnote' }),
      ).toBeInTheDocument();
      expect(
        footnote2.getByRole('button', { name: 'Delete footnote' }),
      ).toBeInTheDocument();
      expect(
        footnote2.getByRole('button', {
          name: 'See matching criteria',
        }),
      ).toBeInTheDocument();

      const footnote3 = within(
        screen.getByTestId('Footnote - Footnote 3 content'),
      );
      expect(footnote3.getByText('Footnote 3 content')).toBeInTheDocument();
      expect(
        footnote3.getByRole('link', { name: 'Edit footnote' }),
      ).toBeInTheDocument();
      expect(
        footnote3.getByRole('button', { name: 'Delete footnote' }),
      ).toBeInTheDocument();
      expect(
        footnote3.getByRole('button', {
          name: 'See matching criteria',
        }),
      ).toBeInTheDocument();
    });

    test('shows the reordering UI when the reorder button is clicked', async () => {
      permissionService.canUpdateRelease.mockResolvedValue(true);
      footnoteService.getFootnoteMeta.mockResolvedValue(testFootnoteMeta);
      footnoteService.getFootnotes.mockResolvedValue(testFootnotes);
      renderPage();

      await waitFor(() => {
        expect(screen.getByText('Footnotes')).toBeInTheDocument();
      });

      expect(
        screen.queryByRole('button', { name: 'Save order' }),
      ).not.toBeInTheDocument();
      expect(
        screen.getByRole('link', { name: 'Create footnote' }),
      ).toBeInTheDocument();

      userEvent.click(
        screen.getByRole('button', { name: 'Reorder footnotes' }),
      );

      expect(
        screen.getByRole('button', { name: 'Save order' }),
      ).toBeInTheDocument();
      expect(
        screen.queryByRole('button', { name: 'Reorder footnotes' }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByRole('link', { name: 'Create footnote' }),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByRole('link', { name: 'Edit footnote' }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByRole('button', { name: 'Delete footnote' }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByRole('button', { name: 'See matching criteria' }),
      ).not.toBeInTheDocument();

      expect(screen.getByTestId('Footnote - Footnote 1 content')).toBe(
        screen.getByRole('button', { name: 'Footnote 1 content' }),
      );
      expect(screen.getByTestId('Footnote - Footnote 3 content')).toBe(
        screen.getByRole('button', { name: 'Footnote 3 content' }),
      );
      expect(screen.getByTestId('Footnote - Footnote 3 content')).toBe(
        screen.getByRole('button', { name: 'Footnote 3 content' }),
      );
    });

    test('calls the footnotes service when save order is clicked', async () => {
      permissionService.canUpdateRelease.mockResolvedValue(true);
      footnoteService.getFootnoteMeta.mockResolvedValue(testFootnoteMeta);
      footnoteService.getFootnotes.mockResolvedValue(testFootnotes);
      renderPage();

      await waitFor(() => {
        expect(screen.getByText('Footnotes')).toBeInTheDocument();
      });

      userEvent.click(
        screen.getByRole('button', { name: 'Reorder footnotes' }),
      );
      await waitFor(() => {
        expect(screen.getByText('Save order')).toBeInTheDocument();
      });
      userEvent.click(screen.getByRole('button', { name: 'Save order' }));

      expect(footnoteService.updateFootnotesOrder).toHaveBeenCalledWith(
        'release-1',
        ['footnote-1', 'footnote-2', 'footnote-3'],
      );
    });
  });

  test('renders correctly without footnotes', async () => {
    permissionService.canUpdateRelease.mockResolvedValue(true);
    footnoteService.getFootnoteMeta.mockResolvedValue(testFootnoteMeta);
    footnoteService.getFootnotes.mockResolvedValue([]);
    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Footnotes')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', { name: 'Create footnote' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Reorder footnotes' }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByText('No footnotes have been created.'),
    ).toBeInTheDocument();
  });

  test('renders correctly with no footnoteMeta', async () => {
    permissionService.canUpdateRelease.mockResolvedValue(true);
    footnoteService.getFootnoteMeta.mockResolvedValue({ subjects: {} });
    footnoteService.getFootnotes.mockResolvedValue([]);
    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Footnotes')).toBeInTheDocument();
    });

    expect(
      screen.queryByRole('link', { name: 'Create footnote' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Reorder footnotes' }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByText(
        /Before footnotes can be created, relevant data files need to be uploaded./,
      ),
    ).toBeInTheDocument();
  });

  test('renders correctly with no update permissions', async () => {
    permissionService.canUpdateRelease.mockResolvedValue(false);
    footnoteService.getFootnoteMeta.mockResolvedValue(testFootnoteMeta);
    footnoteService.getFootnotes.mockResolvedValue(testFootnotes);
    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Footnotes')).toBeInTheDocument();
    });

    expect(
      screen.queryByRole('link', { name: 'Create footnote' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Reorder footnotes' }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByText(
        'This release has been approved, and can no longer be updated.',
      ),
    ).toBeInTheDocument();

    expect(
      screen.getByTestId('Footnote - Footnote 1 content'),
    ).toBeInTheDocument();
    expect(
      screen.getByTestId('Footnote - Footnote 2 content'),
    ).toBeInTheDocument();
    expect(
      screen.getByTestId('Footnote - Footnote 3 content'),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('link', { name: 'Edit footnote' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Delete footnote' }),
    ).not.toBeInTheDocument();
  });
});

function renderPage() {
  const history = createMemoryHistory();
  history.push(
    generatePath<ReleaseRouteParams>(releaseFootnotesRoute.path, {
      publicationId: 'publication-1',
      releaseId: 'release-1',
    }),
  );

  render(
    <Router history={history}>
      <Route
        path={releaseFootnotesRoute.path}
        component={ReleaseFootnotesPage}
      />
    </Router>,
  );
}
