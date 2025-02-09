import ForbiddenPage from '@admin/pages/errors/ForbiddenPage';
import ResourceNotFoundPage from '@admin/pages/errors/ResourceNotFoundPage';
import ServiceProblemsPage from '@admin/pages/errors/ServiceProblemsPage';
import { ErrorControlContextProvider } from '@common/contexts/ErrorControlContext';
import logger from '@common/services/logger';
import { isAxiosError } from 'axios';
import React, { Component, ReactNode } from 'react';
import { RouteComponentProps, withRouter } from 'react-router';

interface State {
  errorCode?: number;
}

interface Props extends RouteComponentProps {
  children: ReactNode;
}

/**
 * This component is responsible for rendering error pages of
 * specific types, or a fallback "Service problems" page
 * dependant on the type of error encountered.
 */
class PageErrorBoundary extends Component<Props, State> {
  public state: State = {};

  private unregisterCallback?: () => void;

  private errorPages = {
    forbidden: () => {
      this.setState({
        errorCode: 403,
      });
    },
  };

  public componentDidMount() {
    const { history } = this.props;

    this.unregisterCallback = history.listen(() => {
      this.setState({
        errorCode: undefined,
      });
    });

    window.addEventListener('unhandledrejection', this.handlePromiseRejections);
  }

  public componentDidCatch(error: Error) {
    logger.error(error);

    this.setState({
      errorCode: 500,
    });
  }

  public componentWillUnmount() {
    if (this.unregisterCallback) {
      this.unregisterCallback();
    }

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

export default withRouter(PageErrorBoundary);
