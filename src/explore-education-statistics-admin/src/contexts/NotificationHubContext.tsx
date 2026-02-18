import { useAuthContext } from '@admin/contexts/AuthContext';
import useHubState, { HubState } from '@admin/hooks/useHubState';
import notificationHub, {
  NotificationHub,
} from '@admin/services/hubs/notificationHub';
import FormattedDate from '@common/components/FormattedDate';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Modal from '@common/components/Modal';
import React, {
  createContext,
  ReactNode,
  useContext,
  useEffect,
  useState,
} from 'react';

const NotificationHubContext = createContext<
  HubState<NotificationHub> | undefined
>(undefined);

interface NotificationHubContextProviderProps {
  children:
    | ReactNode
    | ((value: HubState<NotificationHub> | undefined) => ReactNode);
}

export function NotificationHubContextProvider({
  children,
}: NotificationHubContextProviderProps) {
  const { user } = useAuthContext();
  const isAuthenticated = !!user;
  const hubState = useHubState(notificationHub, isAuthenticated);

  const [notificationMessage, setNotificationMessage] = useState<
    undefined | string
  >(undefined);

  const { hub, status } = hubState || {};

  useEffect(() => {
    if (!hub || status !== 'Connected') {
      return;
    }
    const subscription = hub.subscribe(
      'ServiceAnnouncement',
      (message: string) => {
        setNotificationMessage(message);
      },
    );

    // eslint-disable-next-line consistent-return
    return () => {
      subscription.unsubscribe();
    };
  }, [hub, status]);

  if (!isAuthenticated) {
    return (
      <NotificationHubContext.Provider value={undefined}>
        {typeof children === 'function' ? children(undefined) : children}
      </NotificationHubContext.Provider>
    );
  }

  if (!hubState) {
    return <LoadingSpinner />;
  }

  return (
    <NotificationHubContext.Provider value={hubState}>
      <Modal
        title="Service announcement"
        showClose
        onExit={() => setNotificationMessage('')}
        open={!!notificationMessage}
      >
        <InsetText>
          <FormattedDate
            className="govuk-!-font-weight-bold"
            format="d MMM yyyy, HH:mm"
          >
            {new Date()}
          </FormattedDate>
          <p>{notificationMessage}</p>
        </InsetText>
      </Modal>
      {typeof children === 'function' ? children(hubState) : children}
    </NotificationHubContext.Provider>
  );
}

export function useNotificationHubContext(): HubState<NotificationHub> {
  const context = useContext(NotificationHubContext);

  if (!context) {
    throw new Error(
      `Must have a parent ${NotificationHubContextProvider.name}`,
    );
  }

  return context;
}
