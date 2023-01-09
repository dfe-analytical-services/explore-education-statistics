/* eslint-disable no-shadow */
/* eslint-disable no-await-in-loop */
/* eslint-disable no-console */
import chalk from 'chalk';
import spinner from '../utils/spinner';
import { ReleaseProgressResponse } from '../types/ReleaseProgressResponse';
import sleep from '../utils/sleep';
import { Release } from '../types/Release';
import adminApi from '../utils/adminApi';
import logger from '../utils/logger';

const { ADMIN_URL } = process.env;
const releaseService = {
  createRelease: async (publicationId: string): Promise<string> => {
    spinner.start();
    console.time('createRelease');

    const res = await adminApi.post(
      `/api/publications/${publicationId}/releases`,
      {
        publicationId,
        templateReleaseId: '',
        timePeriodCoverage: { value: 'AY' },
        type: 'AdHocStatistics',
        year: Math.floor(Math.random() * (9999 - 1000 + 1) + 1000),
      },
    );
    console.timeEnd('createRelease');
    const releaseId = res.data.id;
    spinner.succeed(
      `Release URL: ${ADMIN_URL}/publication/${publicationId}/release/${releaseId}/data`,
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
    spinner.start();
    const res = await adminApi.get(`/api/releases/${releaseId}/stage-status`, {
      maxContentLength: Infinity,
      maxBodyLength: Infinity,
    });
    // eslint-disable-next-line no-constant-condition
    if (res.data.publishingStage !== 'Complete') {
      spinner.info(
        `Overall stage of publication: ${chalk.blue(res.data.overallStage)}`,
      );
      await sleep(1500);
      await releaseService.getReleaseProgress(releaseId);
    }
    if (url) {
      spinner.succeed(`Published release: ${url}`);
    }
    return res.data;
  },
  publishRelease: async (release: Release, releaseId: string) => {
    await adminApi.post(`/api/releases/${releaseId}/status`, release);
  },
  getRelease: async (releaseId: string) => {
    spinner.start();
    const res = await adminApi.get(`/api/releases/${releaseId}`);
    const release: Release = res.data;
    await sleep(1000);
    spinner.stop();
    return {
      ...release,
      approvalstatus: 'Approved',
      amendment: 'false',
      latestInternalReleaseNote: 'Approved by publisher testing',
      publishMethod: 'Immediate',
    };
  },

  getAllReleases: async (publicationId: string): Promise<Release[]> => {
    const res = await adminApi.get(
      `/api/publication/${publicationId}/releases?live=false&pageSize=500&includePermissions=true`,
    );

    const releases: Release[] = res.data.results;

    logger.info(
      `Found ${releases.length} ${
        releases.length > 1 ? 'releases' : 'release'
      } for publication ${publicationId}`,
    );

    return releases;
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
