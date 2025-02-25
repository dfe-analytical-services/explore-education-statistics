import ReleasePublishingStatus from '@admin/pages/release/components/ReleasePublishingStatus';
import _releaseService from '@admin/services/releaseVersionService';
import { render, screen } from '@testing-library/react';
import React from 'react';

jest.mock('@admin/services/releaseVersionService');

const releaseVersionService = jest.mocked(_releaseService);

describe('ReleasePublishingStatus', () => {
  test('renders correctly with initial status', async () => {
    releaseVersionService.getReleaseVersionStatus.mockResolvedValue({
      overallStage: 'Started',
      filesStage: 'Queued',
      contentStage: 'Complete',
      publishingStage: 'NotStarted',
    });

    render(<ReleasePublishingStatus releaseVersionId="release-1" />);

    expect(await screen.findByText('Started')).toBeInTheDocument();

    expect(screen.getByText('Files - queued')).toBeInTheDocument();
    expect(screen.getByText('Content - complete')).toBeInTheDocument();
    expect(screen.getByText('Publishing - not started')).toBeInTheDocument();
  });

  test('re-renders correctly when status changes to complete', async () => {
    jest.useFakeTimers();

    releaseVersionService.getReleaseVersionStatus.mockResolvedValue({
      overallStage: 'Started',
    });

    render(<ReleasePublishingStatus releaseVersionId="release-1" />);

    expect(await screen.findByText('Started')).toBeInTheDocument();

    releaseVersionService.getReleaseVersionStatus.mockResolvedValue({
      overallStage: 'Complete',
      contentStage: 'Complete',
      publishingStage: 'Complete',
      filesStage: 'Complete',
    });

    jest.runOnlyPendingTimers();

    expect(await screen.findByText('Complete')).toBeInTheDocument();

    expect(screen.getByText('Content - complete')).toBeInTheDocument();
    expect(screen.getByText('Files - complete')).toBeInTheDocument();
    expect(screen.getByText('Publishing - complete')).toBeInTheDocument();

    jest.useRealTimers();
  });

  test('calls `onChange` only when status changes', async () => {
    jest.useFakeTimers();

    releaseVersionService.getReleaseVersionStatus.mockResolvedValue({
      overallStage: 'Started',
    });

    const handleChange = jest.fn();

    render(
      <ReleasePublishingStatus
        releaseVersionId="release-1"
        onChange={handleChange}
      />,
    );

    expect(await screen.findByText('Started')).toBeInTheDocument();

    expect(handleChange).toHaveBeenCalledTimes(1);

    jest.runOnlyPendingTimers();

    expect(await screen.findByText('Started')).toBeInTheDocument();

    // Not called again because the status hasn't changed
    expect(handleChange).toHaveBeenCalledTimes(1);

    releaseVersionService.getReleaseVersionStatus.mockResolvedValue({
      overallStage: 'Complete',
      contentStage: 'Complete',
      publishingStage: 'Complete',
      filesStage: 'Complete',
    });

    jest.runOnlyPendingTimers();

    expect(await screen.findByText('Complete')).toBeInTheDocument();

    expect(handleChange).toHaveBeenCalledTimes(2);

    jest.useRealTimers();
  });

  test('does not re-render or call service after overall status is no longer `Started`', async () => {
    jest.useFakeTimers();

    releaseVersionService.getReleaseVersionStatus.mockResolvedValue({
      overallStage: 'Started',
    });

    const handleChange = jest.fn();

    render(
      <ReleasePublishingStatus
        releaseVersionId="release-1"
        onChange={handleChange}
      />,
    );

    expect(await screen.findByText('Started')).toBeInTheDocument();

    expect(releaseVersionService.getReleaseVersionStatus).toHaveBeenCalledTimes(
      1,
    );

    releaseVersionService.getReleaseVersionStatus.mockResolvedValue({
      overallStage: 'Complete',
    });

    jest.runOnlyPendingTimers();

    expect(await screen.findByText('Complete')).toBeInTheDocument();

    expect(releaseVersionService.getReleaseVersionStatus).toHaveBeenCalledTimes(
      2,
    );

    // Can't really transition from Complete to Failed, but
    // just simulating a change for the test.
    releaseVersionService.getReleaseVersionStatus.mockResolvedValue({
      overallStage: 'Failed',
    });

    jest.runOnlyPendingTimers();

    expect(await screen.findByText('Complete')).toBeInTheDocument();

    expect(releaseVersionService.getReleaseVersionStatus).toHaveBeenCalledTimes(
      2,
    );

    jest.useRealTimers();
  });
});
