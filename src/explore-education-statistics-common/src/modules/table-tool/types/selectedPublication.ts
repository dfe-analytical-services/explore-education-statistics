import { ReleaseType } from '@common/services/types/releaseType';

// TODO: EES-4312 Cleanup this type - use PublicationTreeSummary
export interface SelectedPublication {
  id: string;
  title: string;
  slug: string;
  selectedRelease: SelectedRelease;
  latestRelease: {
    title: string;
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
