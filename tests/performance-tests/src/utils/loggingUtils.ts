/* eslint-disable no-console */
import constants from './constants';

const loggingUtils = {
  logDashboardUrls: () => {
    console.log(
      `\n\nEES performance results available at: ${constants.grafanaEesDashboardUrl}`,
    );
    console.log(
      `generic performance results available at: ${constants.grafanaGenericDashboardUrl}\n\n`,
    );
  },
};
export default loggingUtils;
