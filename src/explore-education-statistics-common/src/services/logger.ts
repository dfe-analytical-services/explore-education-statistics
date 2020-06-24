/* eslint-disable no-console */
import { getApplicationInsights } from '@common/services/applicationInsightsService';
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
  info(message: string) {
    console.info(message);

    getApplicationInsights().trackTrace({
      message,
      severityLevel: SeverityLevel.Information,
    });
  },
  warn(message: string) {
    console.warn(message);

    getApplicationInsights().trackTrace({
      message,
      severityLevel: SeverityLevel.Warning,
    });
  },
  error(error: Error) {
    console.error(error);

    getApplicationInsights().trackException({
      exception: error,
    });
  },
};

export default logger;
