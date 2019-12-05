import { ErrorControlContext } from '@admin/components/ErrorBoundary';
import { AxiosResponse } from 'axios';
import React from 'react';

const withErrorControl = <P extends object>(
  Component: React.ComponentType<P>,
) => {
  // eslint-disable-next-line react/display-name
  return (props: P) => {
    return (
      <ErrorControlContext.Consumer>
        {({ setErrorCode }) => {
          return (
            <Component
              {...props}
              apiErrorFallbackHandler={(error: AxiosResponse) =>
                setErrorCode(error.status)
              }
            />
          );
        }}
      </ErrorControlContext.Consumer>
    );
  };
};

export default withErrorControl;
