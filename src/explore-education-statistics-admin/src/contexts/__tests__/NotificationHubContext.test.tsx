import {
  NotificationHubContextProvider,
  useNotificationHubContext,
} from '@admin/contexts/NotificationHubContext';
import connectionMock from '@admin/services/hubs/utils/__mocks__/connectionMock';
import render from '@common-test/render';
import { HubConnectionState } from '@microsoft/signalr';
import { act, renderHook, screen, waitFor } from '@testing-library/react';
import { UserEvent } from '@testing-library/user-event';
import React, { FC, ReactNode } from 'react';
import { AuthContextTestProvider, User } from '@admin/contexts/AuthContext';
import { GlobalPermissions } from '@admin/services/authService';

jest.mock('@admin/services/hubs/utils/createConnection');

const nonBauUser: User = {
  id: 'user-id',
  name: 'Analyst 1',
  permissions: {
    isBauUser: false,
  } as GlobalPermissions,
};

const wrapper: FC = ({ children }: { children?: ReactNode }) => (
  <AuthContextTestProvider user={nonBauUser}>
    <NotificationHubContextProvider>{children}</NotificationHubContextProvider>
  </AuthContextTestProvider>
);

describe('NotificationHubContext', () => {
  beforeEach(() => {
    jest.useFakeTimers();
  });

  afterEach(() => {
    jest.useRealTimers();
    jest.clearAllMocks();
  });

  const testMessage =
    'Scheduled system update will occur in 15 minutes, please save your work.';

  test('hub state is undefined when user is not authenticated', async () => {
    await act(async () => {
      render(
        <NotificationHubContextProvider>
          {hubState => {
            expect(hubState).toBeUndefined();
            return <div>Test</div>;
          }}
        </NotificationHubContextProvider>,
      );
      jest.advanceTimersByTime(50);
    });

    expect(connectionMock.start).not.toHaveBeenCalled();
  });

  test('starts in a Disconnected state', async () => {
    const { result } = renderHook(() => useNotificationHubContext(), {
      wrapper,
    });

    await waitFor(() => {
      expect(result.current.status).toBe(HubConnectionState.Disconnected);
    });
  });

  test('changes to a Connected state', async () => {
    jest
      .spyOn(connectionMock, 'state', 'get')
      .mockReturnValue(HubConnectionState.Connected);

    const { result } = renderHook(() => useNotificationHubContext(), {
      wrapper,
    });

    await waitFor(() => {
      expect(result.current.status).toBe(HubConnectionState.Connected);
    });
  });

  test('opens modal when ServiceAnnouncement is triggered', async () => {
    const stateSpy = jest
      .spyOn(connectionMock, 'state', 'get')
      .mockReturnValue(HubConnectionState.Disconnected);

    connectionMock.start.mockImplementation(
      () => new Promise(resolve => setTimeout(resolve, 50)),
    );

    let onServiceAnnouncement: ((message: string) => void) | undefined;

    connectionMock.on.mockImplementation((methodName, callback) => {
      if (methodName === 'ServiceAnnouncement') {
        onServiceAnnouncement = callback;
      }
    });

    await act(async () => {
      render(
        <AuthContextTestProvider user={nonBauUser}>
          <NotificationHubContextProvider>Test</NotificationHubContextProvider>
        </AuthContextTestProvider>,
      );
      jest.advanceTimersByTime(50);
    });

    stateSpy.mockReturnValue(HubConnectionState.Connected);

    await waitFor(() => {
      expect(onServiceAnnouncement).toBeDefined();
    });

    await act(async () => {
      onServiceAnnouncement?.(testMessage);
    });

    const heading = await screen.findByRole('heading', {
      name: 'Service announcement',
    });

    expect(heading).toBeInTheDocument();

    expect(
      screen.getByText(content => content.includes(testMessage)),
    ).toBeInTheDocument();
  });

  test('subsequent notifications overwrite previous messages if multiple are sent', async () => {
    const stateSpy = jest
      .spyOn(connectionMock, 'state', 'get')
      .mockReturnValue(HubConnectionState.Disconnected);

    connectionMock.start.mockImplementation(
      () => new Promise(resolve => setTimeout(resolve, 50)),
    );

    let onServiceAnnouncement: ((message: string) => void) | undefined;

    connectionMock.on.mockImplementation((methodName, callback) => {
      if (methodName === 'ServiceAnnouncement') {
        onServiceAnnouncement = callback;
      }
    });

    await act(async () => {
      render(
        <AuthContextTestProvider user={nonBauUser}>
          <NotificationHubContextProvider>Test</NotificationHubContextProvider>
        </AuthContextTestProvider>,
      );
      jest.advanceTimersByTime(50);
    });

    stateSpy.mockReturnValue(HubConnectionState.Connected);

    await waitFor(() => {
      expect(onServiceAnnouncement).toBeDefined();
    });

    const message1 = 'Service will be unavailable for 15 minutes';
    const message2 = 'New features released';

    await act(async () => {
      onServiceAnnouncement?.(message1);
    });

    await waitFor(() => {
      expect(
        screen.getByText(content => content.includes(message1)),
      ).toBeInTheDocument();
    });

    // Trigger another message
    await act(async () => {
      onServiceAnnouncement?.(message2);
    });

    await waitFor(() => {
      expect(
        screen.getByText(content => content.includes(message2)),
      ).toBeInTheDocument();
    });
  });

  test('closes modal when close button is clicked', async () => {
    const stateSpy = jest
      .spyOn(connectionMock, 'state', 'get')
      .mockReturnValue(HubConnectionState.Disconnected);

    connectionMock.start.mockImplementation(
      () => new Promise(resolve => setTimeout(resolve, 50)),
    );

    let onServiceAnnouncement: ((message: string) => void) | undefined;

    connectionMock.on.mockImplementation((methodName, callback) => {
      if (methodName === 'ServiceAnnouncement') {
        onServiceAnnouncement = callback;
      }
    });

    let user!: UserEvent;
    await act(async () => {
      const renderResult = render(
        <AuthContextTestProvider user={nonBauUser}>
          <NotificationHubContextProvider>Test</NotificationHubContextProvider>
        </AuthContextTestProvider>,
      );
      user = renderResult.user;
      jest.advanceTimersByTime(50);
    });

    stateSpy.mockReturnValue(HubConnectionState.Connected);

    await waitFor(() => {
      expect(onServiceAnnouncement).toBeDefined();
    });

    await act(async () => {
      onServiceAnnouncement?.(testMessage);
    });

    await waitFor(() => {
      expect(
        screen.getByText(content => content.includes(testMessage)),
      ).toBeInTheDocument();
    });

    // Switch to real timers for user interaction
    jest.useRealTimers();

    await user.click(screen.getByRole('button', { name: 'Close modal' }));

    await waitFor(() => {
      expect(screen.queryByText(testMessage)).not.toBeInTheDocument();
    });
  });
});
