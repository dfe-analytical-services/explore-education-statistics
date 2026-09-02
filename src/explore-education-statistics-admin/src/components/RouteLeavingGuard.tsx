import ModalConfirm from '@common/components/ModalConfirm';
import React, { ReactNode, useCallback, useEffect } from 'react';
import { BlockerFunction, useBlocker } from 'react-router';

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
  const blockerFunction = useCallback<BlockerFunction>(
    ({ currentLocation, nextLocation }) => {
      return (
        blockRouteChange && currentLocation.pathname !== nextLocation.pathname
      );
    },
    [blockRouteChange],
  );

  const blocker = useBlocker(blockerFunction);

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

  return (
    <ModalConfirm
      title={title}
      open={blocker.state === 'blocked'}
      onConfirm={() => blocker.proceed?.()}
      onExit={() => blocker.reset?.()}
      onCancel={() => blocker.reset?.()}
    >
      {children}
    </ModalConfirm>
  );
};

export default RouteLeavingGuard;
