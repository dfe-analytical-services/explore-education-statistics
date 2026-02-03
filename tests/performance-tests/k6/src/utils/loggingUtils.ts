/* eslint-disable no-console */
import constants from './constants';

export default {
  logDashboardUrls: () => {
    console.log(
      `\n\nEES performance results available at: ${constants.grafanaEesDashboardUrl}`,
    );
    console.log(
      `generic performance results available at: ${constants.grafanaGenericDashboardUrl}\n\n`,
    );
  },
};
