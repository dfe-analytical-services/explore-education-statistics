import client from '@admin/services/utils/service';
import footnoteToFlatFootnote from './utils/footnote/footnoteToFlatFootnote';

export interface FootnoteSubject {
  selected: boolean;
  indicatorGroups: {
    [key: string]: {
      selected: boolean;
      indicators: string[];
    };
  };
  filters: {
    [key: string]: {
      selected: boolean;
      filterGroups: {
        [key: string]: {
          selected: boolean;
          filterItems: string[];
        };
      };
    };
  };
}

export interface BaseFootnote {
  content: string;
  subjects: {
    [key: string]: FootnoteSubject;
  };
}

export interface Footnote extends BaseFootnote {
  id: string;
}

export interface FootnoteSubjectMeta {
  subjectId: string;
  subjectName: string;
  indicators: {
    [key: string]: {
      // indicator "group"
      label: string;
      options: {
        [key: string]: {
          // indicator "item"
          label: string;
          unit: string;
          value: string;
        };
      };
    };
  };
  filters: {
    [key: string]: {
      // filter
      hint: string;
      legend: string;
      options: {
        [key: string]: {
          label: string;
          // filterGroup
          options: {
            [key: string]: {
              // filterItem
              label: string;
              value: string;
            };
          };
        };
      };
    };
  };
}

export interface FootnoteMeta {
  [key: string /* subjectId */]: FootnoteSubjectMeta;
}

const footnoteService = {
  async getReleaseFootnoteData(
    releaseId: string,
  ): Promise<{ meta: FootnoteMeta; footnotes: Footnote[] }> {
    return client.get(`/data/footnote/release/${releaseId}`);
  },
  async createFootnote(releaseId: string, footnote: BaseFootnote): Promise<Footnote> {
    return client.post(`/data/footnote/release/${releaseId}`, footnoteToFlatFootnote(footnote));
  },
  async getFootnote(id: string) {
    return client.get(`/data/footnote/${id}`);
  },
  async updateFootnote(releaseId: string, id: string, footnote: BaseFootnote): Promise<Footnote> {
    return client.put(`/data/footnote/release/${releaseId}/${id}`, footnoteToFlatFootnote(footnote));
  },
  async deleteFootnote(releaseId: string, id: string): Promise<void> {
    return client.delete(`/data/footnote/release/${releaseId}/${id}`);
  },
};

export default footnoteService;
