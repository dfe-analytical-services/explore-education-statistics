import { contentApi } from '@common/services/api';
import {
  GlossaryCategory,
  GlossaryEntry,
} from '@common/services/types/glossary';

const glossaryService = {
  listEntries(): Promise<GlossaryCategory[]> {
    return contentApi.get('/glossary-entries');
  },
  getEntry(slug: string): Promise<GlossaryEntry> {
    return contentApi.get(`/glossary-entries/${slug}`);
  },
};

export default glossaryService;
