import ReleaseDataFilePage from '@admin/pages/release/data/ReleaseDataFilePage';
import {
  releaseDataFileRoute,
  ReleaseDataFileRouteParams,
} from '@admin/routes/releaseRoutes';
import _releaseDataFileService, {
  DataFile,
} from '@admin/services/releaseDataFileService';
import render from '@common-test/render';
import { screen, waitFor } from '@testing-library/react';
import { createMemoryHistory, MemoryHistory } from 'history';
import React from 'react';
import { generatePath, Route, Router } from 'react-router-dom';

jest.mock('@admin/services/releaseDataFileService');

const releaseDataFileService = _releaseDataFileService as jest.Mocked<
  typeof _releaseDataFileService
>;

describe('ReleaseDataFilePage', () => {
  const testFile: DataFile = {
    id: 'file-1',
    title: 'Test data file',
    userName: 'user1@test.com',
    fileName: 'data-1.csv',
    metaFileId: 'data-1-meta',
    metaFileName: 'data-1.meta.csv',
    rows: 100,
    fileSize: {
      size: 20,
      unit: 'KB',
    },
    created: '2020-06-12T12:00:00',
    status: 'COMPLETE',
    permissions: {
      canCancelImport: false,
    },
  };

  test('renders form with initial values', async () => {
    releaseDataFileService.getDataFile.mockResolvedValue(testFile);

    renderPage();
    await waitFor(() => {
      expect(screen.getByText('Edit data file details')).toBeInTheDocument();
    });

    expect(screen.getByLabelText('Title')).toHaveValue('Test data file');
  });

  test('does not render form if unable to get data file details', async () => {
    releaseDataFileService.getDataFile.mockRejectedValue(
      new Error('Could not find data file'),
    );

    renderPage();
    await waitFor(() => {
      expect(screen.getByText('Edit data file details')).toBeInTheDocument();
    });

    expect(
      screen.getByText('Could not load data file details'),
    ).toBeInTheDocument();
    expect(screen.queryByLabelText('Title')).not.toBeInTheDocument();
  });

  test('shows validation message if `title` field is empty', async () => {
    releaseDataFileService.getDataFile.mockResolvedValue(testFile);

    const { user } = renderPage();
    await waitFor(() => {
      expect(screen.getByText('Edit data file details')).toBeInTheDocument();
    });

    await user.clear(screen.getByLabelText('Title'));
    await user.click(screen.getByRole('button', { name: 'Save changes' }));

    await waitFor(() => {
      expect(
        screen.getByRole('link', { name: 'Enter a title' }),
      ).toHaveAttribute('href', '#dataFileForm-title');
    });
  });

  test('shows validation messages if submitted form is invalid', async () => {
    releaseDataFileService.getDataFile.mockResolvedValue({
      ...testFile,
      title: '',
    });

    const { user } = renderPage();
    await waitFor(() => {
      expect(screen.getByText('Edit data file details')).toBeInTheDocument();
    });

    await user.click(screen.getByRole('button', { name: 'Save changes' }));

    await waitFor(() => {
      expect(
        screen.getByRole('link', { name: 'Enter a title' }),
      ).toHaveAttribute('href', '#dataFileForm-title');
    });
  });

  test('successfully submitting form sends update request to service', async () => {
    releaseDataFileService.getDataFile.mockResolvedValue(testFile);

    const { user } = renderPage();
    await waitFor(() => {
      expect(screen.getByText('Edit data file details')).toBeInTheDocument();
    });

    const input = screen.getByLabelText('Title');

    await user.clear(input);
    await user.type(input, 'Updated test data file');

    await user.click(screen.getByRole('button', { name: 'Save changes' }));

    await waitFor(() => {
      expect(releaseDataFileService.updateFile).toHaveBeenCalledWith<
        Parameters<typeof releaseDataFileService.updateFile>
      >('release-1', 'file-1', {
        title: 'Updated test data file',
      });
    });
  });

  test('successfully submitting form redirects to data files page', async () => {
    releaseDataFileService.getDataFile.mockResolvedValue(testFile);

    const history = createMemoryHistory();

    const { user } = renderPage(history);
    await waitFor(() => {
      expect(screen.getByText('Edit data file details')).toBeInTheDocument();
    });

    await user.click(screen.getByRole('button', { name: 'Save changes' }));

    await waitFor(() => {
      expect(history.location.pathname).toBe(
        '/publication/publication-1/release/release-1/data',
      );
      expect(history.location.hash).toBe('');
    });
  });

  function renderPage(history: MemoryHistory = createMemoryHistory()) {
    history.push(
      generatePath<ReleaseDataFileRouteParams>(releaseDataFileRoute.path, {
        publicationId: 'publication-1',
        releaseVersionId: 'release-1',
        fileId: 'file-1',
      }),
    );

    return render(
      <Router history={history}>
        <Route
          path={releaseDataFileRoute.path}
          component={ReleaseDataFilePage}
        />
      </Router>,
    );
  }
});
