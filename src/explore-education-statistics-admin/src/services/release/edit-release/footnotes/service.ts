import client from '@admin/services/util/service';

const service = {
  async deleteFootnote(id: number) {
    return client.delete(`/footnotes/${id}`);
  },
};

export default service;
