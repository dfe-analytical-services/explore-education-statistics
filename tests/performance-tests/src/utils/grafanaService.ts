import http, { RefinedResponse } from 'k6/http';
import { stringifyWithoutNulls } from './utils';

interface TestDetails {
  name: string;
  config?: object;
}

class GrafanaService {
  addAnnotation({
    title,
    content,
    tags = [],
    timeMillis,
  }: {
    title?: string;
    content: string;
    tags?: string[];
    timeMillis?: number;
  }) {
    http.post(
      'http://influxdb:8086/write?db=k6&precision=ms',
      `events title="${title}",text="${content}",tags="${tags.join(',')}" ${
        timeMillis ?? Date.now()
      }`,
      {
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
        },
      },
    );
  }

  testStart(testDetails: TestDetails) {
    this.logTestNameAndConfiguration({
      ...testDetails,
      annotationHeading: 'Test started',
      tags: ['test-start'],
    });
  }

  testStop(testDetails: TestDetails) {
    this.logTestNameAndConfiguration({
      ...testDetails,
      annotationHeading: 'Test stopped',
      tags: ['test-stop'],
    });
  }

  reportErrorResponse({
    name,
    response,
  }: {
    name: string;
    response: RefinedResponse<'text'>;
  }) {
    return this.reportDetailedError({
      name,
      url: response.url,
      requestBody: JSON.parse(response.request.body),
      errorCode: response.error_code,
      errorMessage: response.error,
    });
  }

  reportErrorObject({
    name,
    error,
    url,
    requestBody,
  }: {
    name: string;
    error: unknown;
    url: string;
    requestBody: object;
  }) {
    return this.reportDetailedError({
      name,
      url,
      errorMessage: `${error}`,
      errorCode: -1,
      requestBody,
    });
  }

  reportDetailedError({
    name,
    url,
    requestBody,
    errorCode,
    errorMessage,
  }: {
    name: string;
    url: string;
    requestBody: object;
    errorCode: number;
    errorMessage: string;
  }) {
    return this.addAnnotation({
      content: `
      <h1>Error in test ${name}</h1>
      <dl>
        <dt>Error code</dt><dd>${errorCode}</dd>
        <dt>Error message</dt><dd>${errorMessage}</dd>
        <dt>URL</dt><dd>${url}</dd>
      </dl>
      <pre>
        ${JSON.stringify(requestBody, null, 2)}
      </pre>`,
      tags: ['error'],
    });
  }

  testStageStarted({ name, stageIndex }: { name: string; stageIndex: number }) {
    this.addAnnotation({
      content: `<h1>Stage ${stageIndex + 1} started</h1><p>${name}</p>`,
      tags: ['test-stage-started'],
    });
  }

  thresholdBreach({
    name,
    thresholdName,
    thresholdValue,
    value,
  }: {
    name: string;
    thresholdName: string;
    thresholdValue: string;
    value: string;
    request?: object;
  }) {
    const content = `<h1>Threshold breach</h1><p>${name}</p>`;
    const thresholdContent = `
      <h2>${thresholdName}</h2>
      <p>Expecting ${thresholdValue} Got: ${value}</p>
    `;
    this.addAnnotation({
      content: `${content}${thresholdContent}`,
      tags: ['threshold'],
    });
  }

  private logTestNameAndConfiguration({
    annotationHeading,
    name,
    config,
    tags,
  }: TestDetails & {
    annotationHeading: string;
    tags: string[];
  }) {
    const content = `<h1>${annotationHeading}</h1><p>${name}</p>`;
    const configurationContent = config
      ? `<h2>Configuration</h2>
         <pre>${stringifyWithoutNulls(config)}</pre>`
      : '';
    this.addAnnotation({
      content: `${content}${configurationContent}`,
      tags,
    });
  }
}

const grafanaService = new GrafanaService();
export default grafanaService;
