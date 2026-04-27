import {
  Contact,
  PublicationTreeSummary,
} from '@common/services/publicationService';
import { Organisation } from '@common/services/types/organisation';
import { ReleaseType } from '@common/services/types/releaseType';

export interface SelectedPublication extends PublicationTreeSummary {
  contact: Contact;
  selectedRelease: SelectedRelease;
  latestRelease: {
    title: string;
    slug: string;
  };
}

// TODO: EES-4312 Cleanup this type - use PublicationReleaseSummary
export interface SelectedRelease {
  id: string;
  slug: string;
  latestData: boolean;
  publishingOrganisations?: Organisation[];
  title: string;
  type: ReleaseType;
}
