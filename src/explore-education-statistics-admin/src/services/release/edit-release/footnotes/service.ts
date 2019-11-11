import client from '@admin/services/util/service';
import { FootnoteProps, Footnote, FootnoteMeta } from './types';

const service = {
  async getReleaseFootnoteData(
    releaseId: string,
  ): Promise<{ meta: FootnoteMeta; footnotes: Footnote[] }> {
    return client.get(`/data/footnote/release/${releaseId}`);
  },
  async createFootnote(footnote: FootnoteProps): Promise<Footnote> {
    return client.post(`/data/footnote`, footnote);
  },
  async getFootnote(id: number) {
    return client.get(`/data/footnote/${id}`);
  },
  async updateFootnote(id: number, footnote: FootnoteProps | Footnote) {
    return client.put(`/data/footnote/${id}`, footnote);
  },
  async deleteFootnote(id: number): Promise<void> {
    return client.delete(`/data/footnote/${id}`);
  },
};

export default service;
