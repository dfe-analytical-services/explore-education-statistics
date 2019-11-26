import { EditableContentBlock } from '@admin/services/publicationService';
import { AbstractRelease } from '@common/services/publicationService';

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

export interface ManageContentPageViewModel {
  release: AbstractRelease<EditableContentBlock>;

  relatedInformation: BasicLink[];

  introductionSection: ContentSectionViewModel;

  contentSections: ContentSectionViewModel[];
}

export interface ContentBlockViewModel {
  id: string;
  order?: number;
  type: string;
  body: string;
}

export type ContentBlockPutModel = Pick<ContentBlockViewModel, 'body'>;
export type ContentBlockPostModel = Pick<
  ContentBlockViewModel,
  'order' | 'type' | 'body'
>;

export default {};
