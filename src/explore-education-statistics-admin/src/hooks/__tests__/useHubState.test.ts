import useHubState from '@admin/hooks/useHubState';
import Hub from '@admin/services/hubs/utils/Hub';
import { HubConnectionState } from '@microsoft/signalr';
import { waitFor } from '@testing-library/react';
import { renderHook } from '@testing-library/react-hooks';
import { mock } from 'jest-mock-extended';
import flushPromises from '@common-test/flushPromises';

describe('useHubState', () => {
  const mockHub = mock<Hub>();

  beforeEach(() => {
    jest.useFakeTimers({
      legacyFakeTimers: true,
    });
  });

  afterEach(() => {
    jest.runOnlyPendingTimers();
    jest.useRealTimers();
  });

  test('calls `start` on hub when mounted', () => {
    mockHub.start.mockResolvedValue();

    renderHook(() => useHubState(() => mockHub));

    expect(mockHub.start).toHaveBeenCalledTimes(1);
  });

  test('returns updated `status` when hub has started', async () => {
    mockHub.start.mockReturnValue(
      new Promise(resolve => setTimeout(resolve, 500)),
    );

    mockHub.status.mockReturnValue(HubConnectionState.Disconnected);

    const { result, waitForNextUpdate } = renderHook(() =>
      useHubState(() => mockHub),
    );

    expect(result.current.status).toBe(HubConnectionState.Disconnected);

    mockHub.status.mockReturnValue(HubConnectionState.Connected);

    jest.runOnlyPendingTimers();

    await waitForNextUpdate();

    expect(result.current.status).toBe(HubConnectionState.Connected);
  });

  test('calls `stop` on hub when unmounted', () => {
    mockHub.start.mockResolvedValue();

    const { unmount } = renderHook(() => useHubState(() => mockHub));

    expect(mockHub.stop).not.toHaveBeenCalled();

    unmount();

    expect(mockHub.stop).toHaveBeenCalledTimes(1);
  });

  test('calls `stop` when unmounted but hub has only just connected', async () => {
    mockHub.start.mockReturnValue(
      new Promise(resolve => setTimeout(resolve, 500)),
    );

    const { unmount } = renderHook(() => useHubState(() => mockHub));

    unmount();

    expect(mockHub.stop).toHaveBeenCalledTimes(1);

    jest.runOnlyPendingTimers();
    await flushPromises();

    await waitFor(() => {
      expect(mockHub.stop).toHaveBeenCalledTimes(2);
    });
  });
});
