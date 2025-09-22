import testEinPageVersion, {
  testEinPageContent,
} from '@admin/pages/education-in-numbers/__tests__/__data__/testEducationInNumbersPageAndContent';
import {
  EducationInNumbersPageContextState,
  educationInNumbersPageReducer as originalEinPageReducer,
} from '@admin/pages/education-in-numbers/content/context/EducationInNumbersPageContentContext';
import { EducationInNumbersPageDispatchAction } from '@admin/pages/education-in-numbers/content/context/EducationInNumbersPageContentContextActionTypes';
import {
  EinContentBlock,
  EinFreeTextStatTile,
  EinHtmlBlock,
  EinTileGroupBlock,
} from '@common/services/types/einBlocks';
import { produce } from 'immer';
import testTile from '../../__tests__/__data__/testTile';

const einPageReducer = (
  initial: EducationInNumbersPageContextState,
  action: EducationInNumbersPageDispatchAction,
) => produce(initial, draft => originalEinPageReducer(draft, action));

describe('EducationInNumbersPageContentContext', () => {
  test('REMOVE_BLOCK_FROM_SECTION removes a block from a section', () => {
    const keyStatsSection = testEinPageContent.content[0];
    const removingBlockId = keyStatsSection.content[0].id;
    const originalLength = keyStatsSection.content.length;

    const { pageContent } = einPageReducer(
      {
        pageContent: testEinPageContent,
        pageVersion: testEinPageVersion,
      },
      {
        type: 'REMOVE_BLOCK_FROM_SECTION',
        payload: {
          meta: {
            blockId: removingBlockId,
            sectionId: keyStatsSection.id,
          },
        },
      },
    );

    expect(pageContent.content[0].content?.length).toEqual(originalLength - 1);

    expect(
      pageContent.content[0].content?.filter(
        block => block.id === removingBlockId,
      ),
    ).toHaveLength(0);
  });

  test('UPDATE_BLOCK_FROM_SECTION updates an HTML block from section', () => {
    const section = testEinPageContent.content[0];
    const blockToUpdate = section.content[0] as EinHtmlBlock;

    const newBody = 'This is some updated text!';

    const { pageContent } = einPageReducer(
      {
        pageContent: testEinPageContent,
        pageVersion: testEinPageVersion,
      },
      {
        type: 'UPDATE_BLOCK_FROM_SECTION',
        payload: {
          meta: {
            blockId: blockToUpdate.id,
            sectionId: section.id,
          },
          block: {
            ...blockToUpdate,
            body: newBody,
          },
        },
      },
    );

    expect((pageContent.content[0].content as EinHtmlBlock[])[0].body).toEqual(
      newBody,
    );
  });
  test('UPDATE_BLOCK_FROM_SECTION updates a tile block from section', () => {
    const tileSection = testEinPageContent.content[2];
    const tileBlockToUpdate = tileSection.content[0] as EinTileGroupBlock;

    const newHeading = 'This is an updated heading!';

    const { pageContent } = einPageReducer(
      {
        pageContent: testEinPageContent,
        pageVersion: testEinPageVersion,
      },
      {
        type: 'UPDATE_BLOCK_FROM_SECTION',
        payload: {
          meta: {
            blockId: tileBlockToUpdate.id,
            sectionId: tileSection.id,
          },
          block: {
            ...tileBlockToUpdate,
            title: newHeading,
          },
        },
      },
    );

    expect(
      (pageContent.content[2].content[0] as EinTileGroupBlock).title,
    ).toEqual(newHeading);
  });

  test('ADD_BLOCK_TO_SECTION adds a block to a section', () => {
    const section = testEinPageContent.content[0];
    const newBlock: EinContentBlock = {
      id: '123',
      order: 1,
      body: 'This section is empty...',
      type: 'HtmlBlock',
    };

    const originalLength = section.content?.length || 0;

    const { pageContent } = einPageReducer(
      {
        pageContent: testEinPageContent,
        pageVersion: testEinPageVersion,
      },
      {
        type: 'ADD_BLOCK_TO_SECTION',
        payload: {
          meta: {
            sectionId: section.id,
          },
          block: newBlock,
        },
      },
    );

    expect(pageContent.content[0].content).toHaveLength(originalLength + 1);

    expect(
      (pageContent.content[0].content as EinHtmlBlock[])[originalLength].id,
    ).toEqual(newBlock.id);
  });

  test('ADD_CONTENT_SECTION adds a new section to page content', () => {
    const originalLength = testEinPageContent.content.length;
    const { pageContent } = einPageReducer(
      {
        pageContent: testEinPageContent,
        pageVersion: testEinPageVersion,
      },
      {
        type: 'ADD_CONTENT_SECTION',
        payload: {
          section: {
            caption: '',
            heading: 'A new section',
            id: 'new-section-1',
            order: testEinPageContent.content.length,
            content: [],
          },
        },
      },
    );

    expect(pageContent.content).toHaveLength(originalLength + 1);

    expect(pageContent.content[originalLength].id).toEqual('new-section-1');
  });

  test("SET_CONTENT sets the page's content", () => {
    const contentSectionsLength = testEinPageContent.content.length;
    const contentSectionsReversed = testEinPageContent.content.map(
      (section, index) => {
        return { ...section, order: contentSectionsLength - (1 + index) };
      },
    );

    const { pageContent } = einPageReducer(
      {
        pageContent: testEinPageContent,
        pageVersion: testEinPageVersion,
      },
      {
        type: 'SET_CONTENT',
        payload: {
          content: contentSectionsReversed,
        },
      },
    );

    expect(pageContent.content[0].order).toEqual(contentSectionsLength - 1);
    expect(pageContent.content[1].order).toEqual(contentSectionsLength - 2);
    expect(pageContent.content).toHaveLength(contentSectionsLength);
  });

  test('UPDATE_CONTENT_SECTION updates a content section', () => {
    const basicSection = testEinPageContent.content[0];
    const { pageContent } = einPageReducer(
      {
        pageContent: testEinPageContent,
        pageVersion: testEinPageVersion,
      },
      {
        type: 'UPDATE_CONTENT_SECTION',
        payload: {
          meta: { sectionId: basicSection.id },
          section: {
            ...basicSection,
            caption: 'updated caption',
            heading: 'updated heading',
          },
        },
      },
    );

    expect(pageContent.content[0].id).toEqual(basicSection.id);
    expect(pageContent.content[0].caption).toEqual('updated caption');
    expect(pageContent.content[0].heading).toEqual('updated heading');
  });

  test('ADD_FREE_TEXT_STAT_TILE_TO_BLOCK adds a tile to a block', () => {
    const section = testEinPageContent.content[2];
    const block = section.content[0] as EinTileGroupBlock;
    const newTile: EinFreeTextStatTile = {
      id: '123',
      order: 1,
      type: 'FreeTextStatTile',
      title: 'A new tile',
      statistic: '1000',
      trend: 'A new trend',
    };

    const originalLength = block.tiles.length || 0;

    const { pageContent } = einPageReducer(
      {
        pageContent: testEinPageContent,
        pageVersion: testEinPageVersion,
      },
      {
        type: 'ADD_FREE_TEXT_STAT_TILE_TO_BLOCK',
        payload: {
          meta: {
            blockId: block.id,
            sectionId: section.id,
          },
          tile: newTile,
        },
      },
    );

    const updatedBlock = pageContent.content[2].content[0] as EinTileGroupBlock;
    expect(updatedBlock.tiles).toHaveLength(originalLength + 1);

    expect(updatedBlock.tiles[originalLength].id).toEqual(newTile.id);
  });

  test('UPDATE_FREE_TEXT_STAT_TILE_IN_BLOCK updates a tile in a block', () => {
    const section = testEinPageContent.content[2];
    const block = section.content[0] as EinTileGroupBlock;
    const tileToUpdate = block.tiles[0];

    const newStatistic = '9999';

    const { pageContent } = einPageReducer(
      {
        pageContent: testEinPageContent,
        pageVersion: testEinPageVersion,
      },
      {
        type: 'UPDATE_FREE_TEXT_STAT_TILE_IN_BLOCK',
        payload: {
          meta: {
            tileId: tileToUpdate.id,
            blockId: block.id,
            sectionId: section.id,
          },
          tile: {
            ...tileToUpdate,
            statistic: newStatistic,
          },
        },
      },
    );

    const updatedBlock = pageContent.content[2].content[0] as EinTileGroupBlock;
    expect(updatedBlock.tiles).toHaveLength(2);

    expect(updatedBlock.tiles[0].id).toEqual(tileToUpdate.id);
    expect(updatedBlock.tiles[0].statistic).toEqual(newStatistic);
  });

  test('REORDER_FREE_TEXT_STAT_TILES_IN_BLOCK reorders tiles in a block', () => {
    const section = testEinPageContent.content[2];
    const block = section.content[0] as EinTileGroupBlock;

    const { pageContent } = einPageReducer(
      {
        pageContent: testEinPageContent,
        pageVersion: testEinPageVersion,
      },
      {
        type: 'REORDER_FREE_TEXT_STAT_TILES_IN_BLOCK',
        payload: {
          meta: {
            blockId: block.id,
            sectionId: section.id,
          },
          tiles: [
            {
              id: 'tile-2',
              type: 'FreeTextStatTile',
              order: 0,
              title: 'Tile 2 title',
              statistic: '2000',
              trend: 'Tile 2 trend',
              linkText: 'Tile 2 link text',
              linkUrl: 'https://example.com/tile-2',
            },
            { ...testTile, order: 1 },
          ],
        },
      },
    );

    const updatedBlock = pageContent.content[2].content[0] as EinTileGroupBlock;
    expect(updatedBlock.tiles).toHaveLength(2);

    expect(updatedBlock.tiles[0].id).toEqual('tile-2');
    expect(updatedBlock.tiles[0].order).toEqual(0);
  });

  test('DELETE_FREE_TEXT_STAT_TILE_FROM_BLOCK removes a tile from a block', () => {
    const section = testEinPageContent.content[2];
    const block = section.content[0] as EinTileGroupBlock;
    const tileToDelete = block.tiles[0];

    const { pageContent } = einPageReducer(
      {
        pageContent: testEinPageContent,
        pageVersion: testEinPageVersion,
      },
      {
        type: 'DELETE_FREE_TEXT_STAT_TILE_FROM_BLOCK',
        payload: {
          meta: {
            tileId: tileToDelete.id,
            blockId: block.id,
            sectionId: section.id,
          },
        },
      },
    );

    const updatedBlock = pageContent.content[2].content[0] as EinTileGroupBlock;
    expect(updatedBlock.tiles).toHaveLength(1);
  });
});
