import client from '@admin/services/util/service';
import { FootnoteProps, Footnote, FootnoteMeta } from './types';
import { footnoteToFlatFootnote } from './util';

const service = {
  async getReleaseFootnoteData(
    releaseId: string,
  ): Promise<{ meta: FootnoteMeta; footnotes: Footnote[] }> {
    return client.get(`/data/footnote/release/${releaseId}`);
  },
  async createFootnote(footnote: FootnoteProps): Promise<Footnote> {
    return client.post(`/data/footnote`, footnoteToFlatFootnote(footnote));
  },
  async getFootnote(id: string) {
    return client.get(`/data/footnote/${id}`);
  },
  async updateFootnote(id: string, footnote: FootnoteProps | Footnote) {
    return client.put(`/data/footnote/${id}`, footnoteToFlatFootnote(footnote));
  },
  async deleteFootnote(id: string): Promise<void> {
    return client.delete(`/data/footnote/${id}`);
  },
};

export default service;
