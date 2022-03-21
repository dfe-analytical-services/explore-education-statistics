/* eslint-disable no-await-in-loop */
/* eslint-disable no-console */
import chalk from 'chalk';
import { ReleaseProgressResponse } from '../types/ReleaseProgressResponse';
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
        type: 'AdHocStatistics',
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
  ): Promise<void> => {
    await adminApi.patch(`/api/release/${releaseId}/data-guidance`, {
      content: '<p>testing</p>',
      subjects: subjArr,
    });
  },

  getReleaseProgress: async (
    releaseId: string,
    url?: string,
  ): Promise<ReleaseProgressResponse> => {
    const res = await adminApi.get(`/api/releases/${releaseId}/stage-status`, {
      maxContentLength: Infinity,
      maxBodyLength: Infinity,
    });
    // eslint-disable-next-line no-constant-condition
    if (res.data.publishingStage !== 'Complete') {
      console.log(
        'Overall stage of publication:',
        chalk.blue(res.data.overallStage),
      );
      await sleep(1500);
      await releaseService.getReleaseProgress(releaseId);
    }
    console.log(chalk.green(`Published release: ${url}`));
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

  addContentSection: async (releaseId: string): Promise<string> => {
    const res = await adminApi.post(
      `/api/release/${releaseId}/content/sections/add`,
      {
        order: 0,
      },
    );
    return res.data.id;
  },
  addTextBlock: async (
    releaseId: string,
    sectionId: string,
  ): Promise<string> => {
    const res = await adminApi.post(
      `/api/release/${releaseId}/content/section/${sectionId}/blocks/add`,
      {
        body: '',
        order: 0,
        type: 'HtmlBlock',
      },
    );
    return res.data.id;
  },
  addTextContent: async (
    releaseId: string,
    sectionId: string,
    blockId: string,
  ): Promise<void> => {
    await adminApi.put(
      `/api/release/${releaseId}/content/section/${sectionId}/block/${blockId}`,
      {
        body: `<p>Ad ullamco reprehenderit sunt reprehenderit tempor est proident dolor. Nisi occaecat ut duis qui duis eu exercitation ex aute incididunt nisi fugiat est. Incididunt laboris sit aliqua culpa anim culpa. Fugiat id minim laborum pariatur sint fugiat. Ea.</p>`,
      },
    );
  },
};
export default releaseService;
