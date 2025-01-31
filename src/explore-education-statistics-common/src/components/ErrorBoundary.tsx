import logger from '@common/services/logger';
import { Component, ErrorInfo, ReactNode } from 'react';

interface Props {
  children: ReactNode;
  fallback: ((error: Error) => ReactNode) | ReactNode;
  onError?: (error: Error, errorInfo: ErrorInfo) => void;
}

interface State {
  error: Error | null;
}

/**
 * Generic error boundary component. Use {@see Props.fallback}
 * to render some replacement UI if the children
 * throws an error whilst rendering.
 */
class ErrorBoundary extends Component<Props, State> {
  state: State = {
    error: null,
  };

  static getDerivedStateFromError(error: Error): State {
    return {
      error,
    };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo): void {
    const { onError } = this.props;

    logger.error(error);

    if (onError) {
      onError(error, errorInfo);
    }
  }

  render() {
    const { error } = this.state;
    const { children, fallback } = this.props;

    if (error) {
      return typeof fallback === 'function' ? fallback(error) : fallback;
    }

    return children;
  }
}

export default ErrorBoundary;
