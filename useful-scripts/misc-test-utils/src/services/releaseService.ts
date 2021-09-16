/* eslint-disable no-await-in-loop */
/* eslint-disable no-console */
import chalk from 'chalk';
import faker from 'faker';
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
  addMetaGuidance: async (
    subjArr: { id: string; content: string }[],
    releaseId: string,
  ): Promise<boolean> => {
    await adminApi.patch(`/api/release/${releaseId}/meta-guidance`, {
      content: '<p>testing</p>',
      subjects: subjArr,
    });
    return true;
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
  ): Promise<boolean> => {
    await adminApi.put(
      `/api/release/${releaseId}/content/section/${sectionId}/block/${blockId}`,
      {
        body: `<p>${faker.lorem.lines(100)}</p>`,
      },
    );
    return true;
  },
};
export default releaseService;
