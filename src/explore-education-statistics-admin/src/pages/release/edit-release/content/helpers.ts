import permissionsService from '@admin/services/permissions/service';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import {
  AbstractRelease,
  ContentSection,
} from '@common/services/publicationService';
import {
  EditableContentBlock,
  ExtendedComment,
} from 'src/services/publicationService';
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
  errorHandler?: (err: any) => void,
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
    if (errorHandler) {
      errorHandler(err);
    } else {
      dispatch({
        type: 'PAGE_ERROR',
        payload: { pageError: 'There was a problem.' },
      });
    }
  }
}

export async function updateAvailableDataBlocks(
  dispatch: Dispatch,
  releaseId: string,
  errorHandler?: (err: any) => void,
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
    if (errorHandler) {
      errorHandler(err);
    } else {
      dispatch({
        type: 'PAGE_ERROR',
        payload: { pageError: 'There was a problem.' },
      });
    }
  }
}

export async function deleteContentSectionBlock(
  dispatch: Dispatch,
  releaseId: string,
  sectionId: string,
  blockId: string,
  sectionKey: RemoveBlockFromSection['payload']['meta']['sectionKey'],
  errorHandler?: (err: any) => void,
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
    if (errorHandler) {
      errorHandler(err);
    } else {
      dispatch({
        type: 'PAGE_ERROR',
        payload: { pageError: 'There was a problem.' },
      });
    }
  }
}

export async function updateContentSectionDataBlock(
  dispatch: Dispatch,
  releaseId: string,
  sectionId: string,
  blockId: string,
  sectionKey: RemoveBlockFromSection['payload']['meta']['sectionKey'],
  errorHandler?: (err: any) => void,
) {
  // await releaseContentService.updateContentSectionDataBlock(
  //                       releaseId,
  //                       sectionId,
  //                       blockId,
  //                       values,
  //                     )
  //                     .then(updatedBlock => {
  //                       setRelease({
  //                         ...release,
  //                         keyStatisticsSection: {
  //                           ...release.keyStatisticsSection,
  //                           content: release.keyStatisticsSection.content
  //                             ? release.keyStatisticsSection.content.map(
  //                                 contentBlock => {
  //                                   if (contentBlock.id === updatedBlock.id) {
  //                                     return updatedBlock;
  //                                   }
  //                                   return contentBlock;
  //                                 },
  //                               )
  //                             : [],
  //                         },
  //                       });
  //                       resolve();
  //                     })
  //                     .catch(handleApiErrors),
}
