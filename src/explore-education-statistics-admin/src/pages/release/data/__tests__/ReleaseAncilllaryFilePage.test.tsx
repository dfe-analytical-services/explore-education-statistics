import ReleaseAncillaryFilePage from '@admin/pages/release/data/ReleaseAncillaryFilePage';
import {
  releaseAncillaryFileRoute,
  releaseDataRoute,
} from '@admin/routes/releaseRoutes';
import _releaseAncillaryFileService, {
  AncillaryFile,
} from '@admin/services/releaseAncillaryFileService';
import render from '@common-test/render';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { generatePath } from 'react-router-dom';
import TestRouterRenderer from '@admin/components/testing/TestRouterRenderer';
import {
  expectLocation,
  expectLocationHash,
} from '@admin/components/testing/TestLocationContext';

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

    await userEvent.clear(title);
    await userEvent.type(title, 'Updated test title');

    const summary = screen.getByLabelText('Summary');

    await userEvent.clear(summary);
    await userEvent.type(summary, 'Updated test summary');

    const file = new File(['test'], 'test.txt');
    await userEvent.upload(
      screen.getByTestId('file-input-ancillaryFileForm-file'),
      file,
    );

    await userEvent.click(screen.getByRole('button', { name: 'Save file' }));

    await waitFor(() => {
      expect(releaseAncillaryFileService.updateFile).toHaveBeenCalledWith<
        Parameters<typeof releaseAncillaryFileService.updateFile>
      >('release-1', 'file-1', {
        title: 'Updated test title',
        summary: 'Updated test summary',
        file,
      });
    });
  });

  test('successfully submitting form redirects to ancillary files page', async () => {
    releaseAncillaryFileService.getFile.mockResolvedValue(testFile);
    releaseAncillaryFileService.listFiles.mockResolvedValue([testFile]);

    await renderPage();

    await userEvent.click(screen.getByRole('button', { name: 'Save file' }));

    await waitFor(async () => {
      await expectLocation('/publication/publication-1/release/release-1/data');
      await expectLocationHash('#file-uploads');
    });
  });

  async function renderPage() {
    const path = generatePath(releaseAncillaryFileRoute.fullPath, {
      publicationId: 'publication-1',
      releaseVersionId: 'release-1',
      fileId: 'file-1',
    });

    render(
      <TestRouterRenderer
        initialUrl={path}
        route={releaseAncillaryFileRoute.fullPath}
        routes={[releaseDataRoute.fullPath]}
      >
        <ReleaseAncillaryFilePage />
      </TestRouterRenderer>,
    );

    await waitFor(() => {
      expect(screen.getByText('Edit ancillary file')).toBeInTheDocument();
    });
  }
});
