import ReleaseDataFileReplacePage from '@admin/pages/release/data/ReleaseDataFileReplacePage';
import {
  releaseDataFileReplaceRoute,
  ReleaseDataFileReplaceRouteParams,
} from '@admin/routes/releaseRoutes';
import _dataReplacementService, {
  DataReplacementPlan,
} from '@admin/services/dataReplacementService';
import _permissionService from '@admin/services/permissionService';
import _releaseDataFileService, {
  DataFile,
} from '@admin/services/releaseDataFileService';
import { render, screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { generatePath, MemoryRouter, Route } from 'react-router';
import userEvent from '@testing-library/user-event';

jest.mock('@admin/services/dataReplacementService');
jest.mock('@admin/services/releaseDataFileService');
jest.mock('@admin/services/permissionService');

const dataReplacementService = _dataReplacementService as jest.Mocked<
  typeof _dataReplacementService
>;
const releaseDataFileService = _releaseDataFileService as jest.Mocked<
  typeof _releaseDataFileService
>;
const permissionService = _permissionService as jest.Mocked<
  typeof _permissionService
>;

describe('ReleaseDataFileReplacePage', () => {
  beforeEach(() => {
    permissionService.getDataFilePermissions.mockResolvedValue({
      canCancelImport: false,
    });
  });

  const testOriginalFile: DataFile = {
    id: 'data-1',
    title: 'Test data',
    status: 'COMPLETE',
    fileName: 'data.csv',
    fileSize: {
      size: 200,
      unit: 'B',
    },
    rows: 100,
    metaFileName: 'data.meta.csv',
    metaFileId: 'meta-1',
    userName: 'original@test.com',
    created: '2020-09-20T12:00:00',
    permissions: {
      canCancelImport: false,
    },
  };

  const testReplacementFile: DataFile = {
    id: 'data-2',
    title: 'Test data',
    status: 'COMPLETE',
    fileName: 'data-replacement.csv',
    fileSize: {
      size: 210,
      unit: 'B',
    },
    rows: 110,
    metaFileName: 'data-replacement.meta.csv',
    metaFileId: 'meta-2',
    userName: 'replacer@test.com',
    created: '2020-09-28T12:00:00',
    permissions: {
      canCancelImport: false,
    },
  };

  const testValidReplacementPlan: DataReplacementPlan = {
    valid: true,
    dataBlocks: [],
    footnotes: [],
    originalSubjectId: 'subject-1',
    replacementSubjectId: 'subject-1',
  };

  test('renders original data file details', async () => {
    releaseDataFileService.getDataFile.mockResolvedValue(testOriginalFile);
    releaseDataFileService.getDataFileImportStatus.mockResolvedValue({
      status: 'COMPLETE',
      percentageComplete: 100,
      stagePercentageComplete: 100,
      totalRows: 100,
    });

    render(
      <MemoryRouter
        initialEntries={[
          generatePath<ReleaseDataFileReplaceRouteParams>(
            releaseDataFileReplaceRoute.path,
            {
              releaseId: 'release-1',
              publicationId: 'publication-1',
              fileId: 'file-1',
            },
          ),
        ]}
      >
        <Route
          path={releaseDataFileReplaceRoute.path}
          component={ReleaseDataFileReplacePage}
        />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByTestId('Subject title')).toHaveTextContent(
        'Test data',
      );
      expect(screen.getByTestId('Data file')).toHaveTextContent('data.csv');
      expect(screen.getByTestId('Metadata file')).toHaveTextContent(
        'data.meta.csv',
      );
      expect(screen.getByTestId('Data file size')).toHaveTextContent('200 B');
      expect(screen.getByTestId('Number of rows')).toHaveTextContent('100');
      expect(screen.getByTestId('Status')).toHaveTextContent('Complete');
      expect(screen.getByTestId('Uploaded by')).toHaveTextContent(
        'original@test.com',
      );
      expect(screen.getByTestId('Date uploaded')).toHaveTextContent(
        '20 September 2020 12:00',
      );
    });
  });

  test('renders replacement upload form', async () => {
    releaseDataFileService.getDataFile.mockResolvedValueOnce(testOriginalFile);
    releaseDataFileService.getDataFileImportStatus.mockResolvedValue({
      status: 'COMPLETE',
      percentageComplete: 100,
      stagePercentageComplete: 100,
      totalRows: 100,
    });
    dataReplacementService.getReplacementPlan.mockResolvedValue(
      testValidReplacementPlan,
    );

    render(
      <MemoryRouter
        initialEntries={[
          generatePath<ReleaseDataFileReplaceRouteParams>(
            releaseDataFileReplaceRoute.path,
            {
              releaseId: 'release-1',
              publicationId: 'publication-1',
              fileId: 'file-1',
            },
          ),
        ]}
      >
        <Route
          path={releaseDataFileReplaceRoute.path}
          component={ReleaseDataFileReplacePage}
        />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByText('Upload replacement data')).toBeInTheDocument();

      expect(screen.getByLabelText('CSV files')).toBeInTheDocument();
      expect(screen.getByLabelText('Upload data file')).toBeInTheDocument();
      expect(screen.getByLabelText('Upload metadata file')).toBeInTheDocument();
      expect(screen.getByLabelText('ZIP file')).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Upload data files' }),
      ).toBeInTheDocument();
    });
  });

  test('renders replacement and original data file details', async () => {
    releaseDataFileService.getDataFile.mockResolvedValueOnce({
      ...testOriginalFile,
      replacedBy: testReplacementFile.id,
    });
    releaseDataFileService.getDataFile.mockResolvedValueOnce(
      testReplacementFile,
    );
    releaseDataFileService.getDataFileImportStatus.mockResolvedValue({
      status: 'COMPLETE',
      percentageComplete: 100,
      stagePercentageComplete: 100,
      totalRows: 100,
    });
    dataReplacementService.getReplacementPlan.mockResolvedValue(
      testValidReplacementPlan,
    );

    render(
      <MemoryRouter
        initialEntries={[
          generatePath<ReleaseDataFileReplaceRouteParams>(
            releaseDataFileReplaceRoute.path,
            {
              releaseId: 'release-1',
              publicationId: 'publication-1',
              fileId: 'file-1',
            },
          ),
        ]}
      >
        <Route
          path={releaseDataFileReplaceRoute.path}
          component={ReleaseDataFileReplacePage}
        />
      </MemoryRouter>,
    );

    await waitFor(() => {
      // Replacement
      expect(screen.getByTestId('Replacement Subject title')).toHaveTextContent(
        'Test data',
      );
      expect(screen.getByTestId('Replacement Data file')).toHaveTextContent(
        'data-replacement.csv',
      );
      expect(screen.getByTestId('Replacement Metadata file')).toHaveTextContent(
        'data-replacement.meta.csv',
      );
      expect(
        screen.getByTestId('Replacement Data file size'),
      ).toHaveTextContent('210 B');
      expect(
        screen.getByTestId('Replacement Number of rows'),
      ).toHaveTextContent('110');
      expect(screen.getByTestId('Replacement Status')).toHaveTextContent(
        'Complete',
      );
      expect(screen.getByTestId('Replacement Uploaded by')).toHaveTextContent(
        'replacer@test.com',
      );
      expect(screen.getByTestId('Replacement Date uploaded')).toHaveTextContent(
        '28 September 2020 12:00',
      );

      // Original
      expect(screen.getByTestId('Subject title')).toHaveTextContent(
        'Test data',
      );
      expect(screen.getByTestId('Data file')).toHaveTextContent('data.csv');
      expect(screen.getByTestId('Metadata file')).toHaveTextContent(
        'data.meta.csv',
      );
      expect(screen.getByTestId('Data file size')).toHaveTextContent('200 B');
      expect(screen.getByTestId('Number of rows')).toHaveTextContent('100');
      expect(screen.getByTestId('Status')).toHaveTextContent(
        'Data replacement in progress',
      );
      expect(screen.getByTestId('Uploaded by')).toHaveTextContent(
        'original@test.com',
      );
      expect(screen.getByTestId('Date uploaded')).toHaveTextContent(
        '20 September 2020 12:00',
      );
    });
  });

  test('renders correct error state if unable to load replacement data file', async () => {
    releaseDataFileService.getDataFile.mockResolvedValueOnce({
      ...testOriginalFile,
      replacedBy: testReplacementFile.id,
    });
    releaseDataFileService.getDataFile.mockRejectedValueOnce(
      new Error('Something went wrong'),
    );
    releaseDataFileService.getDataFileImportStatus.mockResolvedValue({
      status: 'COMPLETE',
      percentageComplete: 100,
      stagePercentageComplete: 100,
      totalRows: 100,
    });

    render(
      <MemoryRouter
        initialEntries={[
          generatePath<ReleaseDataFileReplaceRouteParams>(
            releaseDataFileReplaceRoute.path,
            {
              releaseId: 'release-1',
              publicationId: 'publication-1',
              fileId: 'file-1',
            },
          ),
        ]}
      >
        <Route
          path={releaseDataFileReplaceRoute.path}
          component={ReleaseDataFileReplacePage}
        />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(
        screen.getByText(
          'There was a problem loading the replacement file details.',
        ),
      ).toBeInTheDocument();

      expect(screen.getByTestId('Subject title')).toHaveTextContent(
        'Test data',
      );
      expect(screen.getByTestId('Data file')).toHaveTextContent('data.csv');
      expect(screen.getByTestId('Metadata file')).toHaveTextContent(
        'data.meta.csv',
      );
      expect(screen.getByTestId('Data file size')).toHaveTextContent('200 B');
      expect(screen.getByTestId('Number of rows')).toHaveTextContent('100');
      expect(screen.getByTestId('Status')).toHaveTextContent(
        'Data replacement in progress',
      );
      expect(screen.getByTestId('Uploaded by')).toHaveTextContent(
        'original@test.com',
      );
      expect(screen.getByTestId('Date uploaded')).toHaveTextContent(
        '20 September 2020 12:00',
      );

      expect(screen.getByText('Pending data replacement')).toBeInTheDocument();
      expect(
        screen.getByText('Data replacement in progress'),
      ).toBeInTheDocument();
      expect(
        screen.getByText(
          'There was a problem loading the data replacement information.',
        ),
      ).toBeInTheDocument();
    });
  });

  test('renders valid replacement plan', async () => {
    releaseDataFileService.getDataFile.mockResolvedValueOnce({
      ...testOriginalFile,
      replacedBy: testReplacementFile.id,
    });
    releaseDataFileService.getDataFile.mockResolvedValueOnce(
      testReplacementFile,
    );
    releaseDataFileService.getDataFileImportStatus.mockResolvedValue({
      status: 'COMPLETE',
      percentageComplete: 100,
      stagePercentageComplete: 100,
      totalRows: 100,
    });
    dataReplacementService.getReplacementPlan.mockResolvedValue(
      testValidReplacementPlan,
    );

    render(
      <MemoryRouter
        initialEntries={[
          generatePath<ReleaseDataFileReplaceRouteParams>(
            releaseDataFileReplaceRoute.path,
            {
              releaseId: 'release-1',
              publicationId: 'publication-1',
              fileId: 'file-1',
            },
          ),
        ]}
      >
        <Route
          path={releaseDataFileReplaceRoute.path}
          component={ReleaseDataFileReplacePage}
        />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByText('Pending data replacement')).toBeInTheDocument();
      expect(
        screen.getByText('Data replacement in progress'),
      ).toBeInTheDocument();

      expect(screen.getByText('Data blocks: OK')).toBeInTheDocument();
      expect(screen.getByText('Footnotes: OK')).toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Confirm data replacement' }),
      ).toBeInTheDocument();
    });
  });

  test('renders correct state if replacement file has not finished importing', async () => {
    releaseDataFileService.getDataFile.mockResolvedValueOnce({
      ...testOriginalFile,
      replacedBy: testReplacementFile.id,
    });
    releaseDataFileService.getDataFile.mockResolvedValueOnce({
      ...testReplacementFile,
      status: 'QUEUED',
    });
    releaseDataFileService.getDataFileImportStatus.mockResolvedValueOnce({
      status: 'COMPLETE',
      percentageComplete: 100,
      stagePercentageComplete: 100,
      totalRows: 100,
    });
    releaseDataFileService.getDataFileImportStatus.mockResolvedValueOnce({
      status: 'STAGE_1',
      percentageComplete: 10,
      stagePercentageComplete: 100,
      totalRows: 110,
    });

    render(
      <MemoryRouter
        initialEntries={[
          generatePath<ReleaseDataFileReplaceRouteParams>(
            releaseDataFileReplaceRoute.path,
            {
              releaseId: 'release-1',
              publicationId: 'publication-1',
              fileId: 'file-1',
            },
          ),
        ]}
      >
        <Route
          path={releaseDataFileReplaceRoute.path}
          component={ReleaseDataFileReplacePage}
        />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByText('Pending data replacement')).toBeInTheDocument();
      expect(
        screen.getByText('Data replacement in progress'),
      ).toBeInTheDocument();
      expect(
        screen.getByText(/The replacement data file is still being processed/),
      );

      expect(screen.queryByText('Data blocks: OK')).not.toBeInTheDocument();
      expect(screen.queryByText('Footnotes: OK')).not.toBeInTheDocument();
    });
  });

  test('renders correct error state if unable to load replacement plan', async () => {
    releaseDataFileService.getDataFile.mockResolvedValueOnce({
      ...testOriginalFile,
      replacedBy: testReplacementFile.id,
    });
    releaseDataFileService.getDataFile.mockResolvedValueOnce(
      testReplacementFile,
    );
    releaseDataFileService.getDataFileImportStatus.mockResolvedValue({
      status: 'COMPLETE',
      percentageComplete: 100,
      stagePercentageComplete: 100,
      totalRows: 100,
    });
    dataReplacementService.getReplacementPlan.mockRejectedValue(
      new Error('Something went wrong'),
    );

    render(
      <MemoryRouter
        initialEntries={[
          generatePath<ReleaseDataFileReplaceRouteParams>(
            releaseDataFileReplaceRoute.path,
            {
              releaseId: 'release-1',
              publicationId: 'publication-1',
              fileId: 'file-1',
            },
          ),
        ]}
      >
        <Route
          path={releaseDataFileReplaceRoute.path}
          component={ReleaseDataFileReplacePage}
        />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByText('Pending data replacement')).toBeInTheDocument();
      expect(
        screen.getByText('Data replacement in progress'),
      ).toBeInTheDocument();
      expect(
        screen.getByText(
          'There was a problem loading the data replacement information.',
        ),
      );

      expect(screen.queryByText('Data blocks: OK')).not.toBeInTheDocument();
      expect(screen.queryByText('Footnotes: OK')).not.toBeInTheDocument();
    });
  });

  test('confirming replacement cancellation hides modal and calls service to delete replacement file', async () => {
    releaseDataFileService.getDataFile.mockResolvedValueOnce({
      ...testOriginalFile,
      replacedBy: testReplacementFile.id,
    });
    releaseDataFileService.getDataFile.mockResolvedValueOnce(
      testReplacementFile,
    );
    dataReplacementService.getReplacementPlan.mockResolvedValue(
      testValidReplacementPlan,
    );

    render(
      <MemoryRouter
        initialEntries={[
          generatePath<ReleaseDataFileReplaceRouteParams>(
            releaseDataFileReplaceRoute.path,
            {
              releaseId: 'release-1',
              publicationId: 'publication-1',
              fileId: 'file-1',
            },
          ),
        ]}
      >
        <Route
          path={releaseDataFileReplaceRoute.path}
          component={ReleaseDataFileReplacePage}
        />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(
        screen.getByText(
          /Please check the information below before confirming/,
        ),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Cancel data replacement' }),
      ).toBeInTheDocument();
    });

    userEvent.click(
      screen.getByRole('button', { name: 'Cancel data replacement' }),
    );

    await waitFor(() => {
      expect(screen.getByRole('dialog')).toBeInTheDocument();
    });

    expect(releaseDataFileService.deleteDataFiles).not.toHaveBeenCalled();

    userEvent.click(
      within(screen.getByRole('dialog')).getByRole('button', {
        name: 'Confirm',
      }),
    );

    await waitFor(() => {
      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();

      expect(releaseDataFileService.deleteDataFiles).toHaveBeenCalledWith(
        'release-1',
        'data-2',
      );
    });
  });

  test('renders confirmation modal when cancelling FAILED replacement', async () => {
    releaseDataFileService.getDataFile.mockResolvedValueOnce({
      ...testOriginalFile,
      replacedBy: testReplacementFile.id,
    });
    releaseDataFileService.getDataFile.mockResolvedValueOnce({
      ...testReplacementFile,
      status: 'FAILED',
    });
    releaseDataFileService.getDataFileImportStatus.mockResolvedValue({
      status: 'FAILED',
      percentageComplete: 0,
      stagePercentageComplete: 0,
      totalRows: 0,
    });

    render(
      <MemoryRouter
        initialEntries={[
          generatePath<ReleaseDataFileReplaceRouteParams>(
            releaseDataFileReplaceRoute.path,
            {
              releaseId: 'release-1',
              publicationId: 'publication-1',
              fileId: 'file-1',
            },
          ),
        ]}
      >
        <Route
          path={releaseDataFileReplaceRoute.path}
          component={ReleaseDataFileReplacePage}
        />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(
        screen.getByText(/Replacement data file import failed/),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Cancel data replacement' }),
      ).toBeInTheDocument();
    });

    userEvent.click(
      screen.getByRole('button', { name: 'Cancel data replacement' }),
    );

    await waitFor(() => {
      expect(screen.getByRole('dialog')).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));

      expect(modal.getByRole('heading')).toHaveTextContent(
        'Cancel data replacement',
      );
      expect(
        modal.getByText(
          /Are you sure you want to cancel this data replacement/,
        ),
      ).toBeInTheDocument();
    });

    await waitFor(() => {
      expect(
        screen.getByText(/Replacement data file import failed/),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Cancel data replacement' }),
      ).toBeInTheDocument();
    });

    userEvent.click(
      screen.getByRole('button', { name: 'Cancel data replacement' }),
    );

    await waitFor(() => {
      expect(screen.getByRole('dialog')).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));

      expect(modal.getByRole('heading')).toHaveTextContent(
        'Cancel data replacement',
      );
      expect(
        modal.getByText(
          /Are you sure you want to cancel this data replacement/,
        ),
      ).toBeInTheDocument();
    });
  });

  test('renders confirmation modal when cancelling after replacement plan error', async () => {
    releaseDataFileService.getDataFile.mockResolvedValueOnce({
      ...testOriginalFile,
      replacedBy: testReplacementFile.id,
    });
    releaseDataFileService.getDataFile.mockResolvedValueOnce(
      testReplacementFile,
    );
    dataReplacementService.getReplacementPlan.mockRejectedValue(
      new Error('Something went wrong'),
    );

    render(
      <MemoryRouter
        initialEntries={[
          generatePath<ReleaseDataFileReplaceRouteParams>(
            releaseDataFileReplaceRoute.path,
            {
              releaseId: 'release-1',
              publicationId: 'publication-1',
              fileId: 'file-1',
            },
          ),
        ]}
      >
        <Route
          path={releaseDataFileReplaceRoute.path}
          component={ReleaseDataFileReplacePage}
        />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(
        screen.getByText(
          /There was a problem loading the data replacement information/,
        ),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Cancel data replacement' }),
      ).toBeInTheDocument();
    });

    userEvent.click(
      screen.getByRole('button', { name: 'Cancel data replacement' }),
    );

    await waitFor(() => {
      expect(screen.getByRole('dialog')).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));

      expect(modal.getByRole('heading')).toHaveTextContent(
        'Cancel data replacement',
      );
      expect(
        modal.getByText(
          /Are you sure you want to cancel this data replacement/,
        ),
      ).toBeInTheDocument();
    });
  });
});
