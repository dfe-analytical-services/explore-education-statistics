/* eslint-disable */
import { check } from 'k6';
import http from 'k6/http';
import { htmlReport } from 'https://raw.githubusercontent.com/luke-h1/k6-reporter/main/dist/bundle.js';
import { Rate } from "k6/metrics";

const BASE_URL = 'https://data.dev.explore-education-statistics.service.gov.uk';

export const options = {
  vus: 60,
  duration: '60m',

  noConnectionReuse: true,
};

export let errorRate = new Rate("errors");


export default function () {
  const res = http.get(
    `${BASE_URL}/api/tablebuilder/release/22800dae-b00c-4bec-b36d-08d8e9310e06/data-block/05ab390d-f291-4fae-4b12-08d8e934073e`,
  );
  console.log('status code is', res.status);
  console.log(`Response time was ${String(res.timings.duration)} ms`);

  check(res, {
    'response code was 200': res => res.status == 200,
  });
}

export function handleSummary(data) {
  return {
    'test-results/admin-appeals.html': htmlReport(data),
  };
}
