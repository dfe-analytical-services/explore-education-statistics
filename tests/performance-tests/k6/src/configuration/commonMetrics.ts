import { Rate } from 'k6/metrics';

export const errorRate = new Rate('ees_errors');

export default {};
