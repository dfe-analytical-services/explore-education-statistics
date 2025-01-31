/* eslint-disable no-console */
import spinner from '../utils/spinner';
import adminApi from '../utils/adminApi';

const { ADMIN_URL } = process.env;

const methodologyService = {
  createMethodology: async (publicationId: string): Promise<string> => {
    spinner.start();
    const res = await adminApi.post(
      `/api/publication/${publicationId}/methodology`,
    );
    spinner.succeed(
      `Methodology created: ${ADMIN_URL}/methodology/${res.data.id}/summary`,
    );
    return res.data.id;
  },

  addContentSection: async (methodologyId: string): Promise<string> => {
    spinner.start('Adding methodology content section');
    const res = await adminApi.post(
      `/api/methodology/${methodologyId}/content/sections/add`,
      {
        body: '',
        order: 1,
        type: 'HtmlBlock',
      },
    );
    spinner.succeed(`Content section added: ${res.data.id}`);
    return res.data.id;
  },

  addTextBlock: async (
    methodologyId: string,
    sectionId: string,
  ): Promise<string> => {
    spinner.start('Adding methodology text block');
    const res = await adminApi.post(
      `/api/methodology/${methodologyId}/content/section/${sectionId}/blocks/add`,
      {
        type: 'HtmlBlock',
        order: 0,
        body: '',
      },
    );
    const { id } = res.data;
    spinner.succeed(`Methodology content block added: ${id}`);
    return id;
  },

  addContent: async (
    methodologyId: string,
    sectionId: string,
    blockId: string,
  ): Promise<void> => {
    spinner.start('Adding lorem ipsum methodology text content');
    await adminApi.put(
      `/api/methodology/${methodologyId}/content/section/${sectionId}/block/${blockId}`,
      {
        body: `<p>Ad ullamco reprehenderit sunt reprehenderit tempor est proident dolor. Nisi occaecat ut duis qui duis eu exercitation ex aute incididunt nisi fugiat est. Incididunt laboris sit aliqua culpa anim culpa. Fugiat id minim laborum pariatur sint fugiat. Ea.</p>`,
      },
    );
    spinner.succeed('Lorem ipsum content added');
  },
};
export default methodologyService;
