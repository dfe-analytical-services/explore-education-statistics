import client from '@admin/services/util/service';
import { FootnoteProps, Footnote, FootnoteMeta } from './types';
import { footnoteToFlatFootnote } from './util';

const service = {
  async getReleaseFootnoteData(
    releaseId: string,
  ): Promise<{ meta: FootnoteMeta; footnotes: Footnote[] }> {
    return client.get(`/data/footnote/release/${releaseId}`);
  },
  async createFootnote(
    releaseId: string,
    footnote: FootnoteProps,
  ): Promise<Footnote> {
    return client.post(
      `/data/footnote/release/${releaseId}`,
      footnoteToFlatFootnote(footnote),
    );
  },
  async updateFootnote(
    releaseId: string,
    id: string,
    footnote: FootnoteProps | Footnote,
  ): Promise<Footnote> {
    return client.put(
      `/data/footnote/release/${releaseId}/${id}`,
      footnoteToFlatFootnote(footnote),
    );
  },
  async deleteFootnote(releaseId: string, id: string): Promise<void> {
    return client.delete(`/data/footnote/release/${releaseId}/${id}`);
  },
};

export default service;
