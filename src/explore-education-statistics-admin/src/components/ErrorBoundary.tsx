import ForbiddenPage from '@admin/pages/errors/ForbiddenPage';
import ResourceNotFoundPage from '@admin/pages/errors/ResourceNotFoundPage';
import ServiceProblemsPage from '@admin/pages/errors/ServiceProblemsPage';
import {
  ApiErrorHandler,
  ManualErrorHandler,
} from '@admin/validation/withErrorControl';
import { AxiosResponse } from 'axios';
import * as H from 'history';
import React, { createContext } from 'react';
import { RouteComponentProps, withRouter } from 'react-router';

interface ErrorControl {
  handleApiErrors: ApiErrorHandler;
  handleManualErrors: ManualErrorHandler;
}

export const ErrorControlContext = createContext<ErrorControl>({
  handleApiErrors: _ => {},
  handleManualErrors: {
    forbidden: () => {},
  },
});

interface State {
  errorCode?: number;
}

/**
 * This Component is responsible for rendering error pages of specific types (or a fallback "Service problems" page
 * dependant on the type of error encountered.
 *
 * This Component provides a Context which allows child components to use a "handleApiErrors" callback to signal errors
 * back to this page.
 */
class ErrorBoundary extends React.Component<RouteComponentProps, State> {
  public state: State = {};

  private unregisterCallback?: H.UnregisterCallback;

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
  }

  public componentWillUnmount() {
    if (this.unregisterCallback) {
      this.unregisterCallback();
    }
  }

  public componentDidCatch() {
    this.setState({
      errorCode: 500,
    });
  }

  public render() {
    const handleApiErrors = (error: AxiosResponse) => {
      this.setState({
        errorCode: error.status || 500,
      });
      throw error;
    };

    const handleManualErrors = {
      forbidden: () => {
        this.setState({
          errorCode: 403,
        });
      },
    };

    const { children } = this.props;
    const { errorCode } = this.state;

    if (!errorCode) {
      return (
        <ErrorControlContext.Provider
          value={{ handleApiErrors, handleManualErrors }}
        >
          {children}
        </ErrorControlContext.Provider>
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
