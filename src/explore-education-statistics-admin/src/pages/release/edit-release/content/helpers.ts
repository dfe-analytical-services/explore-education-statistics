import { KeyStatsFormValues } from '@admin/modules/editable-components/EditableKeyStatTile';
import permissionsService from '@admin/services/permissions/service';
import {
  EditableContentBlock,
  ExtendedComment,
  EditableRelease,
} from '@admin/services/publicationService';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import {
  ContentBlockAttachRequest,
  ContentBlockPostModel,
} from '@admin/services/release/edit-release/content/types';
import { Dictionary } from '@admin/types';
import {
  AbstractRelease,
  ContentSection,
} from '@common/services/publicationService';
import { RemoveBlockFromSection } from './actions';
import { Dispatch } from './ReleaseContext';

const contentSectionComments = (
  contentSection?: ContentSection<EditableContentBlock>,
) => {
  if (
    contentSection &&
    contentSection.content &&
    contentSection.content.length > 0
  ) {
    return contentSection.content.reduce<ExtendedComment[]>(
      (allCommentsForSection, content) =>
        content.comments
          ? [...allCommentsForSection, ...content.comments]
          : allCommentsForSection,
      [],
    );
  }

  return [];
};

const getUnresolveComments = (release: AbstractRelease<EditableContentBlock>) =>
  [
    ...contentSectionComments(release.summarySection),
    ...contentSectionComments(release.keyStatisticsSection),
    ...release.content
      .filter(_ => _.content !== undefined)
      .reduce<ExtendedComment[]>(
        (allComments, contentSection) => [
          ...allComments,
          ...contentSectionComments(contentSection),
        ],
        [],
      ),
  ].filter(comment => comment !== undefined && comment.state === 'open');

export async function getReleaseContent(
  dispatch: Dispatch,
  releaseId: string,
  errorHandler: (err: any) => void,
) {
  dispatch({ type: 'CLEAR_STATE' });
  try {
    const {
      release,
      availableDataBlocks,
    } = await releaseContentService.getContent(releaseId);
    const canUpdateRelease = await permissionsService.canUpdateRelease(
      releaseId,
    );
    dispatch({
      type: 'SET_STATE',
      payload: {
        unresolvedComments: getUnresolveComments(release),
        release,
        availableDataBlocks,
        canUpdateRelease,
      },
    });
  } catch (err) {
    errorHandler(err);
  }
}

export async function updateAvailableDataBlocks(
  dispatch: Dispatch,
  releaseId: string,
  errorHandler: (err: any) => void,
) {
  try {
    const availableDataBlocks = await releaseContentService.getAvailableDataBlocks(
      releaseId,
    );
    dispatch({
      type: 'SET_AVAILABLE_DATABLOCKS',
      payload: { availableDataBlocks },
    });
  } catch (err) {
    errorHandler(err);
  }
}

export async function deleteContentSectionBlock(
  dispatch: Dispatch,
  releaseId: string,
  sectionId: string,
  blockId: string,
  sectionKey: RemoveBlockFromSection['payload']['meta']['sectionKey'],
  errorHandler: (err: any) => void,
) {
  try {
    await releaseContentService.deleteContentSectionBlock(
      releaseId,
      sectionId,
      blockId,
    );
    dispatch({
      type: 'REMOVE_BLOCK_FROM_SECTION',
      payload: { meta: { sectionId, blockId, sectionKey } },
    });
    // becuase we don't know if a datablock was removed,
    // and so it is now available again
    updateAvailableDataBlocks(dispatch, releaseId, errorHandler);
  } catch (err) {
    errorHandler(err);
  }
}

export async function updateContentSectionDataBlock(
  dispatch: Dispatch,
  releaseId: string,
  sectionId: string,
  blockId: string,
  sectionKey: RemoveBlockFromSection['payload']['meta']['sectionKey'],
  values: KeyStatsFormValues,
  errorHandler: (err: any) => void,
) {
  try {
    const updateBlock = await releaseContentService.updateContentSectionDataBlock(
      releaseId,
      sectionId,
      blockId,
      values,
    );
    dispatch({
      type: 'UPDATE_BLOCK_FROM_SECTION',
      payload: { meta: { sectionId, blockId, sectionKey }, block: updateBlock },
    });
  } catch (err) {
    errorHandler(err);
  }
}

export async function updateContentSectionBlock(
  dispatch: Dispatch,
  releaseId: string,
  sectionId: string,
  blockId: string,
  sectionKey: RemoveBlockFromSection['payload']['meta']['sectionKey'],
  bodyContent: string,
  errorHandler: (err: any) => void,
) {
  try {
    const updateBlock = await releaseContentService.updateContentSectionBlock(
      releaseId,
      sectionId,
      blockId,
      { body: bodyContent },
    );
    dispatch({
      type: 'UPDATE_BLOCK_FROM_SECTION',
      payload: { meta: { sectionId, blockId, sectionKey }, block: updateBlock },
    });
  } catch (err) {
    errorHandler(err);
  }
}

export async function addContentSectionBlock(
  dispatch: Dispatch,
  releaseId: string,
  sectionId: string,
  sectionKey: RemoveBlockFromSection['payload']['meta']['sectionKey'],
  block: ContentBlockPostModel,
  errorHandler: (err: any) => void,
) {
  try {
    const newBlock = await releaseContentService.addContentSectionBlock(
      releaseId,
      sectionId,
      block,
    );
    dispatch({
      type: 'ADD_BLOCK_TO_SECTION',
      payload: { meta: { sectionId, sectionKey }, block: newBlock },
    });
    // becuase we don't know if a datablock was used,
    // and so it is unavailable
    updateAvailableDataBlocks(dispatch, releaseId, errorHandler);
  } catch (err) {
    errorHandler(err);
  }
}

export async function attachContentSectionBlock(
  dispatch: Dispatch,
  releaseId: string,
  sectionId: string,
  sectionKey: RemoveBlockFromSection['payload']['meta']['sectionKey'],
  block: ContentBlockAttachRequest,
  errorHandler: (err: any) => void,
) {
  try {
    const newBlock = await releaseContentService.attachContentSectionBlock(
      releaseId,
      sectionId,
      block,
    );
    dispatch({
      type: 'ADD_BLOCK_TO_SECTION',
      payload: { meta: { sectionId, sectionKey }, block: newBlock },
    });
    updateAvailableDataBlocks(dispatch, releaseId, errorHandler);
  } catch (err) {
    errorHandler(err);
  }
}

export async function updateSectionBlockOrder(
  dispatch: Dispatch,
  releaseId: string,
  sectionId: string,
  sectionKey: RemoveBlockFromSection['payload']['meta']['sectionKey'],
  order: Dictionary<number>,
  errorHandler: (err: any) => void,
) {
  try {
    const sectionContent = await releaseContentService.updateContentSectionBlocksOrder(
      releaseId,
      sectionId,
      order,
    );
    dispatch({
      type: 'UPDATE_SECTION_CONTENT',
      payload: {
        meta: { sectionId, sectionKey },
        sectionContent,
      },
    });
  } catch (err) {
    errorHandler(err);
  }
}

export async function addContentSection(
  dispatch: Dispatch,
  releaseId: string,
  order: number,
  errorHandler: (err: any) => void,
) {
  try {
    const newSection = await releaseContentService.addContentSection(
      releaseId,
      order,
    );

    dispatch({
      type: 'ADD_CONTENT_SECTION',
      payload: {
        section: newSection,
      },
    });
  } catch (err) {
    errorHandler(err);
  }
}

export async function updateContentSectionsOrder(
  dispatch: Dispatch,
  releaseId: string,
  order: Dictionary<number>,
  errorHandler: (err: any) => void,
) {
  try {
    const content = await releaseContentService.updateContentSectionsOrder(
      releaseId,
      order,
    );

    dispatch({
      type: 'SET_CONTENT',
      payload: {
        content,
      },
    });
  } catch (err) {
    errorHandler(err);
  }
}

export async function removeContentSection(
  dispatch: Dispatch,
  releaseId: string,
  sectionId: string,
  errorHandler: (err: any) => void,
) {
  try {
    const content = await releaseContentService.removeContentSection(
      releaseId,
      sectionId,
    );
    dispatch({
      type: 'SET_CONTENT',
      payload: {
        content,
      },
    });
  } catch (err) {
    errorHandler(err);
  }
}

export async function updateContentSectionHeading(
  dispatch: Dispatch,
  releaseId: string,
  sectionId: string,
  title: string,
  errorHandler: (err: any) => void,
) {
  try {
    const section = await releaseContentService.updateContentSectionHeading(
      releaseId,
      sectionId,
      title,
    );
    dispatch({
      type: 'UPDATE_CONTENT_SECTION',
      payload: {
        meta: { sectionId },
        section,
      },
    });
  } catch (err) {
    errorHandler(err);
  }
}
