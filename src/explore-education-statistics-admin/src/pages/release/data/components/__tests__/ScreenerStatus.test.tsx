import ScreenerStatus from '@admin/pages/release/data/components/ScreenerStatus';
import _releaseDataFileService, {
  DataSetUpload,
} from '@admin/services/releaseDataFileService';
import { render, screen, waitFor } from '@testing-library/react';

jest.mock('@admin/services/releaseDataFileService');

const releaseDataFileService = _releaseDataFileService as jest.Mocked<
  typeof _releaseDataFileService
>;

describe('ScreenerStatus', () => {
  const testDataSetUpload: DataSetUpload = {
    id: 'upload-1',
    dataSetTitle: 'Test data set',
    dataFileName: 'data.csv',
    dataFileSize: '200 B',
    metaFileName: 'meta.csv',
    metaFileSize: '100 B',
    screeningStatus: 'PendingReview',
    created: new Date(),
    uploadedBy: 'test@test.com',
  };

  beforeEach(() => {
    jest.useFakeTimers();
  });

  afterEach(() => {
    jest.useRealTimers();
  });

  test('renders initial terminal status correctly', () => {
    render(
      <ScreenerStatus
        releaseVersionId="release-1"
        dataSetUpload={testDataSetUpload}
      />,
    );

    expect(screen.getByText('Pending review')).toBeInTheDocument();
    expect(screen.queryByRole('progressbar')).not.toBeInTheDocument();
  });

  // TODO EES-7139 - remove handling for null status once the foreground screening
  // process has been decommissioned.
  test('renders null status correctly', () => {
    render(
      <ScreenerStatus
        releaseVersionId="release-1"
        dataSetUpload={{
          ...testDataSetUpload,
          screeningStatus: undefined,
        }}
      />,
    );

    expect(screen.getByText('Screening')).toBeInTheDocument();
    expect(screen.queryByRole('progressbar')).not.toBeInTheDocument();
  });

  test('does not fetch status from service if data set upload status is already terminal', () => {
    releaseDataFileService.getDataFileScreeningStatus.mockResolvedValue({
      status: 'PendingReview',
      percentageComplete: 100,
      stage: 'COMPLETE',
      completed: true,
    });

    render(
      <ScreenerStatus
        releaseVersionId="release-1"
        dataSetUpload={testDataSetUpload}
      />,
    );

    expect(
      releaseDataFileService.getDataFileScreeningStatus,
    ).not.toHaveBeenCalled();
  });

  test('renders with updated status from service', async () => {
    releaseDataFileService.getDataFileScreeningStatus.mockResolvedValue({
      status: 'Screening',
      percentageComplete: 25,
      stage: 'STAGE_1',
      completed: false,
    });

    render(
      <ScreenerStatus
        releaseVersionId="release-1"
        dataSetUpload={{
          ...testDataSetUpload,
          screeningStatus: 'Screening',
        }}
      />,
    );

    expect(screen.getByText('Screening')).toBeInTheDocument();
    expect(
      releaseDataFileService.getDataFileScreeningStatus,
    ).toHaveBeenCalledTimes(1);

    await waitFor(() =>
      expect(screen.getByRole('progressbar')).toHaveAttribute(
        'aria-valuenow',
        '25',
      ),
    );
  });

  test('renders with updated status from service at regular intervals', async () => {
    releaseDataFileService.getDataFileScreeningStatus.mockResolvedValue({
      status: 'Screening',
      percentageComplete: 10,
      stage: 'STAGE_1',
      completed: false,
    });

    render(
      <ScreenerStatus
        releaseVersionId="release-1"
        dataSetUpload={{
          ...testDataSetUpload,
          screeningStatus: 'Screening',
        }}
      />,
    );

    expect(screen.getByText('Screening')).toBeInTheDocument();
    expect(
      releaseDataFileService.getDataFileScreeningStatus,
    ).toHaveBeenCalledTimes(1);

    await waitFor(() =>
      expect(screen.getByRole('progressbar')).toHaveAttribute(
        'aria-valuenow',
        '10',
      ),
    );

    releaseDataFileService.getDataFileScreeningStatus.mockResolvedValue({
      status: 'PendingImport',
      percentageComplete: 100,
      stage: 'COMPLETE',
      completed: true,
    });

    jest.advanceTimersByTime(5000);

    expect(
      releaseDataFileService.getDataFileScreeningStatus,
    ).toHaveBeenCalledTimes(2);

    expect(await screen.findByText('Pending import')).toBeInTheDocument();
    expect(screen.queryByRole('progressbar')).not.toBeInTheDocument();
  });

  test('calls onStatusChange when remote status changes', async () => {
    releaseDataFileService.getDataFileScreeningStatus.mockResolvedValue({
      status: 'PendingImport',
      percentageComplete: 100,
      stage: 'COMPLETE',
      completed: true,
    });

    const onStatusChange = jest.fn();

    render(
      <ScreenerStatus
        releaseVersionId="release-1"
        dataSetUpload={{
          ...testDataSetUpload,
          screeningStatus: 'Screening',
        }}
        onStatusChange={onStatusChange}
      />,
    );

    expect(onStatusChange).not.toHaveBeenCalled();

    expect(await screen.findByText('Pending import')).toBeInTheDocument();

    expect(onStatusChange).toHaveBeenCalledTimes(1);
    expect(onStatusChange).toHaveBeenCalledWith(
      expect.objectContaining({ id: 'upload-1' }),
      expect.objectContaining({ status: 'PendingImport' }),
    );
  });
});
