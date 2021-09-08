import { contentApi } from '@common/services/api';

export interface GlossaryEntry {
  title: string;
  slug: string;
  body: string;
}

export interface GlossaryCategory {
  heading: string;
  entries: GlossaryEntry[];
}

const glossaryService = {
  listGlossaryEntries(): Promise<GlossaryCategory[]> {
    return contentApi.get('/glossary', {});
  },
};

export default glossaryService;
