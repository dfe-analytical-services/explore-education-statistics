import ForbiddenPage from '@admin/pages/errors/ForbiddenPage';
import ResourceNotFoundPage from '@admin/pages/errors/ResourceNotFoundPage';
import ServiceProblemsPage from '@admin/pages/errors/ServiceProblemsPage';
import { clients } from '@admin/services/util/configureAxios';
import { ErrorControlContextProvider } from '@common/contexts/ErrorControlContext';
import { AxiosError } from 'axios';
import * as H from 'history';
import React from 'react';
import { RouteComponentProps, withRouter } from 'react-router';

interface State {
  errorCode?: number;
}

/**
 * This component is responsible for rendering error pages of
 * specific types, or a fallback "Service problems" page
 * dependant on the type of error encountered.
 */
class ErrorBoundary extends React.Component<RouteComponentProps, State> {
  public state: State = {};

  private unregisterCallback?: H.UnregisterCallback;

  private isHandlingErrors = true;

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

    clients.forEach(client => {
      // eslint-disable-next-line no-param-reassign
      client.errorHandler = this.handleApiErrors;
    });
  }

  public componentWillUnmount() {
    if (this.unregisterCallback) {
      this.unregisterCallback();
    }
  }

  private handleApiErrors = (error: AxiosError) => {
    if (this.isHandlingErrors) {
      this.setState({
        errorCode: error.response?.status || 500,
      });
    }
  };

  private withoutErrorHandling = async (callback: () => void) => {
    this.isHandlingErrors = false;

    try {
      await callback();
    } finally {
      this.isHandlingErrors = true;
    }
  };

  public componentDidCatch() {
    this.setState({
      errorCode: 500,
    });
  }

  public render() {
    const { handleApiErrors, handleManualErrors, withoutErrorHandling } = this;
    const { children } = this.props;
    const { errorCode } = this.state;

    if (!errorCode) {
      return (
        <ErrorControlContextProvider
          value={{
            handleApiErrors,
            handleManualErrors,
            withoutErrorHandling,
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

export default withRouter(ErrorBoundary);
