import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';
import React, { ReactNode, useEffect, useState } from 'react';
import { Prompt, useLocation } from 'react-router';
import { useHistory } from 'react-router-dom';

interface Props {
  blockRouteChange: boolean;
  children: ReactNode;
  title: string;
}

const RouteLeavingGuard = ({
  blockRouteChange = false,
  children,
  title,
}: Props) => {
  const location = useLocation();
  const history = useHistory();
  const [lastLocation, setLastLocation] = useState(location);
  const [showModal, toggleShowModal] = useToggle(false);
  const [confirmedNavigation, toggleConfirmedNavigation] = useToggle(false);

  // Block non-react routes
  useEffect(() => {
    const handleBeforeUnload = (event: BeforeUnloadEvent) => {
      if (blockRouteChange) {
        event.preventDefault();
        // eslint-disable-next-line no-param-reassign
        event.returnValue = '';
      }
    };

    window.addEventListener('beforeunload', handleBeforeUnload);

    return () => window.removeEventListener('beforeunload', handleBeforeUnload);
  }, [blockRouteChange]);

  // Block react routes
  useEffect(() => {
    if (confirmedNavigation && lastLocation) {
      toggleConfirmedNavigation.off();
      history.push(lastLocation.pathname);
    }
  }, [confirmedNavigation, lastLocation, history, toggleConfirmedNavigation]);

  return (
    <>
      <Prompt
        when={blockRouteChange}
        message={nextLocation => {
          if (!confirmedNavigation && blockRouteChange) {
            setLastLocation(nextLocation);
            toggleShowModal.on();
            return false;
          }
          return true;
        }}
      />
      <ModalConfirm
        title={title}
        open={showModal}
        onConfirm={() => {
          toggleShowModal.off();
          toggleConfirmedNavigation.on();
        }}
        onExit={toggleShowModal.off}
        onCancel={toggleShowModal.off}
      >
        {children}
      </ModalConfirm>
    </>
  );
};

export default RouteLeavingGuard;
