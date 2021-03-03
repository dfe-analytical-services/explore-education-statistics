import {
  ErrorControlState,
  useErrorControl,
} from '@common/contexts/ErrorControlContext';
import React, { ComponentType, ReactElement } from 'react';

/**
 * This function creates a higher order component of the provided Component,
 * that supplies it with the `handleApiErrors` function for ease of signalling
 * errors back to the ErrorBoundary surrounding the component tree and
 * resulting in an error page.
 *
 * Therefore this HOC function expects to be passed Components that expect
 * props from  ErrorControlProps and optionally others as well, but returns
 * a function that expects the caller to provide all other props OTHER THAN
 * the ones from ErrorControlProps - the reason being that it is the job
 * of this HOC function to provide the props in ErrorControlProps, not the
 * code that includes the wrapped Component as a child.
 *
 * @param Component
 */
function withErrorControl<P extends ErrorControlState>(
  Component: ComponentType<P>,
): (props: Omit<P, keyof ErrorControlState>) => ReactElement {
  const WrappedComponent = (props: Omit<P, keyof ErrorControlState>) => {
    const { handleError, errorPages } = useErrorControl();

    return (
      <Component
        {...(props as P)}
        handleError={handleError}
        errorPages={errorPages}
      />
    );
  };

  WrappedComponent.displayName = `withErrorControl(${
    Component.displayName ?? Component.name
  })`;

  return WrappedComponent;
}

export default withErrorControl;
