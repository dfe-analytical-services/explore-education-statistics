import { ITelemetryItem } from '@microsoft/applicationinsights-web';
import { filterSensitiveData } from '../applicationInsightsService';

describe('filterSensitiveData', () => {
  const sensitiveQueryParamVariations: string[][] = [
    ['email'],
    ['email[0]'],
    ['email[]'],
    ['Email'],
    ['EMAIL'],
    ['eMail'],
    ['email1'],
    ['email10address'],
    ['emailA'],
    ['address_email'],
    ['email_address'],
    ['email-address'],
    ['email:address'],
    ['email+address'],
    ['email.address'],
    ['EmailAddress'],
    ['emailAddress'],
    ['emailAddress1'],
    ['emailAddress10'],
    ['email_address[0]'],
    ['email_address[]'],
    ['email++address'],
    ['email[address]'],
    ['address[email]'],
    // These have escaped reserved characters
    // such as ' ', ':', '+', '|'
    ['email%20address'],
    ['email%3Aaddress'],
    ['email%2Baddress'],
    ['email%7Caddress'],
  ];

  describe('RemoteDependencyData', () => {
    const baseType = 'RemoteDependencyData';

    test('does nothing if there is no query', () => {
      const telemetry: ITelemetryItem = {
        baseType,
        name: '',
        baseData: {
          name: 'GET /test',
          target: 'https://localhost/test',
          type: 'Ajax',
        },
      };

      expect(filterSensitiveData(telemetry)).toBe(true);
      expect(telemetry).toEqual<ITelemetryItem>({
        baseType,
        name: '',
        baseData: {
          name: 'GET /test',
          target: 'https://localhost/test',
          type: 'Ajax',
        },
      });
    });

    test('does nothing if there are no targeted fields', () => {
      const telemetry: ITelemetryItem = {
        baseType,
        name: '',
        baseData: {
          type: 'Ajax',
        },
      };

      expect(filterSensitiveData(telemetry)).toBe(true);
      expect(telemetry).toEqual<ITelemetryItem>({
        baseType,
        name: '',
        baseData: {
          type: 'Ajax',
        },
      });
    });

    test('redacts single candidate query parameter', () => {
      const telemetry: ITelemetryItem = {
        baseType,
        name: '',
        baseData: {
          name: 'GET /test?code=my-code',
          target: 'https://localhost/test?code=my-code',
          type: 'Ajax',
        },
      };

      expect(filterSensitiveData(telemetry)).toBe(true);
      expect(telemetry).toEqual<ITelemetryItem>({
        baseType,
        name: '',
        baseData: {
          name: 'GET /test?code=__redacted__',
          target: 'https://localhost/test?code=__redacted__',
          type: 'Ajax',
        },
      });
    });

    test('redacts multiple candidate query parameters', () => {
      const telemetry: ITelemetryItem = {
        baseType,
        name: '',
        baseData: {
          name: 'GET /test?password=my-password&code=my-code&token=my-token&email=my-email',
          target:
            'https://localhost/test?password=my-password&code=my-code&token=my-token&email=my-email',
          type: 'Ajax',
        },
      };

      expect(filterSensitiveData(telemetry)).toBe(true);
      expect(telemetry).toEqual<ITelemetryItem>({
        baseType,
        name: '',
        baseData: {
          name: 'GET /test?password=__redacted__&code=__redacted__&token=__redacted__&email=__redacted__',
          target:
            'https://localhost/test?password=__redacted__&code=__redacted__&token=__redacted__&email=__redacted__',
          type: 'Ajax',
        },
      });
    });

    test('tries to redact query parameters on invalid URLs', () => {
      const telemetry: ITelemetryItem = {
        baseType,
        name: '',
        baseData: {
          name: 'GET not-a-url?password=my-password',
          target: 'not-a-url?password=my-password',
          type: 'Ajax',
        },
      };

      expect(filterSensitiveData(telemetry)).toBe(true);
      expect(telemetry).toEqual<ITelemetryItem>({
        baseType,
        name: '',
        baseData: {
          name: 'GET not-a-url?password=__redacted__',
          target: 'not-a-url?password=__redacted__',
          type: 'Ajax',
        },
      });
    });

    test.each(sensitiveQueryParamVariations)(
      'redacts query parameter `%s`',
      key => {
        const telemetry: ITelemetryItem = {
          baseType,
          name: '',
          baseData: {
            name: `GET /test?${key}=some-value`,
            target: `https://localhost/test?${key}=some-value`,
            type: 'Ajax',
          },
        };

        expect(filterSensitiveData(telemetry)).toBe(true);
        expect(telemetry).toEqual<ITelemetryItem>({
          baseType,
          name: '',
          baseData: {
            name: `GET /test?${key}=__redacted__`,
            target: `https://localhost/test?${key}=__redacted__`,
            type: 'Ajax',
          },
        });
      },
    );

    test('does nothing to non-candidate query parameters', () => {
      const telemetry: ITelemetryItem = {
        baseType,
        name: '',
        baseData: {
          name: 'GET /test?key=value',
          target: 'https://localhost/test?key=value',
          type: 'Ajax',
        },
      };

      expect(filterSensitiveData(telemetry)).toBe(true);
      expect(telemetry).toEqual<ITelemetryItem>({
        baseType,
        name: '',
        baseData: {
          name: 'GET /test?key=value',
          target: 'https://localhost/test?key=value',
          type: 'Ajax',
        },
      });
    });

    test('does nothing to non-candidate, lookalike query parameter', () => {
      const telemetry: ITelemetryItem = {
        baseType,
        name: '',
        baseData: {
          name: 'GET /test?encodeType=something',
          target: 'https://localhost/test?encodeType=something',
          type: 'Ajax',
        },
      };

      expect(filterSensitiveData(telemetry)).toBe(true);
      expect(telemetry).toEqual<ITelemetryItem>({
        baseType,
        name: '',
        baseData: {
          name: 'GET /test?encodeType=something',
          target: 'https://localhost/test?encodeType=something',
          type: 'Ajax',
        },
      });
    });
  });

  describe('MetricData', () => {
    const baseType = 'MetricData';

    test('does nothing if there is no query', () => {
      const telemetry: ITelemetryItem = {
        baseType,
        name: '',
        data: {
          PageUrl: 'https://localhost/test',
        },
      };

      expect(filterSensitiveData(telemetry)).toBe(true);
      expect(telemetry).toEqual<ITelemetryItem>({
        baseType,
        name: '',
        data: {
          PageUrl: 'https://localhost/test',
        },
      });
    });

    test('does nothing if there are no targeted fields', () => {
      const telemetry: ITelemetryItem = {
        baseType,
        name: '',
      };

      expect(filterSensitiveData(telemetry)).toBe(true);
      expect(telemetry).toEqual<ITelemetryItem>({
        baseType,
        name: '',
      });
    });

    test('redacts single candidate query parameter', () => {
      const telemetry: ITelemetryItem = {
        baseType,
        name: '',
        data: {
          PageUrl: 'https://localhost/test?code=my-code',
        },
      };

      expect(filterSensitiveData(telemetry)).toBe(true);
      expect(telemetry).toEqual<ITelemetryItem>({
        baseType,
        name: '',
        data: {
          PageUrl: 'https://localhost/test?code=__redacted__',
        },
      });
    });

    test('redacts multiple candidate query parameters', () => {
      const telemetry: ITelemetryItem = {
        baseType,
        name: '',
        data: {
          PageUrl:
            'https://localhost/test?password=my-password&code=my-code&token=my-token&email=my-email',
        },
      };

      expect(filterSensitiveData(telemetry)).toBe(true);
      expect(telemetry).toEqual<ITelemetryItem>({
        baseType,
        name: '',
        data: {
          PageUrl:
            'https://localhost/test?password=__redacted__&code=__redacted__&token=__redacted__&email=__redacted__',
        },
      });
    });

    test('tries to redact query parameters on invalid URLs', () => {
      const telemetry: ITelemetryItem = {
        baseType,
        name: '',
        data: {
          PageUrl: 'not-a-url?password=my-password',
        },
      };

      expect(filterSensitiveData(telemetry)).toBe(true);
      expect(telemetry).toEqual<ITelemetryItem>({
        baseType,
        name: '',
        data: {
          PageUrl: 'not-a-url?password=__redacted__',
        },
      });
    });

    test.each(sensitiveQueryParamVariations)(
      'redacts query parameter `%s`',
      key => {
        const telemetry: ITelemetryItem = {
          baseType,
          name: '',
          data: {
            PageUrl: `https://localhost/test?${key}=some-value`,
          },
        };

        expect(filterSensitiveData(telemetry)).toBe(true);
        expect(telemetry).toEqual<ITelemetryItem>({
          baseType,
          name: '',
          data: {
            PageUrl: `https://localhost/test?${key}=__redacted__`,
          },
        });
      },
    );

    test('does nothing to non-candidate query parameters', () => {
      const telemetry: ITelemetryItem = {
        baseType,
        name: '',
        data: {
          PageUrl: 'https://localhost/test?key=value',
        },
      };

      expect(filterSensitiveData(telemetry)).toBe(true);
      expect(telemetry).toEqual<ITelemetryItem>({
        baseType,
        name: '',
        data: {
          PageUrl: 'https://localhost/test?key=value',
        },
      });
    });

    test('does nothing to non-candidate, lookalike query parameter', () => {
      const telemetry: ITelemetryItem = {
        baseType,
        name: '',
        data: {
          PageUrl: 'https://localhost/test?encodeType=something',
        },
      };

      expect(filterSensitiveData(telemetry)).toBe(true);
      expect(telemetry).toEqual<ITelemetryItem>({
        baseType,
        name: '',
        data: {
          PageUrl: 'https://localhost/test?encodeType=something',
        },
      });
    });
  });

  describe('PageviewData', () => {
    const baseType = 'PageviewData';

    test('does nothing if there is no query', () => {
      const telemetry: ITelemetryItem = {
        baseType,
        name: '',
        baseData: {
          refUri: 'https://localhost/test',
          uri: 'https://localhost/test',
        },
      };

      expect(filterSensitiveData(telemetry)).toBe(true);
      expect(telemetry).toEqual<ITelemetryItem>({
        baseType,
        name: '',
        baseData: {
          refUri: 'https://localhost/test',
          uri: 'https://localhost/test',
        },
      });
    });

    test('does nothing if there are no targeted fields', () => {
      const telemetry: ITelemetryItem = {
        baseType,
        name: '',
      };

      expect(filterSensitiveData(telemetry)).toBe(true);
      expect(telemetry).toEqual<ITelemetryItem>({
        baseType,
        name: '',
      });
    });

    test('redacts single candidate query parameter', () => {
      const telemetry: ITelemetryItem = {
        baseType,
        name: '',
        baseData: {
          refUri: 'https://localhost/test?code=my-code',
          uri: 'https://localhost/test?code=my-code',
        },
      };

      expect(filterSensitiveData(telemetry)).toBe(true);
      expect(telemetry).toEqual<ITelemetryItem>({
        baseType,
        name: '',
        baseData: {
          refUri: 'https://localhost/test?code=__redacted__',
          uri: 'https://localhost/test?code=__redacted__',
        },
      });
    });

    test('redacts multiple candidate query parameters', () => {
      const telemetry: ITelemetryItem = {
        baseType,
        name: '',
        baseData: {
          refUri:
            'https://localhost/test?password=my-password&code=my-code&token=my-token&email=my-email',
          uri: 'https://localhost/test?password=my-password&code=my-code&token=my-token&email=my-email',
        },
      };

      expect(filterSensitiveData(telemetry)).toBe(true);
      expect(telemetry).toEqual<ITelemetryItem>({
        baseType,
        name: '',
        baseData: {
          refUri:
            'https://localhost/test?password=__redacted__&code=__redacted__&token=__redacted__&email=__redacted__',
          uri: 'https://localhost/test?password=__redacted__&code=__redacted__&token=__redacted__&email=__redacted__',
        },
      });
    });

    test('tries to redact query parameters on invalid URLs', () => {
      const telemetry: ITelemetryItem = {
        baseType,
        name: '',
        baseData: {
          refUri: 'not-a-url?password=my-password',
          uri: 'not-a-url?password=my-password',
        },
      };

      expect(filterSensitiveData(telemetry)).toBe(true);
      expect(telemetry).toEqual<ITelemetryItem>({
        baseType,
        name: '',
        baseData: {
          refUri: 'not-a-url?password=__redacted__',
          uri: 'not-a-url?password=__redacted__',
        },
      });
    });

    test.each(sensitiveQueryParamVariations)(
      'redacts query parameter `%s`',
      key => {
        const telemetry: ITelemetryItem = {
          baseType,
          name: '',
          baseData: {
            refUri: `https://localhost/test?${key}=some-value`,
            uri: `https://localhost/test?${key}=some-value`,
          },
        };

        expect(filterSensitiveData(telemetry)).toBe(true);
        expect(telemetry).toEqual<ITelemetryItem>({
          baseType,
          name: '',
          baseData: {
            refUri: `https://localhost/test?${key}=__redacted__`,
            uri: `https://localhost/test?${key}=__redacted__`,
          },
        });
      },
    );

    test('does nothing to non-candidate query parameters', () => {
      const telemetry: ITelemetryItem = {
        baseType,
        name: '',
        baseData: {
          refUri: 'https://localhost/test?key=value',
          uri: 'https://localhost/test?key=value',
        },
      };

      expect(filterSensitiveData(telemetry)).toBe(true);
      expect(telemetry).toEqual<ITelemetryItem>({
        baseType,
        name: '',
        baseData: {
          refUri: 'https://localhost/test?key=value',
          uri: 'https://localhost/test?key=value',
        },
      });
    });

    test('does nothing to non-candidate, lookalike query parameter', () => {
      const telemetry: ITelemetryItem = {
        baseType,
        name: '',
        baseData: {
          refUri: 'https://localhost/test?encodeType=something',
          uri: 'https://localhost/test?encodeType=something',
        },
      };

      expect(filterSensitiveData(telemetry)).toBe(true);
      expect(telemetry).toEqual<ITelemetryItem>({
        baseType,
        name: '',
        baseData: {
          refUri: 'https://localhost/test?encodeType=something',
          uri: 'https://localhost/test?encodeType=something',
        },
      });
    });
  });

  describe('PageviewPerformanceData', () => {
    const baseType = 'PageviewPerformanceData';

    test('does nothing if there is no query', () => {
      const telemetry: ITelemetryItem = {
        baseType,
        name: '',
        baseData: {
          uri: 'https://localhost/test',
        },
      };

      expect(filterSensitiveData(telemetry)).toBe(true);
      expect(telemetry).toEqual<ITelemetryItem>({
        baseType,
        name: '',
        baseData: {
          uri: 'https://localhost/test',
        },
      });
    });

    test('does nothing if there are no targeted fields', () => {
      const telemetry: ITelemetryItem = {
        baseType,
        name: '',
      };

      expect(filterSensitiveData(telemetry)).toBe(true);
      expect(telemetry).toEqual<ITelemetryItem>({
        baseType,
        name: '',
      });
    });

    test('redacts single candidate query parameter', () => {
      const telemetry: ITelemetryItem = {
        baseType,
        name: '',
        baseData: {
          uri: 'https://localhost/test?code=my-code',
        },
      };

      expect(filterSensitiveData(telemetry)).toBe(true);
      expect(telemetry).toEqual<ITelemetryItem>({
        baseType,
        name: '',
        baseData: {
          uri: 'https://localhost/test?code=__redacted__',
        },
      });
    });

    test('redacts multiple candidate query parameters', () => {
      const telemetry: ITelemetryItem = {
        baseType,
        name: '',
        baseData: {
          uri: 'https://localhost/test?password=my-password&code=my-code&token=my-token&email=my-email',
        },
      };

      expect(filterSensitiveData(telemetry)).toBe(true);
      expect(telemetry).toEqual<ITelemetryItem>({
        baseType,
        name: '',
        baseData: {
          uri: 'https://localhost/test?password=__redacted__&code=__redacted__&token=__redacted__&email=__redacted__',
        },
      });
    });

    test('tries to redact query parameters on invalid URLs', () => {
      const telemetry: ITelemetryItem = {
        baseType,
        name: '',
        baseData: {
          uri: 'not-a-url?password=my-password',
        },
      };

      expect(filterSensitiveData(telemetry)).toBe(true);
      expect(telemetry).toEqual<ITelemetryItem>({
        baseType,
        name: '',
        baseData: {
          uri: 'not-a-url?password=__redacted__',
        },
      });
    });

    test.each(sensitiveQueryParamVariations)(
      'redacts query parameter `%s`',
      key => {
        const telemetry: ITelemetryItem = {
          baseType,
          name: '',
          baseData: {
            uri: `https://localhost/test?${key}=some-value`,
          },
        };

        expect(filterSensitiveData(telemetry)).toBe(true);
        expect(telemetry).toEqual<ITelemetryItem>({
          baseType,
          name: '',
          baseData: {
            uri: `https://localhost/test?${key}=__redacted__`,
          },
        });
      },
    );

    test('does nothing to non-candidate query parameters', () => {
      const telemetry: ITelemetryItem = {
        baseType,
        name: '',
        baseData: {
          uri: 'https://localhost/test?key=value',
        },
      };

      expect(filterSensitiveData(telemetry)).toBe(true);
      expect(telemetry).toEqual<ITelemetryItem>({
        baseType,
        name: '',
        baseData: {
          uri: 'https://localhost/test?key=value',
        },
      });
    });

    test('does nothing to non-candidate, lookalike query parameter', () => {
      const telemetry: ITelemetryItem = {
        baseType,
        name: '',
        baseData: {
          uri: 'https://localhost/test?encodeType=something',
        },
      };

      expect(filterSensitiveData(telemetry)).toBe(true);
      expect(telemetry).toEqual<ITelemetryItem>({
        baseType,
        name: '',
        baseData: {
          uri: 'https://localhost/test?encodeType=something',
        },
      });
    });
  });
});
