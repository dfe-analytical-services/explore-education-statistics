import client from '@admin/services/util/service';

const service = {
  async deleteFootnote(id: string) {
    return client.delete(`/footnotes/${id}`);
  },
};

export default service;
