/* eslint-disable no-console */
import faker from 'faker';
import chalk from 'chalk';
import adminApi from '../utils/adminApi';

const { ADMIN_URL } = process.env;

const methodologyService = {
  createMethodology: async (publicationId: string): Promise<string> => {
    const res = await adminApi.post(
      `/api/publication/${publicationId}/methodology`,
    );
    console.log(
      chalk.green(
        `Methodology created: ${ADMIN_URL}/methodology/${res.data.id}/summary`,
      ),
    );
    return res.data.id;
  },

  addContentSection: async (methodologyId: string): Promise<string> => {
    const res = await adminApi.post(
      `/api/methodology/${methodologyId}/content/sections/add`,
      {
        body: '',
        order: 1,
        type: 'HtmlBlock',
      },
    );
    return res.data.id;
  },

  addTextBlock: async (
    methodologyId: string,
    sectionId: string,
  ): Promise<string> => {
    const res = await adminApi.post(
      `/api/methodology/${methodologyId}/content/section/${sectionId}/blocks/add`,
      {
        type: 'HtmlBlock',
        order: 0,
        body: '',
      },
    );
    const { id } = res.data;
    return id;
  },

  addContent: async (
    methodologyId: string,
    sectionId: string,
    blockId: string,
  ): Promise<void> => {
    await adminApi.put(
      `/api/methodology/${methodologyId}/content/section/${sectionId}/block/${blockId}`,
      {
        body: `<p>${faker.lorem.lines(100)}</p>`,
      },
    );
  },
};
export default methodologyService;
