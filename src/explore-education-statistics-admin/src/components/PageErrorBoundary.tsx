import ForbiddenPage from '@admin/pages/errors/ForbiddenPage';
import ResourceNotFoundPage from '@admin/pages/errors/ResourceNotFoundPage';
import ServiceProblemsPage from '@admin/pages/errors/ServiceProblemsPage';
import { ErrorControlContextProvider } from '@common/contexts/ErrorControlContext';
import logger from '@common/services/logger';
import { isAxiosError } from 'axios';
import React, { Component, ReactNode } from 'react';
import { Location, useLocation } from 'react-router';

interface State {
  errorCode?: number;
}

interface Props {
  children: ReactNode;
  location: Location;
}

/**
 * This component is responsible for rendering error pages of
 * specific types, or a fallback "Service problems" page
 * dependent on the type of error encountered.
 */
class PageErrorBoundary extends Component<Props, State> {
  public state: State = {};

  private errorPages = {
    forbidden: () => {
      this.setState({
        errorCode: 403,
      });
    },
  };

  public componentDidMount() {
    window.addEventListener('unhandledrejection', this.handlePromiseRejections);
  }

  public componentDidUpdate(prevProps: Props) {
    const { location } = this.props;

    if (location.key !== prevProps.location.key) {
      this.setState({
        errorCode: undefined,
      });
    }
  }

  public componentDidCatch(error: Error) {
    logger.error(error);

    // errors can now come via react-query and not through "unhandledrejection"
    this.setState({
      errorCode: (isAxiosError(error) && error.response?.status) || 500,
    });
  }

  public componentWillUnmount() {
    window.removeEventListener(
      'unhandledrejection',
      this.handlePromiseRejections,
    );
  }

  private handlePromiseRejections = (event: PromiseRejectionEvent) => {
    this.handleError(event.reason);
  };

  private handleError = (error: unknown) => {
    logger.error(error);

    this.setState({
      errorCode: isAxiosError(error) ? error.response?.status : 500,
    });
  };

  public render() {
    const { handleError, errorPages } = this;
    const { children } = this.props;
    const { errorCode } = this.state;

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
  }
}

function PageErrorBoundaryWithRouter({ children }: { children: ReactNode }) {
  const location = useLocation();

  return <PageErrorBoundary location={location}>{children}</PageErrorBoundary>;
}

export default PageErrorBoundaryWithRouter;
