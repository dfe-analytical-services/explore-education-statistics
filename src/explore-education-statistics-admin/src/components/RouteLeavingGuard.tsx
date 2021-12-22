import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';
import React, { useEffect, useState } from 'react';
import { Prompt, useLocation } from 'react-router';
import { useHistory } from 'react-router-dom';

interface Props {
  blockRouteChange: boolean;
}

const RouteLeavingGuard = ({ blockRouteChange = false }: Props) => {
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
        title="There are unsaved changes"
        open={showModal}
        onConfirm={() => {
          toggleShowModal.off();
          toggleConfirmedNavigation.on();
        }}
        onExit={toggleShowModal.off}
        onCancel={toggleShowModal.off}
      >
        <p>
          Clicking away from this tab will result in the changes being lost.
        </p>
      </ModalConfirm>
    </>
  );
};

export default RouteLeavingGuard;
