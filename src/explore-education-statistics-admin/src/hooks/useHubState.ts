import Hub from '@admin/services/hubs/utils/Hub';
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
  hub: THub,
): HubState<THub> {
  const [hubState, setHubState] = useState<HubState<THub>>({
    hub,
    status: hub.status(),
  });

  useEffect(() => {
    // Set a new copy of the hub to avoid stale state
    const updateHub = () =>
      setHubState({
        status: hub.status(),
        hub,
      });

    hub.start().then(updateHub);

    hub.onReconnected(updateHub);
    hub.onReconnecting(updateHub);
    hub.onDisconnect(updateHub);

    return () => {
      hub?.stop();
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return hubState;
}
