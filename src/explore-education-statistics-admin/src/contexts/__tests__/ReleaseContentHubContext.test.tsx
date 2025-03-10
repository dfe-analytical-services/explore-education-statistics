import {
  ReleaseContentHubContextProvider,
  useReleaseContentHubContext,
} from '@admin/contexts/ReleaseContentHubContext';
import connectionMock from '@admin/services/hubs/utils/__mocks__/connectionMock';
import { HubConnectionState } from '@microsoft/signalr';
import { renderHook, waitFor } from '@testing-library/react';
import React, { FC, ReactNode } from 'react';

jest.mock('@admin/services/hubs/utils/createConnection');

const wrapper: FC = ({ children }: { children?: ReactNode }) => (
  <ReleaseContentHubContextProvider releaseVersionId="release-1">
    {children}
  </ReleaseContentHubContextProvider>
);

type SendParameters = Parameters<typeof connectionMock.send>;

describe('ReleaseContentHubContext', () => {
  beforeEach(() => {
    jest.useFakeTimers();
  });

  test('starts in a Disconnected state', () => {
    const { result } = renderHook(() => useReleaseContentHubContext(), {
      wrapper,
    });

    expect(result.current.status).toBe(HubConnectionState.Disconnected);
  });

  test('changes to a Connected state', async () => {
    jest
      .spyOn(connectionMock, 'state', 'get')
      .mockReturnValue(HubConnectionState.Connected);

    const { result } = renderHook(() => useReleaseContentHubContext(), {
      wrapper,
    });

    await waitFor(() => {
      expect(result.current.status).toBe(HubConnectionState.Connected);
    });
  });

  test('joins release group when Connected', async () => {
    connectionMock.start.mockImplementation(
      () => new Promise(resolve => setTimeout(resolve, 50)),
    );

    const stateSpy = jest
      .spyOn(connectionMock, 'state', 'get')
      .mockReturnValue(HubConnectionState.Disconnected);

    renderHook(() => useReleaseContentHubContext(), { wrapper });

    expect(connectionMock.send).not.toHaveBeenCalled();

    jest.advanceTimersByTime(50);

    stateSpy.mockReturnValue(HubConnectionState.Connected);

    await waitFor(() => {
      expect(connectionMock.send).toHaveBeenCalledTimes(1);
    });
    expect(connectionMock.send).toHaveBeenCalledWith<SendParameters>(
      'JoinReleaseGroup',
      {
        id: 'release-1',
      },
    );
  });

  test('leaves release group when unmounting and Connected', async () => {
    connectionMock.start.mockImplementation(
      () => new Promise(resolve => setTimeout(resolve, 50)),
    );

    const stateSpy = jest
      .spyOn(connectionMock, 'state', 'get')
      .mockReturnValue(HubConnectionState.Disconnected);

    const { unmount } = renderHook(() => useReleaseContentHubContext(), {
      wrapper,
    });

    expect(connectionMock.send).not.toHaveBeenCalled();

    jest.advanceTimersByTime(50);

    stateSpy.mockReturnValue(HubConnectionState.Connected);

    await waitFor(() => {
      expect(connectionMock.send).toHaveBeenCalledTimes(1);
    });

    unmount();

    await waitFor(() => {
      expect(connectionMock.send).toHaveBeenCalledTimes(2);
    });

    expect(connectionMock.send).toHaveBeenNthCalledWith<SendParameters>(
      1,
      'JoinReleaseGroup',
      { id: 'release-1' },
    );
    expect(connectionMock.send).toHaveBeenNthCalledWith<SendParameters>(
      2,
      'LeaveReleaseGroup',
      { id: 'release-1' },
    );
  });

  test('does not leave release group when unmounting and Disconnected', async () => {
    connectionMock.start.mockImplementation(
      () => new Promise(resolve => setTimeout(resolve, 50)),
    );

    const stateSpy = jest
      .spyOn(connectionMock, 'state', 'get')
      .mockReturnValue(HubConnectionState.Disconnected);

    const { unmount } = renderHook(() => useReleaseContentHubContext(), {
      wrapper,
    });

    expect(connectionMock.send).not.toHaveBeenCalled();

    jest.advanceTimersByTime(50);

    stateSpy.mockReturnValue(HubConnectionState.Connected);
    await waitFor(() => {
      // Only joins, does not leave
      expect(connectionMock.send).toHaveBeenCalledTimes(1);
    });
    stateSpy.mockReturnValue(HubConnectionState.Disconnected);

    unmount();

    expect(connectionMock.send).toHaveBeenNthCalledWith<SendParameters>(
      1,
      'JoinReleaseGroup',
      { id: 'release-1' },
    );
  });
});
