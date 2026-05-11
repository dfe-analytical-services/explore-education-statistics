import educationInNumbersContentService, {
  EinContentBlockAddRequest,
} from '@admin/services/educationInNumbersContentService';
import {
  EinBlockType,
  EinFreeTextStatTile,
  EinTile,
  EinTileType,
} from '@common/services/types/einBlocks';
import { useEducationInNumbersPageContentDispatch } from './EducationInNumbersPageContentContext';
import { FreeTextStatTileFormValues } from '../components/EditableFreeTextStatTileForm';

export default function useEducationInNumbersPageContentActions() {
  const dispatch = useEducationInNumbersPageContentDispatch();

  async function deleteContentSectionBlock({
    educationInNumbersPageId,
    sectionId,
    blockId,
  }: {
    educationInNumbersPageId: string;
    sectionId: string;
    blockId: string;
  }) {
    await educationInNumbersContentService.deleteContentSectionBlock({
      educationInNumbersPageId,
      sectionId,
      blockId,
    });
    dispatch({
      type: 'REMOVE_BLOCK_FROM_SECTION',
      payload: {
        meta: {
          sectionId,
          blockId,
        },
      },
    });
  }

  async function updateContentSectionBlock({
    educationInNumbersPageId,
    sectionId,
    blockId,
    content,
    type,
  }: {
    educationInNumbersPageId: string;
    sectionId: string;
    blockId: string;
    content: string;
    type: EinBlockType;
  }) {
    const updateBlock =
      type === 'HtmlBlock'
        ? await educationInNumbersContentService.updateContentSectionHtmlBlock({
            educationInNumbersPageId,
            sectionId,
            blockId,
            block: { body: content },
          })
        : await educationInNumbersContentService.updateContentSectionGroupBlock(
            {
              educationInNumbersPageId,
              sectionId,
              blockId,
              block: { title: content },
            },
          );
    dispatch({
      type: 'UPDATE_BLOCK_FROM_SECTION',
      payload: {
        meta: {
          sectionId,
          blockId,
        },
        block: updateBlock,
      },
    });
  }

  async function addContentSectionBlock({
    educationInNumbersPageId,
    sectionId,
    block,
  }: {
    educationInNumbersPageId: string;
    sectionId: string;
    block: EinContentBlockAddRequest;
  }) {
    const newBlock =
      await educationInNumbersContentService.addContentSectionBlock({
        educationInNumbersPageId,
        sectionId,
        block,
      });
    dispatch({
      type: 'ADD_BLOCK_TO_SECTION',
      payload: {
        meta: { sectionId },
        block: newBlock,
      },
    });
    return newBlock;
  }

  async function updateSectionBlockOrder({
    educationInNumbersPageId,
    sectionId,
    order,
  }: {
    educationInNumbersPageId: string;
    sectionId: string;
    order: string[];
  }) {
    const sectionBlocks =
      await educationInNumbersContentService.updateContentSectionBlocksOrder({
        educationInNumbersPageId,
        sectionId,
        order,
      });
    dispatch({
      type: 'UPDATE_SECTION_CONTENT',
      payload: {
        meta: { sectionId },
        sectionContent: sectionBlocks,
      },
    });
  }

  // NOTE: `order` could be removed from addContentSection as an argument, as
  // we only ever add new sections to the end of all existing sections. If
  // someone ever did provide an order that clashed with an existing section,
  // currently the frontend doesn't adjust the `order`s of existing sections.
  async function addContentSection({
    educationInNumbersPageId,
    order,
  }: {
    educationInNumbersPageId: string;
    order: number;
  }) {
    const newSection = await educationInNumbersContentService.addContentSection(
      {
        educationInNumbersPageId,
        order,
      },
    );
    dispatch({
      type: 'ADD_CONTENT_SECTION',
      payload: {
        section: newSection,
      },
    });
    return newSection;
  }

  async function updateContentSectionsOrder({
    educationInNumbersPageId,
    order,
  }: {
    educationInNumbersPageId: string;
    order: string[];
  }) {
    const content =
      await educationInNumbersContentService.updateContentSectionsOrder({
        educationInNumbersPageId,
        order,
      });
    dispatch({
      type: 'SET_CONTENT',
      payload: {
        content,
      },
    });
  }

  async function removeContentSection({
    educationInNumbersPageId,
    sectionId,
  }: {
    educationInNumbersPageId: string;
    sectionId: string;
  }) {
    const content = await educationInNumbersContentService.removeContentSection(
      {
        educationInNumbersPageId,
        sectionId,
      },
    );
    dispatch({
      type: 'SET_CONTENT',
      payload: {
        content,
      },
    });

    return content;
  }

  async function updateContentSectionHeading({
    educationInNumbersPageId,
    sectionId,
    heading,
  }: {
    educationInNumbersPageId: string;
    sectionId: string;
    heading: string;
  }) {
    const section =
      await educationInNumbersContentService.updateContentSectionHeading({
        educationInNumbersPageId,
        sectionId,
        heading,
      });

    dispatch({
      type: 'UPDATE_CONTENT_SECTION',
      payload: {
        meta: { sectionId },
        section,
      },
    });
  }

  async function addTile({
    educationInNumbersPageId,
    sectionId,
    blockId,
    type,
  }: {
    educationInNumbersPageId: string;
    sectionId: string;
    blockId: string;
    type: EinTileType;
  }) {
    const newTile = await educationInNumbersContentService.addTile({
      educationInNumbersPageId,
      blockId,
      type,
    });
    dispatch({
      type: 'ADD_TILE_TO_GROUP_BLOCK',
      payload: {
        meta: { sectionId, blockId },
        tile: newTile,
      },
    });
    return newTile;
  }

  async function reorderTiles({
    educationInNumbersPageId,
    sectionId,
    blockId,
    tiles,
  }: {
    educationInNumbersPageId: string;
    sectionId: string;
    blockId: string;
    tiles: EinTile[];
  }) {
    const newTiles =
      await educationInNumbersContentService.reorderGroupBlockTiles({
        educationInNumbersPageId,
        blockId,
        order: tiles.map(tile => tile.id),
      });
    dispatch({
      type: 'REORDER_TILES_IN_GROUP_BLOCK',
      payload: {
        meta: { sectionId, blockId },
        tiles: newTiles,
      },
    });
  }

  async function deleteTile({
    educationInNumbersPageId,
    sectionId,
    blockId,
    tileId,
  }: {
    educationInNumbersPageId: string;
    sectionId: string;
    blockId: string;
    tileId: string;
  }) {
    await educationInNumbersContentService.deleteTile({
      educationInNumbersPageId,
      blockId,
      tileId,
    });
    dispatch({
      type: 'DELETE_TILE_FROM_GROUP_BLOCK',
      payload: {
        meta: { sectionId, blockId, tileId },
      },
    });
  }

  async function updateFreeTextStatTile({
    educationInNumbersPageId,
    sectionId,
    blockId,
    tileId,
    values,
  }: {
    educationInNumbersPageId: string;
    sectionId: string;
    blockId: string;
    tileId: string;
    values: FreeTextStatTileFormValues;
  }) {
    const newTile =
      await educationInNumbersContentService.updateFreeTextStatTile({
        educationInNumbersPageId,
        tileId,
        values,
      });
    dispatch({
      type: 'UPDATE_FREE_TEXT_STAT_TILE_IN_BLOCK',
      payload: {
        meta: { sectionId, blockId, tileId },
        tile: newTile,
      },
    });
    return newTile;
  }

  return {
    addTile,
    reorderTiles,
    deleteTile,
    updateFreeTextStatTile,
    deleteContentSectionBlock,
    updateContentSectionBlock,
    addContentSectionBlock,
    updateSectionBlockOrder,
    addContentSection,
    updateContentSectionsOrder,
    removeContentSection,
    updateContentSectionHeading,
  };
}
