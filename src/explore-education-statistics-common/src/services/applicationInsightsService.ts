import {
  ApplicationInsights,
  ITelemetryItem,
  SeverityLevel,
} from '@microsoft/applicationinsights-web';

export { ApplicationInsights, SeverityLevel };

let isInitialised = false;

const appInsights = new ApplicationInsights({
  config: {
    autoTrackPageVisitTime: true,
  },
});

export function initApplicationInsights(
  instrumentationKey: string,
): ApplicationInsights {
  if (instrumentationKey) {
    appInsights.config.instrumentationKey = instrumentationKey;

    if (!isInitialised) {
      appInsights.loadAppInsights();
      appInsights.addTelemetryInitializer(filterSensitiveData);

      isInitialised = true;

      // eslint-disable-next-line no-console
      console.log('Application Insights initialised');
    }
  }

  return appInsights;
}

/**
 * Escape hatch to get the Application Insights client
 * directly. Prefer to use context if possible.
 */
export function getApplicationInsights(): ApplicationInsights {
  return appInsights;
}

const REDACTED_VALUE = '__redacted__';
const REDACTED_TOKENS = [
  'code',
  'email',
  'phone',
  'token',
  'passphrase',
  'password',
  'pass',
  'user',
  'name',
  'username',
  'firstname',
  'lastname',
  'fullname',
];

/* eslint-disable no-param-reassign */
export function filterSensitiveData(telemetry: ITelemetryItem): boolean {
  switch (telemetry.baseType) {
    case 'RemoteDependencyData':
      if (telemetry.baseData?.name) {
        telemetry.baseData.name = filterUri(telemetry.baseData.name);
      }

      if (telemetry.baseData?.target) {
        telemetry.baseData.target = filterUri(telemetry.baseData.target);
      }

      break;

    case 'MetricData':
      if (telemetry.data?.PageUrl) {
        telemetry.data.PageUrl = filterUri(telemetry.data.PageUrl);
      }

      break;

    case 'PageviewData':
    case 'PageviewPerformanceData':
      if (telemetry.baseData?.uri) {
        telemetry.baseData.uri = filterUri(telemetry.baseData.uri);
      }

      if (telemetry.baseData?.refUri) {
        telemetry.baseData.refUri = filterUri(telemetry.baseData.refUri);
      }

      break;

    default:
      return true;
  }

  return true;
}
/* eslint-enable no-param-reassign */

function filterUri(uri: string): string {
  const [path, query] = uri.split('?', 2);

  if (!query) {
    return uri;
  }

  const filteredQuery = query
    .split('&')
    .map(keyValue => {
      const [key] = keyValue.split('=', 2);

      if (!key) {
        return keyValue;
      }

      return isRedactedKey(key) ? `${key}=${REDACTED_VALUE}` : keyValue;
    })
    .join('&');

  return `${path}?${filteredQuery}`;
}

function isRedactedKey(key: string): boolean {
  return decodeURIComponent(key)
    .split(/[_\-:.|\s[\]+]/)
    .some(part => {
      if (REDACTED_TOKENS.includes(part.toLowerCase())) {
        return true;
      }

      return part
        .split(/([A-Z][a-z]*|[0-9])/)
        .some(word => REDACTED_TOKENS.includes(word.toLowerCase()));
    });
}
