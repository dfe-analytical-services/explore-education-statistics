import ForbiddenPage from '@admin/pages/errors/ForbiddenPage';
import ResourceNotFoundPage from '@admin/pages/errors/ResourceNotFoundPage';
import ServiceProblemsPage from '@admin/pages/errors/ServiceProblemsPage';
import { ApiErrorHandler } from '@admin/validation/withErrorControl';
import { AxiosResponse } from 'axios';
import * as H from 'history';
import React, { createContext } from 'react';
import { RouteComponentProps, withRouter } from 'react-router';

interface ErrorControl {
  handleApiErrors: ApiErrorHandler;
}

export const ErrorControlContext = createContext<ErrorControl>({
  handleApiErrors: _ => {},
});

interface State {
  errorCode?: number;
}

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

    const { children } = this.props;
    const { errorCode } = this.state;

    if (!errorCode) {
      return (
        <ErrorControlContext.Provider value={{ handleApiErrors }}>
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
