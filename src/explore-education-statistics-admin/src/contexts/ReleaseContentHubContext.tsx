import useHubState, { HubState } from '@admin/hooks/useHubState';
import releaseContentHub, {
  ReleaseContentHub,
} from '@admin/services/hubs/releaseContentHub';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useMountedRef from '@common/hooks/useMountedRef';
import React, {
  createContext,
  ReactNode,
  useContext,
  useEffect,
  useRef,
} from 'react';

const ReleaseContentHubContext = createContext<
  HubState<ReleaseContentHub> | undefined
>(undefined);

interface ReleaseContentHubContextProviderProps {
  children: ReactNode | ((value: HubState<ReleaseContentHub>) => ReactNode);
  releaseId: string;
}

export function ReleaseContentHubContextProvider({
  children,
  releaseId,
}: ReleaseContentHubContextProviderProps) {
  const hubState = useHubState(releaseContentHub);

  const joinStateRef = useRef<'joining' | 'joined' | ''>('');
  const isMountedRef = useMountedRef();

  const { hub, status } = hubState;

  useEffect(() => {
    if (joinStateRef.current === '' && status === 'Connected') {
      joinStateRef.current = 'joining';

      hub
        .joinReleaseGroup(releaseId)
        .then(() => {
          joinStateRef.current = 'joined';
        })
        .catch(() => {
          joinStateRef.current = '';
        });
    }

    return () => {
      if (status === 'Connected') {
        hub.leaveReleaseGroup(releaseId);
      }
    };
  }, [hub, hubState, isMountedRef, releaseId, status]);

  if (!hubState) {
    return <LoadingSpinner />;
  }

  return (
    <ReleaseContentHubContext.Provider value={hubState}>
      {typeof children === 'function' ? children(hubState) : children}
    </ReleaseContentHubContext.Provider>
  );
}

export function useReleaseContentHubContext(): HubState<ReleaseContentHub> {
  const context = useContext(ReleaseContentHubContext);

  if (!context) {
    throw new Error(
      `Must have a parent ${ReleaseContentHubContextProvider.name}`,
    );
  }

  return context;
}
