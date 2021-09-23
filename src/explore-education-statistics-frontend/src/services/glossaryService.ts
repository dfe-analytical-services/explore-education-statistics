import { contentApi } from '@common/services/api';
import {
  GlossaryCategory,
  GlossaryEntry,
} from '@common/services/types/glossary';

const glossaryService = {
  listGlossaryEntries(): Promise<GlossaryCategory[]> {
    return contentApi.get('/glossary', {});
  },
  getGlossaryEntry(slug: string): Promise<GlossaryEntry> {
    return contentApi.get(`glossary/${slug}`, {});
  },
};

export default glossaryService;
