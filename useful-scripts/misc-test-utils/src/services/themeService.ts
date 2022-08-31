/* eslint-disable no-console */
import chalk from 'chalk';
import { randomInt } from 'crypto';
import spinner from '../utils/spinner';
import adminApi from '../utils/adminApi';

const id = randomInt(10, 100);

const themeService = {
  deleteTheme: async (themeId: string): Promise<void> => {
    spinner.start(`Deleting theme with ID ${chalk.blue(themeId)}`);
    await adminApi.delete(`/api/themes/${themeId}`);
    spinner.succeed(`Deleted theme with ID ${chalk.blue(themeId)}`);
  },
  renameTheme: async (themeId: string): Promise<void> => {
    spinner.start(`Renaming theme with ID ${chalk.blue(themeId)}`);
    await adminApi.put(`/api/themes/${themeId}`, {
      title: `UI test theme-${id}`,
      summary: 'test summary',
      slug: `ui-test-theme-${id}`,
    });
    spinner.succeed(`Renamed theme with ID ${chalk.blue(themeId)}`);
  },
};
export default themeService;
