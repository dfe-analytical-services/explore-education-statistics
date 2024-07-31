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
  getFootnoteMeta(releaseVersionId: string): Promise<FootnoteMeta> {
    return client.get(`/releases/${releaseVersionId}/footnotes-meta`);
  },
  getFootnotes(releaseVersionId: string): Promise<Footnote[]> {
    return client.get(`/releases/${releaseVersionId}/footnotes`);
  },
  getFootnote(releaseVersionId: string, id: string): Promise<Footnote> {
    return client.get(`/releases/${releaseVersionId}/footnotes/${id}`);
  },
  createFootnote(
    releaseVersionId: string,
    footnote: BaseFootnote,
  ): Promise<Footnote> {
    return client.post(
      `/releases/${releaseVersionId}/footnotes`,
      footnoteToFlatFootnote(footnote),
    );
  },
  updateFootnote(
    releaseVersionId: string,
    id: string,
    footnote: BaseFootnote,
  ): Promise<Footnote> {
    return client.put(
      `/releases/${releaseVersionId}/footnotes/${id}`,
      footnoteToFlatFootnote(footnote),
    );
  },
  deleteFootnote(releaseVersionId: string, id: string): Promise<void> {
    return client.delete(`/releases/${releaseVersionId}/footnotes/${id}`);
  },
  updateFootnotesOrder(
    releaseVersionId: string,
    footnoteIds: string[],
  ): Promise<string[]> {
    return client.patch(`/releases/${releaseVersionId}/footnotes/`, {
      footnoteIds,
    });
  },
};

export default footnoteService;
