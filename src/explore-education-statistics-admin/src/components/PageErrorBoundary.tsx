import ForbiddenPage from '@admin/pages/errors/ForbiddenPage';
import ResourceNotFoundPage from '@admin/pages/errors/ResourceNotFoundPage';
import ServiceProblemsPage from '@admin/pages/errors/ServiceProblemsPage';
import { ErrorControlContextProvider } from '@common/contexts/ErrorControlContext';
import logger from '@common/services/logger';
import isAxiosError from '@common/utils/error/isAxiosError';
import React, { ReactNode, useEffect, useState } from 'react';
import { useHistory } from 'react-router';

interface Props {
  children: ReactNode;
}

/**
 * This component is responsible for rendering error pages of
 * specific types, or a fallback "Service problems" page
 * dependant on the type of error encountered.
 */
const PageErrorBoundary = ({ children }: Props) => {
  const [errorCode, setErrorCode] = useState<number | undefined>(undefined);
  const history = useHistory();

  useEffect(() => {
    const unregisterCallback = history.listen(() => {
      setErrorCode(undefined);
    });

    window.addEventListener('unhandledrejection', handlePromiseRejections);

    return () => {
      unregisterCallback();
      window.removeEventListener('unhandledrejection', handlePromiseRejections);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const errorPages = {
    forbidden: () => {
      setErrorCode(403);
    },
  };

  const handlePromiseRejections = (event: PromiseRejectionEvent) => {
    handleError(event.reason);
  };

  const handleError = (error: unknown) => {
    logger.error(error);

    setErrorCode(isAxiosError(error) ? error.response?.status : 500);
  };

  if (!errorCode) {
    return (
      <ErrorControlContextProvider
        value={{
          handleError,
          errorPages,
        }}
      >
        {children}
      </ErrorControlContextProvider>
    );
  }

  if (errorCode === 401 || errorCode === 403) {
    return <ForbiddenPage />;
  }

  if (errorCode === 404) {
    return <ResourceNotFoundPage />;
  }

  return <ServiceProblemsPage />;
};

export default PageErrorBoundary;
