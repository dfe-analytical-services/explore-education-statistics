import ForbiddenPage from '@admin/pages/errors/ForbiddenPage';
import ResourceNotFoundPage from '@admin/pages/errors/ResourceNotFoundPage';
import ServiceProblemsPage from '@admin/pages/errors/ServiceProblemsPage';
import { ErrorControlContextProvider } from '@common/contexts/ErrorControlContext';
import logger from '@common/services/logger';
import { AxiosError } from 'axios';
import * as H from 'history';
import React, { Component } from 'react';
import { RouteComponentProps, withRouter } from 'react-router';

interface State {
  errorCode?: number;
}

/**
 * This component is responsible for rendering error pages of
 * specific types, or a fallback "Service problems" page
 * dependant on the type of error encountered.
 */
class PageErrorBoundary extends Component<RouteComponentProps, State> {
  public state: State = {};

  private unregisterCallback?: H.UnregisterCallback;

  private handleManualErrors = {
    forbidden: () => {
      this.setState({
        errorCode: 403,
      });
    },
  };

  public constructor(props: RouteComponentProps) {
    super(props);
  }

  public componentDidMount() {
    const { history } = this.props;
    this.unregisterCallback = history.listen(() => {
      this.setState({
        errorCode: undefined,
      });
    });

    window.addEventListener('unhandledrejection', this.handlePromiseRejections);
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
    this.handleApiErrors(event.reason);
  };

  private handleApiErrors = (error: AxiosError) => {
    this.setState({
      errorCode: error.response?.status || 500,
    });
  };

  public componentDidCatch(error: Error) {
    logger.error(error);

    this.setState({
      errorCode: 500,
    });
  }

  public render() {
    const { handleApiErrors, handleManualErrors } = this;
    const { children } = this.props;
    const { errorCode } = this.state;

    if (!errorCode) {
      return (
        <ErrorControlContextProvider
          value={{
            handleApiErrors,
            handleManualErrors,
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
