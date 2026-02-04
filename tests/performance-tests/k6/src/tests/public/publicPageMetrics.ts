import { Counter, Trend } from 'k6/metrics';

export const getPublicPageOverallRequestsSuccessCount = new Counter(
  'ees_public_page_overall_requests_success',
);

export const getPublicPageOverallRequestsFailureCount = new Counter(
  'ees_public_page_overall_requests_failure',
);

export const getPublicPageOverallRequestsDuration = new Trend(
  'ees_get_public_page_overall_requests_duration',
  true,
);

export const getPublicPageMainRequestSuccessCount = new Counter(
  'ees_get_public_page_main_request_success',
);

export const getPublicPageMainRequestFailureCount = new Counter(
  'ees_get_public_page_main_request_failure',
);

export const getPublicPageMainRequestDuration = new Trend(
  'ees_get_public_page_main_request_duration',
  true,
);

export const getPublicPageOverallDataRequestsSuccessCount = new Counter(
  'ees_public_page_overall_data_requests_success',
);

export const getPublicPageOverallDataRequestsDuration = new Trend(
  'ees_get_public_page_overall_data_requests_duration',
  true,
);

export const getPublicPageDataRequestDuration = new Trend(
  'ees_get_public_page_data_request_duration',
  true,
);

export const getPublicPageDataRequestSuccessCount = new Counter(
  'ees_get_public_page_data_request_success',
);

export const getPublicPageDataRequestFailureCount = new Counter(
  'ees_get_public_page_data_request_failure',
);

export default {};
