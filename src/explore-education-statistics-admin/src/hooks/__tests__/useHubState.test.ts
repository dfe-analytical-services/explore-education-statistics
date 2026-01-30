import useHubState from '@admin/hooks/useHubState';
import Hub from '@admin/services/hubs/utils/Hub';
import { HubConnectionState } from '@microsoft/signalr';
import { act, renderHook, waitFor } from '@testing-library/react';
import { mock } from 'jest-mock-extended';

describe('useHubState', () => {
  const mockHub = mock<Hub>();

  beforeEach(() => {
    jest.useFakeTimers();
  });

  test('calls `start` on hub when mounted', async () => {
    mockHub.start.mockResolvedValue();

    renderHook(() => useHubState(() => mockHub));

    await waitFor(() => {
      expect(mockHub.start).toHaveBeenCalledTimes(1);
    });
  });

  test('returns updated `status` when hub has started', async () => {
    mockHub.start.mockReturnValue(
      new Promise(resolve => setTimeout(resolve, 500)),
    );

    mockHub.status.mockReturnValue(HubConnectionState.Disconnected);

    const { result } = renderHook(() => useHubState(() => mockHub));

    expect(result.current.status).toBe(HubConnectionState.Disconnected);

    mockHub.status.mockReturnValue(HubConnectionState.Connected);

    await act(async () => {
      jest.runOnlyPendingTimers();
    });

    await waitFor(() => {
      expect(result.current.status).toBe(HubConnectionState.Connected);
    });
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

    await act(async () => {
      jest.runOnlyPendingTimers();
    });

    await waitFor(() => {
      expect(mockHub.stop).toHaveBeenCalledTimes(2);
    });
  });
});
