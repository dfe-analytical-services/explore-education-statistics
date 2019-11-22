import { EditableContentBlock } from '@admin/services/publicationService';
import { TimeIdentifier } from '@common/modules/full-table/services/tableBuilderService';
import { ContactDetails } from '@admin/services/common/types';

export interface BasicLink {
  id: string;
  title: string;
  url: string;
}

export interface ContentSectionViewModel {
  id: string;
  order: number;
  heading: string;
  caption: string;
  content: EditableContentBlock[];
}

export interface ReleaseViewModel {
  id: string;
  typeId?: string;
  timePeriodCoverage: TimeIdentifier;
  publishScheduled?: Date;
  nextReleaseDate: {
    day: string;
    month: string;
    year: string;
  };
  title: string;
  yearTitle: string;
  coverageTitle: string;
  releaseName: string;
  slug: string;
  publicationTitle: string;
  publicationId: string;
  contact: ContactDetails;
}

export interface ManageContentPageViewModel {
  release: ReleaseViewModel;

  previousReleases: BasicLink[];

  releaseNotes: {
    content: string;
    publishedDate: Date;
  }[];

  relatedInformation: BasicLink[];

  introductionSection: ContentSectionViewModel;

  contentSections: ContentSectionViewModel[];
}

export default {};
