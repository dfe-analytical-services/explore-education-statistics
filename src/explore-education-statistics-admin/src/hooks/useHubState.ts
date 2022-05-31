import Hub from '@admin/services/hubs/utils/Hub';
import useMountedRef from '@common/hooks/useMountedRef';
import { HubConnectionState } from '@microsoft/signalr';
import { useEffect, useState } from 'react';

export interface HubState<THub extends Hub> {
  hub: THub;
  status: HubConnectionState;
}

/**
 * Hook integrating a {@see Hub} class with React.
 */
export default function useHubState<THub extends Hub = Hub>(
  factory: () => THub,
): HubState<THub> {
  const isMountedRef = useMountedRef();
  const [hubState, setHubState] = useState<HubState<THub>>(() => {
    const hub = factory();

    return {
      hub,
      status: hub.status(),
    };
  });

  const { hub } = hubState;

  useEffect(() => {
    const updateHub = async () => {
      const status = hub.status();

      if (isMountedRef.current) {
        // Re-set the hub in state to avoid stale state
        // issues with the underlying hub connection.
        setHubState({ status, hub });

        return;
      }

      // Hub may only finish connecting after the component
      // has unmounted, so we should call `stop` here to
      // disconnect the hub as well.
      // We do this to keep the number of open connections
      // to a minimum to prevent over-saturating server.
      await hub.stop();
    };

    hub.start().then(updateHub);

    hub.onReconnected(updateHub);
    hub.onReconnecting(updateHub);
    hub.onDisconnect(updateHub);

    return () => {
      hub.stop();
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return hubState;
}
