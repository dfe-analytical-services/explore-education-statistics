/* eslint-disable no-console */
import chalk from 'chalk';
import sleep from '../utils/sleep';
import { ReleaseData } from '../types/ReleaseData';
import adminApi from '../utils/adminApi';

const { ADMIN_URL } = process.env;
const releaseService = {
  createRelease: async (publicationId: string): Promise<string> => {
    console.time('createRelease');
    const res = await adminApi.post(
      `/api/publications/${publicationId}/releases`,
      {
        timePeriodCoverage: { value: 'AY' },
        releaseName: 2222,
        typeId: '1821abb8-68b0-431b-9770-0bea65d02ff0',
        publicationId: 'c76c1bc4-0a01-4116-74cd-08d8eebbf0c1',
        templateReleaseId: '',
      },
    );
    console.timeEnd('createRelease');
    const releaseId = res.data.id;

    console.log(
      chalk.green(
        `Release URL: ${ADMIN_URL}/publication/${publicationId}/release/${releaseId}/data`,
      ),
    );
    return releaseId;
  },
  addDataGuidance: async (
    subjArr: { id: string; content: string }[],
    releaseId: string,
  ): Promise<boolean> => {
    await adminApi.patch(`/api/release/${releaseId}/meta-guidance`, {
      content: '<p>testing</p>',
      subjects: subjArr,
    });
    return true;
  },
  getReleaseProgress: async (releaseId: string) => {
    const res = await adminApi.get(`/api/releases/${releaseId}/stage-status`, {
      maxContentLength: Infinity,
      maxBodyLength: Infinity,
    });

    // eslint-disable-next-line no-constant-condition
    while (res.data.overallStage !== 'Complete') {
      console.log(
        chalk.blue('overall stage'),
        chalk.green(res.data.overallStage),
      );
      // eslint-disable-next-line no-await-in-loop
      await sleep(3000);
      // eslint-disable-next-line no-await-in-loop
      await releaseService.getReleaseProgress(releaseId);
    }
    return res.data;
  },
  publishRelease: async (obj: ReleaseData, releaseId: string) => {
    await adminApi.post(`/api/releases/${releaseId}/status`, obj);
  },
  getRelease: async (releaseId: string) => {
    const res = await adminApi.get(`/api/releases/${releaseId}`);
    const releaseData: ReleaseData = res.data;
    await sleep(1000);
    return {
      ...releaseData,
      approvalstatus: 'Approved',
      amendment: 'false',
      latestInternalReleaseNote: 'Approved by publisher testing',
      publishMethod: 'Immediate',
    };
  },
};
export default releaseService;
