import client from '@admin/services/utils/service';
import { GlossaryEntry } from '@common/services/types/glossary';

const glossaryService = {
  getEntry(slug: string): Promise<GlossaryEntry> {
    return client.get(`/glossary-entries/${slug}`);
  },
};

export default glossaryService;
