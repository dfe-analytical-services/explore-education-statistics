import config from './config';
import logger from './logger';

const logDashboardUrls = () => {
  logger.info(
    `\n\nEES performance results available at: ${config.grafanaEesDashboardUrl}`,
  );
  logger.info(
    `generic performance results available at: ${config.grafanaGenericDashboardUrl}\n\n`,
  );
};
export default logDashboardUrls;
