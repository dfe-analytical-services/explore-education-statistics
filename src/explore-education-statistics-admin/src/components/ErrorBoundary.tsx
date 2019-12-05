import ForbiddenPage from '@admin/pages/errors/ForbiddenPage';
import ResourceNotFoundPage from '@admin/pages/errors/ResourceNotFoundPage';
import ServiceProblemsPage from '@admin/pages/errors/ServiceProblemsPage';
import * as H from 'history';
import React, { createContext } from 'react';
import { RouteComponentProps, withRouter } from 'react-router';

interface ErrorControl {
  setErrorCode: (errorCode: number) => void;
}

export const ErrorControlContext = createContext<ErrorControl>({
  setErrorCode: () => {},
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
    const setErrorCode = (errorCode: number) => {
      this.setState({
        errorCode,
      });
    };

    const { children } = this.props;
    const { errorCode } = this.state;

    if (!errorCode) {
      return (
        <ErrorControlContext.Provider value={{ setErrorCode }}>
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
