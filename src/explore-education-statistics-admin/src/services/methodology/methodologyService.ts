import {
  CreateMethodologyRequest,
  MethodologyContent,
  MethodologyStatusListItem,
  UpdateMethodologyStatusRequest,
} from '@admin/services/methodology/types';
import {
  ContentSectionViewModel,
  ContentBlockPutModel,
  ContentBlockPostModel,
} from '@admin/services/release/edit-release/content/types';
import client from '@admin/services/util/service';
import { Dictionary } from '@common/types';
import {
  BasicMethodology,
  IdTitlePair,
  MethodologyStatus,
} from '../common/types';
import { EditableContentBlock } from '../publicationService';

const methodologyService = {
  getMethodologies(): Promise<MethodologyStatusListItem[]> {
    return client.get<MethodologyStatusListItem[]>('/bau/methodology');
  },

  createMethodology(
    createRequest: CreateMethodologyRequest,
  ): Promise<IdTitlePair> {
    return client.post(`/methodologies/`, createRequest);
  },

  getMethodologyContent(methodologyId: string): Promise<MethodologyContent> {
    return client.get(`methodology/${methodologyId}/content`);
  },

  addContentSection({
    methodologyId,
    order,
    isAnnexes = false,
  }: {
    methodologyId: string;
    order: number;
    isAnnexes?: boolean;
  }): Promise<ContentSectionViewModel> {
    return client.post(
      `methodology/${methodologyId}/content/sections/add${
        isAnnexes ? '?type=annexes' : ''
      }`,
      { order },
    );
  },

  updateContentSectionHeading(
    methodologyId: string,
    sectionId: string,
    heading: string,
    isAnnexes = false,
  ): Promise<ContentSectionViewModel> {
    return client.put(
      `methodology/${methodologyId}/content/section/${sectionId}/heading${
        isAnnexes ? '?type=annexes' : ''
      }`,
      { heading },
    );
  },

  updateContentSectionsOrder({
    methodologyId,
    order,
    isAnnexes = false,
  }: {
    methodologyId: string;
    order: Dictionary<number>;
    isAnnexes?: boolean;
  }): Promise<ContentSectionViewModel[]> {
    return client.put(
      `methodology/${methodologyId}/content/sections/order${
        isAnnexes ? '?type=annexes' : ''
      }`,
      order,
    );
  },

  removeContentSection(
    methodologyId: string,
    sectionId: string,
    isAnnexes = false,
  ): Promise<ContentSectionViewModel[]> {
    return client.delete(
      `methodology/${methodologyId}/content/section/${sectionId}${
        isAnnexes ? '?type=annexes' : ''
      }`,
    );
  },

  getMethodologyStatus: (methodologyId: string): Promise<MethodologyStatus> =>
    client
      .get<BasicMethodology>(`/methodology/${methodologyId}/summary`)
      .then(methodology => methodology.status),

  updateMethodologyStatus(
    methodologyId: string,
    updateRequest: UpdateMethodologyStatusRequest,
  ): Promise<BasicMethodology> {
    return client.put(`/methodology/${methodologyId}/status`, updateRequest);
  },

  addContentSectionBlock(
    methodologyId: string,
    sectionId: string,
    block: ContentBlockPostModel,
    isAnnexes = false,
  ): Promise<EditableContentBlock> {
    return client.post<EditableContentBlock>(
      `/methodology/${methodologyId}/content/section/${sectionId}/blocks/add${
        isAnnexes ? '?type=annexes' : ''
      }`,
      block,
    );
  },

  deleteContentSectionBlock(
    methodologyId: string,
    sectionId: string,
    blockId: string,
    isAnnexes = false,
  ): Promise<ContentSectionViewModel[]> {
    return client.delete(
      `methodology/${methodologyId}/content/section/${sectionId}/block/${blockId}${
        isAnnexes ? '?type=annexes' : ''
      }`,
    );
  },

  updateContentSectionBlock(
    methodologyId: string,
    sectionId: string,
    blockId: string,
    block: ContentBlockPutModel,
    isAnnexes = false,
  ): Promise<EditableContentBlock> {
    return client.put(
      `methodology/${methodologyId}/content/section/${sectionId}/block/${blockId}${
        isAnnexes ? '?type=annexes' : ''
      }`,
      block,
    );
  },

  updateContentSectionBlocksOrder(
    methodologyId: string,
    sectionId: string,
    order: Dictionary<number>,
    isAnnexes = false,
  ): Promise<EditableContentBlock[]> {
    return client.put<EditableContentBlock[]>(
      `methodology/${methodologyId}/content/section/${sectionId}/blocks/order${
        isAnnexes ? '?type=annexes' : ''
      }`,
      order,
    );
  },
};

export default methodologyService;
