import { render, screen, waitFor, within } from '@testing-library/react';
import ImporterStatus from '@admin/pages/release/data/components/ImporterStatus';
import _releaseDataFileService, {
  DataFile,
} from '@admin/services/releaseDataFileService';
import React from 'react';

jest.mock('@admin/services/releaseDataFileService');

const releaseDataFileService = _releaseDataFileService as jest.Mocked<
  typeof _releaseDataFileService
>;

describe('ImporterStatus', () => {
  const testDataFile: DataFile = {
    id: 'file-1',
    rows: 100,
    fileName: 'data.csv',
    fileSize: {
      size: 200,
      unit: 'B',
    },
    userName: 'test@test.com',
    title: 'Test data',
    metaFileId: 'file-meta-1',
    metaFileName: 'meta.csv',
    status: 'COMPLETE',
  };

  beforeEach(() => {
    jest.useFakeTimers();
  });

  test('renders initial status correctly', () => {
    releaseDataFileService.getDataFileImportStatus.mockResolvedValue({
      status: 'COMPLETE',
      numberOfRows: 100,
    });

    render(<ImporterStatus releaseId="release-1" dataFile={testDataFile} />);

    expect(screen.getByText('Complete')).toBeInTheDocument();
    expect(screen.queryByRole('group')).not.toBeInTheDocument();
  });

  test('renders with updated status from service', async () => {
    releaseDataFileService.getDataFileImportStatus.mockResolvedValue({
      status: 'RUNNING_PHASE_1',
      numberOfRows: 100,
    });

    render(
      <ImporterStatus
        releaseId="release-1"
        dataFile={{
          ...testDataFile,
          status: 'QUEUED',
        }}
      />,
    );

    expect(screen.getByText('Queued')).toBeInTheDocument();

    await waitFor(() => {
      expect(screen.getByText('Validating')).toBeInTheDocument();
    });
  });

  test('renders with updated status from service at regular intervals', async () => {
    releaseDataFileService.getDataFileImportStatus.mockResolvedValue({
      status: 'RUNNING_PHASE_1',
      numberOfRows: 100,
    });

    render(
      <ImporterStatus
        releaseId="release-1"
        dataFile={{
          ...testDataFile,
          status: 'QUEUED',
        }}
      />,
    );

    expect(screen.getByText('Queued')).toBeInTheDocument();

    await waitFor(() => {
      expect(screen.getByText('Validating')).toBeInTheDocument();
    });

    releaseDataFileService.getDataFileImportStatus.mockResolvedValue({
      status: 'RUNNING_PHASE_2',
      numberOfRows: 100,
    });

    jest.advanceTimersByTime(5000);

    await waitFor(() => {
      expect(screen.getByText('Importing')).toBeInTheDocument();
    });

    releaseDataFileService.getDataFileImportStatus.mockResolvedValue({
      status: 'COMPLETE',
      numberOfRows: 100,
    });

    jest.advanceTimersByTime(5000);

    await waitFor(() => {
      expect(screen.getByText('Complete')).toBeInTheDocument();
    });

    expect(screen.queryByRole('group')).not.toBeInTheDocument();
  });

  test('renders with error messages from service', async () => {
    releaseDataFileService.getDataFileImportStatus.mockResolvedValue({
      status: 'FAILED',
      numberOfRows: 100,
      errors: ['Some error 1', 'Some error 2'],
    });

    render(
      <ImporterStatus
        releaseId="release-1"
        dataFile={{
          ...testDataFile,
          status: 'QUEUED',
        }}
      />,
    );

    expect(screen.getByText('Queued')).toBeInTheDocument();
    expect(screen.queryByRole('group')).not.toBeInTheDocument();

    await waitFor(() => {
      expect(screen.getByText('Failed')).toBeInTheDocument();

      const details = within(screen.getByRole('group'));

      expect(
        details.getByRole('button', { name: 'See errors' }),
      ).toBeInTheDocument();

      const errors = details.getAllByRole('listitem', { hidden: true });

      expect(errors).toHaveLength(2);
      expect(errors[0]).toHaveTextContent('Some error 1');
      expect(errors[1]).toHaveTextContent('Some error 2');
    });
  });
});
