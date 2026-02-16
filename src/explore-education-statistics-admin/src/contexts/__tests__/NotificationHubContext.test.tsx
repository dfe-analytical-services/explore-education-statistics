import {
  NotificationHubContextProvider,
  useNotificationHubContext,
} from '@admin/contexts/NotificationHubContext';
import connectionMock from '@admin/services/hubs/utils/__mocks__/connectionMock';
import { HubConnectionState } from '@microsoft/signalr';
import { render, renderHook, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React, { FC, ReactNode } from 'react';

jest.mock('@admin/services/hubs/utils/createConnection');

const wrapper: FC = ({ children }: { children?: ReactNode }) => (
  <NotificationHubContextProvider>{children}</NotificationHubContextProvider>
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
  const senderName = 'Joe Bloggs';

  test('starts in a Disconnected state', () => {
    const { result } = renderHook(() => useNotificationHubContext(), {
      wrapper,
    });

    expect(result.current.status).toBe(HubConnectionState.Disconnected);
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

    let onServiceAnnouncement:
      | ((senderName: string, message: string) => void)
      | undefined;

    connectionMock.on.mockImplementation((methodName, callback) => {
      if (methodName === 'ServiceAnnouncement') {
        onServiceAnnouncement = callback;
      }
    });

    render(
      <NotificationHubContextProvider>Test</NotificationHubContextProvider>,
    );

    jest.advanceTimersByTime(50);

    stateSpy.mockReturnValue(HubConnectionState.Connected);

    await waitFor(() => {
      expect(onServiceAnnouncement).toBeDefined();
    });

    onServiceAnnouncement?.(senderName, testMessage);

    await waitFor(() => {
      expect(
        screen.getByRole('heading', { name: 'Service announcement' }),
      ).toBeInTheDocument();
    });

    expect(
      screen.getByText(content => content.includes(senderName)),
    ).toBeInTheDocument();
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

    let onServiceAnnouncement:
      | ((senderName: string, message: string) => void)
      | undefined;

    connectionMock.on.mockImplementation((methodName, callback) => {
      if (methodName === 'ServiceAnnouncement') {
        onServiceAnnouncement = callback;
      }
    });

    render(
      <NotificationHubContextProvider>Test</NotificationHubContextProvider>,
    );

    jest.advanceTimersByTime(50);

    stateSpy.mockReturnValue(HubConnectionState.Connected);

    await waitFor(() => {
      expect(onServiceAnnouncement).toBeDefined();
    });

    const message1 = 'Service will be unavailable for 15 minutes';
    const message2 = 'New features released';

    onServiceAnnouncement?.(senderName, message1);

    await waitFor(() => {
      expect(
        screen.getByText(content => content.includes(senderName)),
      ).toBeInTheDocument();
      expect(
        screen.getByText(content => content.includes(message1)),
      ).toBeInTheDocument();
    });

    // Trigger another message
    onServiceAnnouncement?.(senderName, message2);

    await waitFor(() => {
      expect(
        screen.getByText(content => content.includes(senderName)),
      ).toBeInTheDocument();
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

    let onServiceAnnouncement:
      | ((senderName: string, message: string) => void)
      | undefined;

    connectionMock.on.mockImplementation((methodName, callback) => {
      if (methodName === 'ServiceAnnouncement') {
        onServiceAnnouncement = callback;
      }
    });

    render(
      <NotificationHubContextProvider>Test</NotificationHubContextProvider>,
    );

    jest.advanceTimersByTime(50);

    stateSpy.mockReturnValue(HubConnectionState.Connected);

    await waitFor(() => {
      expect(onServiceAnnouncement).toBeDefined();
    });

    onServiceAnnouncement?.(senderName, testMessage);

    await waitFor(() => {
      expect(
        screen.getByText(content => content.includes(senderName)),
      ).toBeInTheDocument();
      expect(
        screen.getByText(content => content.includes(testMessage)),
      ).toBeInTheDocument();
    });

    // Switch to real timers for user interaction
    jest.useRealTimers();

    const closeButton = screen.getByRole('button', { name: 'Close modal' });
    await userEvent.click(closeButton);

    await waitFor(() => {
      expect(screen.queryByText(testMessage)).not.toBeInTheDocument();
    });
  });
});
