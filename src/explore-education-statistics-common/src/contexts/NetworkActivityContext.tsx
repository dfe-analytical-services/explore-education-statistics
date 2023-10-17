import useMountedRef from '@common/hooks/useMountedRef';
import {
  RequestInterceptor,
  ResponseInterceptor,
} from '@common/services/api/Client';
import React, {
  createContext,
  ReactNode,
  SetStateAction,
  useCallback,
  useContext,
  useEffect,
  useState,
} from 'react';

const EVENT_NETWORK_REQUEST = 'ees:network_request';
const EVENT_NETWORK_REQUEST_ERROR = 'ees:network_request_error';
const EVENT_NETWORK_RESPONSE = 'ees:network_response';
const EVENT_NETWORK_RESPONSE_ERROR = 'ees:network_response_error';

const dataAttribute = 'data-network-activity';

export type NetworkActivityStatus = 'active' | 'idle';

export interface NetworkActivityState {
  status: NetworkActivityStatus;
  requestCount: number;
}

const NetworkActivityContext = createContext<NetworkActivityState | undefined>(
  undefined,
);

export interface NetworkActivityContextProviderProps {
  children?: ReactNode;
  idleTimeout?: number;
}

/**
 * Provider that listens for network activity events.
 *
 * Upon receiving these events, the provider will update the `data-network-activity`
 * attribute on the document body to notify other listeners (outside of React) on
 * the current state.
 *
 * This is primarily used by Robot Framework tests to identify when the network is
 * idle and the page can be considered to be 'loaded' before proceeding with assertions.
 *
 * This provider works by using client interceptors to hook into the request/response
 * lifecycle. These dispatch custom DOM events that we can listen for in the provider.
 */
export function NetworkActivityContextProvider({
  children,
  idleTimeout = 500,
}: NetworkActivityContextProviderProps) {
  const isMountedRef = useMountedRef();
  const [state, _setState] = useState<NetworkActivityState>({
    status: 'idle',
    requestCount: 0,
  });

  const setState = useCallback(
    (setter: SetStateAction<NetworkActivityState>) => {
      if (!isMountedRef.current) {
        return;
      }

      _setState(prevState => {
        const nextState =
          typeof setter === 'function' ? setter(prevState) : setter;

        document.body.setAttribute(dataAttribute, nextState.status);

        return nextState;
      });
    },
    [isMountedRef],
  );

  useEffect(() => {
    if (document.body.hasAttribute(dataAttribute)) {
      throw new Error(
        `Cannot have more than once instance of ${NetworkActivityContextProvider.name}`,
      );
    }

    document.body.setAttribute(dataAttribute, 'idle');

    return () => {
      document.body.removeAttribute(dataAttribute);
    };
  }, []);

  useEffect(() => {
    const handleRequest = () => {
      setState(prevState => {
        return {
          status: 'active',
          requestCount: prevState.requestCount + 1,
        };
      });
    };

    const handleResponse = () => {
      setState(prevState => ({
        ...prevState,
        requestCount: prevState.requestCount - 1,
      }));

      setTimeout(() => {
        setState(prevState => {
          return {
            ...prevState,
            status: prevState.requestCount === 0 ? 'idle' : 'active',
          };
        });
      }, idleTimeout);
    };

    window.addEventListener(EVENT_NETWORK_REQUEST, handleRequest);
    window.addEventListener(EVENT_NETWORK_REQUEST_ERROR, handleResponse);
    window.addEventListener(EVENT_NETWORK_RESPONSE, handleResponse);
    window.addEventListener(EVENT_NETWORK_RESPONSE_ERROR, handleResponse);

    return () => {
      window.removeEventListener(EVENT_NETWORK_REQUEST, handleRequest);
      window.removeEventListener(EVENT_NETWORK_REQUEST_ERROR, handleResponse);
      window.removeEventListener(EVENT_NETWORK_RESPONSE, handleResponse);
      window.removeEventListener(EVENT_NETWORK_RESPONSE_ERROR, handleResponse);
    };
  }, [idleTimeout, setState]);

  return (
    <NetworkActivityContext.Provider value={state}>
      {children}
    </NetworkActivityContext.Provider>
  );
}

export function useNetworkActivityContext(): NetworkActivityState {
  const context = useContext(NetworkActivityContext);

  if (!context) {
    throw new Error(
      `Must be used within a ${NetworkActivityContextProvider.name}`,
    );
  }

  return context;
}

export const networkActivityRequestInterceptor: RequestInterceptor = {
  onRequest: config => {
    if (typeof window !== 'undefined') {
      window.dispatchEvent(new CustomEvent(EVENT_NETWORK_REQUEST));
    }

    return config;
  },
  onError: error => {
    if (typeof window !== 'undefined') {
      window.dispatchEvent(new CustomEvent(EVENT_NETWORK_REQUEST_ERROR));
    }

    return Promise.reject(error);
  },
};

export const networkActivityResponseInterceptor: ResponseInterceptor = {
  onResponse: response => {
    if (typeof window !== 'undefined') {
      window.dispatchEvent(new CustomEvent(EVENT_NETWORK_RESPONSE));
    }

    return response;
  },
  onError: error => {
    if (typeof window !== 'undefined') {
      window.dispatchEvent(new CustomEvent(EVENT_NETWORK_RESPONSE_ERROR));
    }

    return Promise.reject(error);
  },
};
