/* eslint-disable no-console */
import chalk from 'chalk';
import { randomInt } from 'crypto';
import adminApi from '../utils/adminApi';

const id = randomInt(10, 100);

const themeService = {
  deleteTheme: async (themeId: string): Promise<void> => {
    console.log(chalk.green(`Deleting theme with ID ${chalk.blue(themeId)}`));
    await adminApi.delete(`/api/themes/${themeId}`);
    console.log(chalk.green(`Deleted theme with ID ${chalk.blue(themeId)}`));
  },
  renameTheme: async (themeId: string): Promise<void> => {
    await adminApi.put(`/api/themes/${themeId}`, {
      title: `UI test theme-${id}`,
      summary: 'test summary',
      slug: `ui-test-theme-${id}`,
    });
  },
};
export default themeService;
