import { Counter, Trend } from 'k6/metrics';

export const getPublicPageFullRequestsSuccessCount = new Counter(
  'ees_public_page_overall_requests_success',
);

export const getPublicPageFullRequestsFailureCount = new Counter(
  'ees_public_page_overall_requests_failure',
);

export const getPublicPageFullRequestsDuration = new Trend(
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
