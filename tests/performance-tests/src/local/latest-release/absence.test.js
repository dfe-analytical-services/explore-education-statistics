/* eslint-disable */
import { check } from 'k6';
import http from 'k6/http';
import { htmlReport } from 'https://raw.githubusercontent.com/luke-h1/k6-reporter/main/dist/bundle.js';
import { Rate } from "k6/metrics";

const username = '';
const password = '';

const credentials = `${username}:${password}`;

const BASE_URL = `http://${credentials}@localhost:3000`;

export const options = {
  stages: [
    { duration: '1m', target: 100 },
    { duration: '1m', target: 0 },
  ],
  noConnectionReuse: true,
};

export let errorRate = new Rate("errors");


export default function () {
  const res = http.get(
    `${BASE_URL}/find-statistics/pupil-absence-in-schools-in-england`,
    {
      timeout: '4m',
      auth: 'basic',
    },
  );
  console.log('status code is', res.status);
  console.log(`Response time was ${String(res.timings.duration)} ms`);

  check(res, {
    'response code was 200': res => res.status == 200,
  });

  check(res, {
    'response contained the correct text': res =>
      res.body.indexOf(
        '<h1 class="govuk-heading-xl" data-testid="page-title">Pupil absence in schools in England</h1>',
      ) !== -1,
  });
}

export function handleSummary(data) {
  return {
    'test-results/absence-local.html': htmlReport(data),
  };
}
