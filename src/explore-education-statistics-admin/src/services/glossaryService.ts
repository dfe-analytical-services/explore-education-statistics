import client from '@admin/services/utils/service';
import {
  GlossaryCategory,
  GlossaryEntry,
} from '@common/services/types/glossary';

const glossaryService = {
  listEntries(): Promise<GlossaryCategory[]> {
    return client.get('/glossary-entries');
  },
  getEntry(slug: string): Promise<GlossaryEntry> {
    return client.get(`/glossary-entries/${slug}`);
  },
};

export default glossaryService;
