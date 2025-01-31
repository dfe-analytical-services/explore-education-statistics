/* eslint-disable no-console */
import { getApplicationInsights } from '@common/services/applicationInsightsService';
import isErrorLike from '@common/utils/error/isErrorLike';
import { SeverityLevel } from '@microsoft/applicationinsights-web';

const logger = {
  debugGroup(name: string, ...args: unknown[]) {
    if (process.env.NODE_ENV === 'development') {
      console.group(name, ...args);
    }
  },
  debugGroupEnd() {
    if (process.env.NODE_ENV === 'development') {
      console.groupEnd();
    }
  },
  debug(...args: unknown[]) {
    if (process.env.NODE_ENV === 'development') {
      console.log(...args);
    }
  },
  debugTime(...args: unknown[]) {
    if (process.env.NODE_ENV === 'development') {
      console.log(`[${new Date().toISOString()}]`, ...args);
    }
  },
  info(message: string) {
    if (process.env.NODE_ENV === 'test') {
      return;
    }

    console.info(message);

    getApplicationInsights().trackTrace({
      message,
      severityLevel: SeverityLevel.Information,
    });
  },
  warn(message: string) {
    if (process.env.NODE_ENV === 'test') {
      return;
    }

    console.warn(message);

    getApplicationInsights().trackTrace({
      message,
      severityLevel: SeverityLevel.Warning,
    });
  },
  error(error: unknown) {
    if (process.env.NODE_ENV === 'test') {
      return;
    }

    console.error(error);

    if (isErrorLike(error)) {
      getApplicationInsights().trackException({
        exception: error,
      });
    }
  },
};

export default logger;
