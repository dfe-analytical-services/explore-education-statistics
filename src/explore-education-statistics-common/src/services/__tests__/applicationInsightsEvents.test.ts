import { ApplicationInsights } from '@microsoft/applicationinsights-web';

jest.mock('@microsoft/applicationinsights-web', () => ({
  ApplicationInsights: jest.fn(),
}));

describe('application event delivery', () => {
  const trackEvent = jest.fn();
  const flush = jest.fn();
  const loadAppInsights = jest.fn();

  function loadService() {
    let service: typeof import('../applicationInsightsService');
    jest.isolateModules(() => {
      // Each isolated module needs its own SDK mock before constructing the client.
      // eslint-disable-next-line global-require, @typescript-eslint/no-require-imports
      const sdk = require('@microsoft/applicationinsights-web') as {
        ApplicationInsights: jest.MockedClass<typeof ApplicationInsights>;
      };
      sdk.ApplicationInsights.mockImplementation(
        () =>
          ({
            config: {},
            trackEvent,
            flush,
            loadAppInsights,
            addTelemetryInitializer: jest.fn(),
          }) as unknown as ApplicationInsights,
      );
      // eslint-disable-next-line global-require, @typescript-eslint/no-require-imports
      service = require('../applicationInsightsService');
    });
    return service!;
  }

  test('buffers events until initialisation, then delivers once and flushes failures', () => {
    const service = loadService();
    service.trackApplicationEvent(
      'AdminSignInOutcome',
      { outcome: 'failure' },
      true,
    );
    expect(trackEvent).not.toHaveBeenCalled();
    service.initApplicationInsights('test-key');
    service.initApplicationInsights('test-key');
    expect(loadAppInsights).toHaveBeenCalledTimes(1);
    expect(trackEvent).toHaveBeenCalledTimes(1);
    expect(trackEvent).toHaveBeenCalledWith(
      { name: 'AdminSignInOutcome' },
      { outcome: 'failure' },
    );
    expect(flush).toHaveBeenCalledTimes(1);
    service.trackApplicationEvent('AdminSignInOutcome', { outcome: 'success' });
    expect(trackEvent).toHaveBeenCalledTimes(2);
    expect(flush).toHaveBeenCalledTimes(1);
  });

  test('bounds queued events when telemetry remains disabled', () => {
    const service = loadService();
    service.initApplicationInsights('');
    Array.from({ length: 101 }).forEach(() =>
      service.trackApplicationEvent('event', {}),
    );
    expect(trackEvent).not.toHaveBeenCalled();
    service.initApplicationInsights('test-key');
    expect(trackEvent).toHaveBeenCalledTimes(100);
  });

  test('does not fail callers or initialisation if sending a queued event throws', () => {
    const service = loadService();
    trackEvent.mockImplementation(() => {
      throw new Error('SDK unavailable');
    });
    service.trackApplicationEvent('event', {});
    expect(() => service.initApplicationInsights('test-key')).not.toThrow();
    expect(() => service.trackApplicationEvent('event', {})).not.toThrow();
  });
});
