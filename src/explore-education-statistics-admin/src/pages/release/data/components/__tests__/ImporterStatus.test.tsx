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
    permissions: {
      canCancelImport: false,
    },
  };

  beforeEach(() => {
    jest.useFakeTimers();
  });

  test('renders initial complete status correctly', () => {
    render(<ImporterStatus releaseId="release-1" dataFile={testDataFile} />);

    expect(screen.getByText('Complete')).toBeInTheDocument();
    expect(screen.queryByRole('group')).not.toBeInTheDocument();
  });

  test('does not fetch status from service if data file status is already complete', () => {
    releaseDataFileService.getDataFileImportStatus.mockResolvedValue({
      status: 'COMPLETE',
      percentageComplete: 100,
      stagePercentageComplete: 100,
      totalRows: 100,
    });

    render(<ImporterStatus releaseId="release-1" dataFile={testDataFile} />);

    expect(
      releaseDataFileService.getDataFileImportStatus,
    ).not.toHaveBeenCalled();
  });

  test('renders with updated status from service', async () => {
    releaseDataFileService.getDataFileImportStatus.mockResolvedValue({
      status: 'STAGE_1',
      percentageComplete: 100,
      stagePercentageComplete: 100,
      totalRows: 100,
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
    expect(
      releaseDataFileService.getDataFileImportStatus,
    ).toHaveBeenCalledTimes(1);

    await waitFor(() => {
      expect(screen.getByText('Validating')).toBeInTheDocument();
      expect(
        releaseDataFileService.getDataFileImportStatus,
      ).toHaveBeenCalledTimes(1);
    });
  });

  test('renders with updated status from service at regular intervals', async () => {
    releaseDataFileService.getDataFileImportStatus.mockResolvedValue({
      status: 'STAGE_1',
      percentageComplete: 10,
      stagePercentageComplete: 100,
      totalRows: 100,
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

    expect(
      releaseDataFileService.getDataFileImportStatus,
    ).toHaveBeenCalledTimes(1);

    await waitFor(() => {
      expect(screen.getByText('Validating')).toBeInTheDocument();
      expect(screen.getByRole('progressbar')).toHaveAttribute(
        'aria-valuenow',
        '10',
      );
      expect(screen.getByText('10% complete')).toBeInTheDocument();
    });

    releaseDataFileService.getDataFileImportStatus.mockResolvedValue({
      status: 'STAGE_2',
      percentageComplete: 50,
      stagePercentageComplete: 100,
      totalRows: 100,
    });

    jest.advanceTimersByTime(5000);

    expect(
      releaseDataFileService.getDataFileImportStatus,
    ).toHaveBeenCalledTimes(2);

    await waitFor(() => {
      expect(screen.getByText('Importing')).toBeInTheDocument();
      expect(screen.getByRole('progressbar')).toHaveAttribute(
        'aria-valuenow',
        '50',
      );
      expect(screen.getByText('50% complete')).toBeInTheDocument();
    });

    releaseDataFileService.getDataFileImportStatus.mockResolvedValue({
      status: 'COMPLETE',
      percentageComplete: 100,
      stagePercentageComplete: 100,
      totalRows: 100,
    });

    jest.advanceTimersByTime(5000);

    expect(
      releaseDataFileService.getDataFileImportStatus,
    ).toHaveBeenCalledTimes(3);

    await waitFor(() => {
      expect(screen.getByText('Complete')).toBeInTheDocument();
      expect(screen.queryByRole('progressbar')).not.toBeInTheDocument();
      expect(screen.queryByText('100% complete')).not.toBeInTheDocument();
    });

    expect(screen.queryByRole('group')).not.toBeInTheDocument();
  });

  test('renders with error messages from service', async () => {
    releaseDataFileService.getDataFileImportStatus.mockResolvedValue({
      status: 'FAILED',
      percentageComplete: 0,
      stagePercentageComplete: 100,
      totalRows: 100,
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
