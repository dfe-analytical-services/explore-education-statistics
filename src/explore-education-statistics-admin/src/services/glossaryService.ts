import client from '@admin/services/utils/service';
import { GlossaryEntry } from '@common/services/types/glossary';

const glossaryService = {
  getGlossaryEntry(slug: string): Promise<GlossaryEntry> {
    return client.get(`/glossary/${slug}`, {});
  },
};

export default glossaryService;
