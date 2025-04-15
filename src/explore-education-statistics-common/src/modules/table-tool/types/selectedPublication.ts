import { PublicationTreeSummary } from '@common/services/publicationService';
import { ReleaseType } from '@common/services/types/releaseType';

export interface SelectedPublication extends PublicationTreeSummary {
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
  title: string;
  type: ReleaseType;
}
