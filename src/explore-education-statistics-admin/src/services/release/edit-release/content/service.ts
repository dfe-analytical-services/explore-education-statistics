import client from '@admin/services/util/service';
import {
  AbstractRelease,
  BasicLink,
  ReleaseNote,
} from '@common/services/publicationService';
import { Dictionary } from '@common/types/util';
import {
  ContentBlockPostModel,
  ContentBlockPutModel,
  ContentBlockViewModel,
  ContentSectionViewModel,
  ManageContentPageViewModel,
} from './types';

export interface ReleaseContentService {
  getContent: (releaseId: string) => Promise<ManageContentPageViewModel>;
  getContentSections: (releaseId: string) => Promise<ContentSectionViewModel[]>;
  updateContentSectionsOrder: (
    releaseId: string,
    order: Dictionary<number>,
  ) => Promise<ContentSectionViewModel[]>;

  getContentSection: (
    releaseId: string,
    sectionId: string,
  ) => Promise<ContentSectionViewModel>;

  addContentSectionBlock: (
    releaseId: string,
    sectionId: string,
    block: ContentBlockPostModel,
  ) => Promise<ContentBlockViewModel>;

  updateContentSectionBlock: (
    releaseId: string,
    sectionId: string,
    contentBlockId: string,
    block: ContentBlockPutModel,
  ) => Promise<ContentBlockViewModel>;

  deleteContentSectionBlock: (
    releaseId: string,
    sectionId: string,
    contentBlockId: string,
  ) => Promise<void>;
}

const service: ReleaseContentService = {
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
  updateContentSectionsOrder(
    releaseId: string,
    order: Dictionary<number>,
  ): Promise<ContentSectionViewModel[]> {
    return client.put<ContentSectionViewModel[]>(
      `/release/${releaseId}/content/sections/order`,
      order,
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
  ): Promise<ContentBlockViewModel> {
    return client.post<ContentBlockViewModel>(
      `/release/${releaseId}/content/section/${sectionId}/blocks/add`,
      block,
    );
  },

  updateContentSectionBlock(
    releaseId: string,
    sectionId: string,
    blockId: string,
    block: ContentBlockPutModel,
  ): Promise<ContentBlockViewModel> {
    return client.put<ContentBlockViewModel>(
      `/release/${releaseId}/content/section/${sectionId}/block/${blockId}`,
      block,
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
};

const releaseNoteService = {
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

const relatedInformationService = {
  getAll: (
    releaseId: string,
  ): Promise<AbstractRelease<{}>['relatedInformation']> => {
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

export default {
  ...service,
  releaseNote: releaseNoteService,
  relatedInfo: relatedInformationService,
};
