import { KeyStatsFormValues } from '@admin/components/editable/EditableKeyStatTile';
import {
  EditableBlock,
  EditableContentBlock,
  EditableDataBlock,
  ExtendedComment,
} from '@admin/services/publicationService';
import client from '@admin/services/util/service';
import {
  BasicLink,
  ContentSection,
  Release,
  ReleaseNote,
} from '@common/services/publicationService';
import { DataBlock } from '@common/services/types/blocks';
import { Dictionary } from '@common/types/util';
import {
  ContentBlockAttachRequest,
  ContentBlockPostModel,
  ContentBlockPutModel,
  ManageContentPageViewModel,
} from './types';

type ContentSectionViewModel = ContentSection<EditableBlock>;

export const releaseContentService = {
  getContent(releaseId: string): Promise<ManageContentPageViewModel> {
    return client.get<ManageContentPageViewModel>(
      `/release/${releaseId}/content`,
    );
  },
  getContentSections(releaseId: string): Promise<ContentSectionViewModel[]> {
    return client.get<ContentSectionViewModel[]>(
      `/release/${releaseId}/content/sections`,
    );
  },
  addContentSection(
    releaseId: string,
    order: number,
  ): Promise<ContentSectionViewModel> {
    return client.post<ContentSectionViewModel>(
      `/release/${releaseId}/content/sections/add`,
      { order },
    );
  },
  updateContentSectionsOrder(
    releaseId: string,
    order: Dictionary<number>,
  ): Promise<ContentSectionViewModel[]> {
    return client.put<ContentSectionViewModel[]>(
      `/release/${releaseId}/content/sections/order`,
      order,
    );
  },
  removeContentSection(
    releaseId: string,
    sectionId: string,
  ): Promise<ContentSectionViewModel[]> {
    return client.delete<ContentSectionViewModel[]>(
      `/release/${releaseId}/content/section/${sectionId}`,
    );
  },

  getContentSection(
    releaseId: string,
    sectionId: string,
  ): Promise<ContentSectionViewModel> {
    return client.get<ContentSectionViewModel>(
      `/release/${releaseId}/content/section/${sectionId}`,
    );
  },

  addContentSectionBlock(
    releaseId: string,
    sectionId: string,
    block: ContentBlockPostModel,
  ): Promise<EditableContentBlock> {
    return client.post<EditableContentBlock>(
      `/release/${releaseId}/content/section/${sectionId}/blocks/add`,
      block,
    );
  },

  updateContentSectionHeading(
    releaseId: string,
    sectionId: string,
    heading: string,
  ): Promise<ContentSectionViewModel> {
    return client.put<ContentSectionViewModel>(
      `/release/${releaseId}/content/section/${sectionId}/heading`,
      { heading },
    );
  },

  updateContentSectionBlock(
    releaseId: string,
    sectionId: string,
    blockId: string,
    block: ContentBlockPutModel,
  ): Promise<EditableContentBlock> {
    return client.put<EditableContentBlock>(
      `/release/${releaseId}/content/section/${sectionId}/block/${blockId}`,
      block,
    );
  },

  updateContentSectionDataBlock(
    releaseId: string,
    contentSectionId: string,
    contentBlockId: string,
    newSummary: KeyStatsFormValues,
  ): Promise<EditableDataBlock> {
    return client.put<EditableDataBlock>(
      `/release/${releaseId}/content/section/${contentSectionId}/data-block/${contentBlockId}`,
      newSummary,
    );
  },

  updateContentSectionBlocksOrder(
    releaseId: string,
    sectionId: string,
    order: Dictionary<number>,
  ): Promise<EditableBlock[]> {
    return client.put<EditableBlock[]>(
      `/release/${releaseId}/content/section/${sectionId}/blocks/order`,
      order,
    );
  },

  deleteContentSectionBlock(
    releaseId: string,
    sectionId: string,
    blockId: string,
  ): Promise<void> {
    return client.delete(
      `/release/${releaseId}/content/section/${sectionId}/block/${blockId}`,
    );
  },

  getContentSectionComments(
    releaseId: string,
    sectionId: string,
    contentBlockId: string,
  ): Promise<ExtendedComment[]> {
    return client.get(
      `/release/${releaseId}/content/section/${sectionId}/block/${contentBlockId}/comments`,
    );
  },

  addContentSectionComment(
    releaseId: string,
    sectionId: string,
    contentBlockId: string,
    comment: ExtendedComment,
  ): Promise<ExtendedComment> {
    return client.post(
      `/release/${releaseId}/content/section/${sectionId}/block/${contentBlockId}/comments/add`,
      comment,
    );
  },

  updateContentSectionComment(
    releaseId: string,
    sectionId: string,
    contentBlockId: string,
    comment: ExtendedComment,
  ): Promise<ExtendedComment> {
    return client.put(
      `/release/${releaseId}/content/section/${sectionId}/block/${contentBlockId}/comment/${comment.id}`,
      comment,
    );
  },

  deleteContentSectionComment(
    releaseId: string,
    sectionId: string,
    contentBlockId: string,
    commentId: string,
  ): Promise<void> {
    return client.delete(
      `/release/${releaseId}/content/section/${sectionId}/block/${contentBlockId}/comment/${commentId}`,
    );
  },

  getAvailableDataBlocks(releaseId: string): Promise<DataBlock[]> {
    return client.get(`/release/${releaseId}/content/available-datablocks`);
  },

  attachContentSectionBlock(
    releaseId: string,
    sectionId: string,
    block: ContentBlockAttachRequest,
  ): Promise<EditableBlock> {
    return client.post<EditableBlock>(
      `/release/${releaseId}/content/section/${sectionId}/blocks/attach`,
      block,
    );
  },
};

export const releaseNoteService = {
  create: (
    releaseId: string,
    releaseNote: Omit<ReleaseNote, 'id' | 'on' | 'releaseId'>,
  ): Promise<ReleaseNote[]> => {
    return client.post(
      `/release/${releaseId}/content/release-note`,
      releaseNote,
    );
  },
  delete: (id: string, releaseId: string): Promise<ReleaseNote[]> => {
    return client.delete(`/release/${releaseId}/content/release-note/${id}`);
  },
  edit: (
    id: string,
    releaseId: string,
    releaseNote: Omit<ReleaseNote, 'id' | 'releaseId'>,
  ): Promise<ReleaseNote[]> => {
    return client.put(
      `/release/${releaseId}/content/release-note/${id}`,
      releaseNote,
    );
  },
};

export const relatedInformationService = {
  getAll: (releaseId: string): Promise<Release['relatedInformation']> => {
    return client.get(`/release/${releaseId}/content/related-information`);
  },
  create: (
    releaseId: string,
    link: Omit<BasicLink, 'id'>,
  ): Promise<BasicLink[]> => {
    return client.post(
      `/release/${releaseId}/content/related-information`,
      link,
    );
  },
  update: (
    releaseId: string,
    link: { id: string; description: string; url: string },
  ): Promise<BasicLink[]> => {
    return client.put(
      `/release/${releaseId}/content/related-information/${link.id}`,
      link,
    );
  },
  delete: (releaseId: string, linkId: string): Promise<BasicLink[]> => {
    return client.delete(
      `/release/${releaseId}/content/related-information/${linkId}`,
    );
  },
};

export default {};
