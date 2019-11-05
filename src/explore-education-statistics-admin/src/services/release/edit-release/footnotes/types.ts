import { PublicationSubjectMeta } from '@common/modules/full-table/services/tableBuilderService';

export interface FootnoteMeta {
  [key: number /* subjectId */]: {
    subjectId: number;
    subjectName: string;
    indicators: PublicationSubjectMeta['indicators'];
    filters: PublicationSubjectMeta['filters'];
  };
}

export interface FootnoteProps {
  content: string;
  subjects?: number[];
  indicators?: number[];
  filterGroups?: number[];
  filters?: number[];
  filterItems?: number[];
}

export interface Footnote extends FootnoteProps {
  id: string;
}
