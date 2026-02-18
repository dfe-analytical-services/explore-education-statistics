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
  enabled: boolean = true,
): HubState<THub> | undefined {
  const isMountedRef = useMountedRef();
  const [hubState, setHubState] = useState<HubState<THub> | undefined>(() => {
    if (!enabled) return undefined;
    const hub = factory();

    return {
      hub,
      status: hub.status(),
    };
  });

  useEffect(() => {
    if (!enabled) return;

    let currentHub = hubState?.hub;
    if (!currentHub) {
      currentHub = factory();
      setHubState({ hub: currentHub, status: currentHub.status() });
    }

    const updateHub = async () => {
      if (!currentHub) return;
      const status = currentHub.status();

      if (isMountedRef.current) {
        setHubState({ status, hub: currentHub });
        return;
      }
      await currentHub.stop();
    };

    currentHub.start().then(updateHub);
    currentHub.onReconnected(updateHub);
    currentHub.onReconnecting(updateHub);
    currentHub.onDisconnect(updateHub);

    // eslint-disable-next-line consistent-return
    return () => {
      currentHub?.stop();
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [enabled]);

  return hubState;
}
