import { Counter, Trend } from 'k6/metrics';

export const publicPageOverallRequestsSuccessCount = new Counter(
  'ees_public_page_overall_requests_success',
);

export const publicPageOverallRequestsFailureCount = new Counter(
  'ees_public_page_overall_requests_failure',
);

export const publicPageOverallRequestsDuration = new Trend(
  'ees_get_public_page_overall_requests_duration',
  true,
);

export const publicPageMainRequestSuccessCount = new Counter(
  'ees_get_public_page_main_request_success',
);

export const publicPageMainRequestFailureCount = new Counter(
  'ees_get_public_page_main_request_failure',
);

export const publicPageMainRequestDuration = new Trend(
  'ees_get_public_page_main_request_duration',
  true,
);

export const publicPageOverallDataRequestsSuccessCount = new Counter(
  'ees_public_page_overall_data_requests_success',
);

export const publicPageOverallDataRequestsDuration = new Trend(
  'ees_get_public_page_overall_data_requests_duration',
  true,
);

export const publicPageDataRequestDuration = new Trend(
  'ees_get_public_page_data_request_duration',
  true,
);

export const publicPageDataRequestSuccessCount = new Counter(
  'ees_get_public_page_data_request_success',
);

export const publicPageDataRequestFailureCount = new Counter(
  'ees_get_public_page_data_request_failure',
);

export default {};
