/* eslint-disable no-console */
import faker from 'faker';
import adminApi from '../utils/adminApi';

const { ADMIN_URL } = process.env;

const methodologyService = {
  createMethodology: async (publicationId: string): Promise<string> => {
    const res = await adminApi.post(
      `/api/publication/${publicationId}/methodology`,
    );
    console.log(
      `Methodology created: ${ADMIN_URL}/methodology/${res.data.id}/summary`,
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
    const { id } = res.data;
    return id;
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
  ): Promise<boolean> => {
    await adminApi.put(
      `/api/methodology/${methodologyId}/content/section/${sectionId}/block/${blockId}`,
      {
        body: `<p>${faker.lorem.lines(100)}</p>`,
      },
    );
    return true;
  },
};
export default methodologyService;
