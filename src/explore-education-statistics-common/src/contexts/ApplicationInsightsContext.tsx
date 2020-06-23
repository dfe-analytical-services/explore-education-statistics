import { ApplicationInsights } from '@microsoft/applicationinsights-web';
import React, {
  createContext,
  ReactNode,
  useContext,
  useEffect,
  useState,
} from 'react';

const ApplicationInsightsContext = createContext<
  ApplicationInsights | undefined
>(undefined);

interface ApplicationInsightsContextProviderProps {
  children: ReactNode;
  instrumentationKey: string | Promise<string>;
}

export const ApplicationInsightsContextProvider = ({
  children,
  instrumentationKey,
}: ApplicationInsightsContextProviderProps) => {
  const [isLoading, setLoading] = useState(true);
  const [appInsights, setAppInsights] = useState<ApplicationInsights>();

  useEffect(() => {
    import('@common/services/applicationInsightsService').then(
      async ({ initApplicationInsights }) => {
        const applicationInsights = initApplicationInsights(
          await instrumentationKey,
        );

        setAppInsights(applicationInsights);
        setLoading(false);
      },
    );
  }, [instrumentationKey]);

  return (
    <ApplicationInsightsContext.Provider value={appInsights}>
      {!isLoading ? children : null}
    </ApplicationInsightsContext.Provider>
  );
};

export function useApplicationInsights(): ApplicationInsights {
  const appInsights = useContext(ApplicationInsightsContext);

  if (!appInsights) {
    throw new Error(
      'ApplicationInsightsContextProvider has not been initialised',
    );
  }

  return appInsights;
}
