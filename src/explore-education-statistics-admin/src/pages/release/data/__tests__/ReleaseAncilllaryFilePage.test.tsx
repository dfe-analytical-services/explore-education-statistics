import ReleaseAncillaryFilePage from '@admin/pages/release/data/ReleaseAncillaryFilePage';
import {
  releaseAncillaryFileRoute,
  ReleaseAncillaryFileRouteParams,
} from '@admin/routes/releaseRoutes';
import _releaseAncillaryFileService, {
  AncillaryFile,
} from '@admin/services/releaseAncillaryFileService';
import render from '@common-test/render';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { createMemoryHistory, MemoryHistory } from 'history';
import React from 'react';
import { generatePath, Route, Router } from 'react-router-dom';

jest.mock('@admin/services/releaseAncillaryFileService');

const releaseAncillaryFileService = _releaseAncillaryFileService as jest.Mocked<
  typeof _releaseAncillaryFileService
>;

describe('ReleaseAncillaryFilePage', () => {
  const testFile: AncillaryFile = {
    id: 'file-1',
    title: 'Test title 1',
    summary: 'Test summary 1',
    filename: 'test-file-1.txt',
    fileSize: {
      size: 20,
      unit: 'kB',
    },
    userName: '',
    created: '',
  };

  test('renders form with initial values', async () => {
    releaseAncillaryFileService.getFile.mockResolvedValue(testFile);
    releaseAncillaryFileService.listFiles.mockResolvedValue([testFile]);

    await renderPage();

    expect(screen.getByLabelText('Title')).toHaveValue('Test title 1');
  });

  test('does not render form if unable to get ancillary file details', async () => {
    releaseAncillaryFileService.getFile.mockRejectedValue(
      new Error('Could not find ancillary file'),
    );
    releaseAncillaryFileService.listFiles.mockResolvedValue([]);

    await renderPage();

    expect(
      screen.getByText('Could not load ancillary file details'),
    ).toBeInTheDocument();
    expect(screen.queryByLabelText('Title')).not.toBeInTheDocument();
  });

  test('successfully submitting form sends service requests', async () => {
    releaseAncillaryFileService.getFile.mockResolvedValue(testFile);
    releaseAncillaryFileService.listFiles.mockResolvedValue([testFile]);

    await renderPage();

    const title = screen.getByLabelText('Title');

    userEvent.clear(title);
    await userEvent.type(title, 'Updated test title');

    const summary = screen.getByLabelText('Summary');

    userEvent.clear(summary);
    await userEvent.type(summary, 'Updated test summary');

    const file = new File(['test'], 'test.txt');

    userEvent.upload(screen.getByLabelText('Upload new file'), file);

    userEvent.click(screen.getByRole('button', { name: 'Save file' }));

    await waitFor(() => {
      expect(releaseAncillaryFileService.updateFile).toHaveBeenCalledWith<
        Parameters<typeof releaseAncillaryFileService.updateFile>
      >('release-1', 'file-1', {
        title: 'Updated test title',
        summary: 'Updated test summary',
      });
      expect(releaseAncillaryFileService.replaceFile).toHaveBeenCalledWith<
        Parameters<typeof releaseAncillaryFileService.replaceFile>
      >('release-1', 'file-1', file);
    });
  });

  test('successfully submitting form redirects to ancillary files page', async () => {
    releaseAncillaryFileService.getFile.mockResolvedValue(testFile);
    releaseAncillaryFileService.listFiles.mockResolvedValue([testFile]);

    const history = createMemoryHistory();

    await renderPage(history);

    userEvent.click(screen.getByRole('button', { name: 'Save file' }));

    await waitFor(() => {
      expect(history.location.pathname).toBe(
        '/publication/publication-1/release/release-1/data',
      );
      expect(history.location.hash).toBe('#file-uploads');
    });
  });

  async function renderPage(history: MemoryHistory = createMemoryHistory()) {
    history.push(
      generatePath<ReleaseAncillaryFileRouteParams>(
        releaseAncillaryFileRoute.path,
        {
          publicationId: 'publication-1',
          releaseId: 'release-1',
          fileId: 'file-1',
        },
      ),
    );

    render(
      <Router history={history}>
        <Route
          path={releaseAncillaryFileRoute.path}
          component={ReleaseAncillaryFilePage}
        />
      </Router>,
    );

    await waitFor(() => {
      expect(screen.getByText('Edit ancillary file')).toBeInTheDocument();
    });
  }
});
