import useHubState, { HubState } from '@admin/hooks/useHubState';
import notificationHub, {
  NotificationHub,
} from '@admin/services/hubs/notificationHub';
import FormattedDate from '@common/components/FormattedDate';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Modal from '@common/components/Modal';
import useMountedRef from '@common/hooks/useMountedRef';
import React, {
  createContext,
  ReactNode,
  useContext,
  useEffect,
  useRef,
  useState,
} from 'react';

const NotificationHubContext = createContext<
  HubState<NotificationHub> | undefined
>(undefined);

interface NotificationHubContextProviderProps {
  children: ReactNode | ((value: HubState<NotificationHub>) => ReactNode);
}

export function NotificationHubContextProvider({
  children,
}: NotificationHubContextProviderProps) {
  const hubState = useHubState(notificationHub);

  const [showNotificationModal, setShowNotificationModal] =
    useState<boolean>(false);
  const [notificationSenderName, setNotificationSenderName] =
    useState<string>();
  const [notificationMessage, setNotificationMessage] = useState<
    undefined | string
  >(undefined);

  const joinStateRef = useRef<'joining' | ''>('');
  const isMountedRef = useMountedRef();

  const { hub, status } = hubState;

  useEffect(() => {
    if (joinStateRef.current === '' && status === 'Connected') {
      joinStateRef.current = 'joining';

      hub.subscribe(
        'ServiceAnnouncement',
        (senderName: string, message: string) => {
          setNotificationSenderName(senderName);
          setNotificationMessage(message);
          setShowNotificationModal(true);
        },
      );
    }
  }, [hub, hubState, isMountedRef, status]);

  if (!hubState) {
    return <LoadingSpinner />;
  }

  return (
    <NotificationHubContext.Provider value={hubState}>
      <Modal
        title="Service announcement"
        showClose
        onExit={() => setShowNotificationModal(false)}
        open={showNotificationModal}
      >
        <p>{notificationSenderName} has sent the following message:</p>
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
