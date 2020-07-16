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
  const [appInsights, setAppInsights] = useState<ApplicationInsights>();

  useEffect(() => {
    import('@common/services/applicationInsightsService').then(
      async ({ initApplicationInsights }) => {
        const applicationInsights = initApplicationInsights(
          await instrumentationKey,
        );

        setAppInsights(applicationInsights);
      },
    );
  }, [instrumentationKey]);

  return (
    <ApplicationInsightsContext.Provider value={appInsights}>
      {children}
    </ApplicationInsightsContext.Provider>
  );
};

export function useApplicationInsights(): ApplicationInsights | undefined {
  return useContext(ApplicationInsightsContext);
}
