import client from '@admin/services/utils/service';
import footnoteToFlatFootnote from './utils/footnote/footnoteToFlatFootnote';

export type SubjectSelectionType = 'NA' | 'All' | 'Specific';

export interface FootnoteSubject {
  selectionType: SubjectSelectionType;
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
        // indicator "item"
        label: string;
        unit: string;
        value: string;
      }[];
    };
  };
  filters: {
    [key: string]: {
      // filter
      hint: string;
      legend: string;
      options: {
        // filterGroup
        [key: string]: {
          label: string;
          options: {
            // filterItem
            label: string;
            value: string;
          }[];
        };
      };
    };
  };
}

export interface FootnoteMeta {
  subjects: {
    [subjectId: string]: FootnoteSubjectMeta;
  };
}

const footnoteService = {
  getFootnoteMeta(releaseId: string): Promise<FootnoteMeta> {
    return client.get(`/releases/${releaseId}/footnotes-meta`);
  },
  getFootnotes(releaseId: string): Promise<Footnote[]> {
    return client.get(`/releases/${releaseId}/footnotes`);
  },
  getFootnote(releaseId: string, id: string): Promise<Footnote> {
    return client.get(`/releases/${releaseId}/footnotes/${id}`);
  },
  createFootnote(releaseId: string, footnote: BaseFootnote): Promise<Footnote> {
    return client.post(
      `/releases/${releaseId}/footnotes`,
      footnoteToFlatFootnote(footnote),
    );
  },
  updateFootnote(
    releaseId: string,
    id: string,
    footnote: BaseFootnote,
  ): Promise<Footnote> {
    return client.put(
      `/releases/${releaseId}/footnotes/${id}`,
      footnoteToFlatFootnote(footnote),
    );
  },
  deleteFootnote(releaseId: string, id: string): Promise<void> {
    return client.delete(`/releases/${releaseId}/footnotes/${id}`);
  },
  updateFootnotesOrder(
    releaseId: string,
    footnoteIds: string[],
  ): Promise<string[]> {
    return client.patch(`/releases/${releaseId}/footnotes/`, {
      footnoteIds,
    });
  },
};

export default footnoteService;
