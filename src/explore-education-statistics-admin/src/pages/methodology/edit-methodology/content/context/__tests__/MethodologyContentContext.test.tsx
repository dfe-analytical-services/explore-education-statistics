import { MethodologyContent } from '@admin/services/methodologyContentService';
import { EditableContentBlock } from '@admin/services/types/content';
import { produce } from 'immer';
import {
  MethodologyContextState,
  methodologyReducer as originalMethodologyReducer,
} from '@admin/pages/methodology/edit-methodology/content/context/MethodologyContentContext';
import { MethodologyDispatchAction } from '@admin/pages/methodology/edit-methodology/content/context/MethodologyContentContextActionTypes';

const basicMethodology: MethodologyContent = {
  id: 'methodology-0',
  title: 'Academic year 2020/21',
  status: 'Draft',
  slug: '2020-21',
  content: [
    {
      id: 'content-section-0',
      order: 0,
      caption: '',
      heading: 'New section 3',
      content: [
        {
          id: 'content-section-0-content-0',
          body: '',
          type: 'MarkDownBlock',
          order: 0,
          comments: [],
        },
        {
          id: 'content-section-0-content-1',
          body: 'Part 2',
          type: 'MarkDownBlock',
          order: 0,
          comments: [],
        },
      ],
    },
    {
      id: 'content-section-1',
      order: 1,
      heading: 'New section',
      content: [],
      caption: '',
    },
  ],
  annexes: [
    {
      id: 'annex-section-0',
      order: 0,
      heading: 'New section 3',
      caption: '',
      content: [
        {
          id: 'annex-section-0-content-0',
          body: 'annex text',
          type: 'MarkDownBlock',
          order: 0,
          comments: [],
        },
      ],
    },
    {
      id: 'annex-section-1',
      order: 1,
      heading: 'New section',
      content: [],
      caption: '',
    },
  ],
  notes: [],
};

const methodologyReducer = (
  initial: MethodologyContextState,
  action: MethodologyDispatchAction,
) => produce(initial, draft => originalMethodologyReducer(draft, action));

describe('MethodologyContext', () => {
  test('REMOVE_BLOCK_FROM_SECTION removes a block from a section', () => {
    const sectionKey = 'annexes';
    const keyStatsSection = basicMethodology[sectionKey][0];
    const removingBlockId = keyStatsSection.content[0].id;
    const originalLength = keyStatsSection.content.length;

    const { methodology } = methodologyReducer(
      {
        methodology: basicMethodology,
        canUpdateMethodology: true,
      },
      {
        type: 'REMOVE_BLOCK_FROM_SECTION',
        payload: {
          meta: {
            blockId: removingBlockId,
            sectionId: keyStatsSection.id,
            sectionKey,
          },
        },
      },
    );

    expect(methodology[sectionKey][0].content?.length).toEqual(
      originalLength - 1,
    );

    expect(
      methodology[sectionKey][0].content?.filter(
        block => block.id === removingBlockId,
      ),
    ).toHaveLength(0);
  });

  test('REMOVE_BLOCK_FROM_SECTION removes a block from content section', () => {
    const sectionKey = 'content';
    const methodologyContent = basicMethodology[sectionKey];
    const removingBlockId = methodologyContent[0].content[0].id;
    const originalLength = methodologyContent[0].content.length;

    const { methodology } = methodologyReducer(
      {
        methodology: basicMethodology,
        canUpdateMethodology: true,
      },
      {
        type: 'REMOVE_BLOCK_FROM_SECTION',
        payload: {
          meta: {
            blockId: removingBlockId,
            sectionId: methodologyContent[0].id,
            sectionKey,
          },
        },
      },
    );

    expect(methodology.content[0].content?.length).toEqual(originalLength - 1);

    expect(
      methodology.content[0].content?.filter(
        block => block.id === removingBlockId,
      ),
    ).toHaveLength(0);
  });

  test('UPDATE_BLOCK_FROM_SECTION updates a block from section', () => {
    const sectionKey = 'annexes';
    const section = basicMethodology[sectionKey][0];
    const blockToUpdate = (section.content as EditableContentBlock[])[0];

    const newBody = 'This is some updated text!';

    const { methodology } = methodologyReducer(
      {
        methodology: basicMethodology,
        canUpdateMethodology: true,
      },
      {
        type: 'UPDATE_BLOCK_FROM_SECTION',
        payload: {
          meta: {
            blockId: blockToUpdate.id,
            sectionId: section.id,
            sectionKey,
          },
          block: {
            ...blockToUpdate,
            body: newBody,
          },
        },
      },
    );

    expect(
      (methodology[sectionKey][0].content as EditableContentBlock[])[0].body,
    ).toEqual(newBody);
  });

  test('UPDATE_BLOCK_FROM_SECTION updates a block from a content section', () => {
    const sectionKey = 'content';
    const sectionId = basicMethodology.content[0].id;
    const blockToUpdate = (basicMethodology.content[0]
      .content as EditableContentBlock[])[0];

    const newBody = 'This is some updated text!';

    const { methodology } = methodologyReducer(
      {
        methodology: basicMethodology,
        canUpdateMethodology: true,
      },
      {
        type: 'UPDATE_BLOCK_FROM_SECTION',
        payload: {
          meta: {
            blockId: blockToUpdate.id,
            sectionId,
            sectionKey,
          },
          block: {
            ...blockToUpdate,
            body: newBody,
          },
        },
      },
    );

    expect(
      (methodology.content[0].content as EditableContentBlock[])[0].body,
    ).toEqual(newBody);
  });

  test('ADD_BLOCK_TO_SECTION adds a block to a section', () => {
    const sectionKey = 'annexes';
    const section = basicMethodology[sectionKey][0];
    const newBlock: EditableContentBlock = {
      id: '123',
      order: 1,
      body: 'This section is empty...',
      comments: [],
      type: 'MarkDownBlock',
    };

    const originalLength = section.content?.length || 0;

    const { methodology } = methodologyReducer(
      {
        methodology: basicMethodology,
        canUpdateMethodology: false,
      },
      {
        type: 'ADD_BLOCK_TO_SECTION',
        payload: {
          meta: {
            sectionId: section.id,
            sectionKey,
          },
          block: newBlock,
        },
      },
    );

    expect(methodology[sectionKey][0].content).toHaveLength(originalLength + 1);

    expect(
      (methodology[sectionKey][0].content as EditableContentBlock[])[
        originalLength
      ].id,
    ).toEqual(newBlock.id);
  });

  test('ADD_BLOCK_TO_SECTION adds a block to a content section', () => {
    const sectionKey = 'content';
    const section = basicMethodology.content[0];

    const newBlock: EditableContentBlock = {
      id: '123',
      order: 0,
      body: 'This section is empty...',
      comments: [],
      type: 'MarkDownBlock',
    };

    const originalLength = section.content?.length || 0;

    const { methodology } = methodologyReducer(
      {
        methodology: basicMethodology,
        canUpdateMethodology: true,
      },
      {
        type: 'ADD_BLOCK_TO_SECTION',
        payload: {
          meta: {
            sectionId: section.id,
            sectionKey,
          },
          block: newBlock,
        },
      },
    );

    expect(
      methodology.content[0].content as EditableContentBlock[],
    ).toHaveLength(originalLength + 1);

    expect(
      (methodology.content[0].content as EditableContentBlock[])[originalLength]
        .id,
    ).toEqual(newBlock.id);
  });

  test('ADD_CONTENT_SECTION adds a new section to methodology content', () => {
    const originalLength = basicMethodology.content.length;
    const { methodology } = methodologyReducer(
      {
        methodology: basicMethodology,
        canUpdateMethodology: true,
      },
      {
        type: 'ADD_CONTENT_SECTION',
        payload: {
          sectionKey: 'content',
          section: {
            caption: '',
            heading: 'A new section',
            id: 'new-section-1',
            order: basicMethodology.content.length,
            content: [],
          },
        },
      },
    );

    expect(methodology.content).toHaveLength(originalLength + 1);

    expect(methodology.content[originalLength].id).toEqual('new-section-1');
  });

  test("SET_CONTENT sets the methodology's content", () => {
    const contentSectionsLength = basicMethodology.content.length;
    const contentSectionsReversed = basicMethodology.content.map(
      (section, index) => {
        return { ...section, order: contentSectionsLength - (1 + index) };
      },
    );

    const { methodology } = methodologyReducer(
      {
        methodology: basicMethodology,
        canUpdateMethodology: true,
      },
      {
        type: 'SET_CONTENT',
        payload: {
          sectionKey: 'content',
          content: contentSectionsReversed,
        },
      },
    );

    expect(methodology.content[0].order).toEqual(contentSectionsLength - 1);
    expect(methodology.content[1].order).toEqual(contentSectionsLength - 2);
    expect(methodology.content).toHaveLength(contentSectionsLength);
  });

  test('UPDATE_CONTENT_SECTION updates a content section', () => {
    const basicSection = basicMethodology.content[0];
    const { methodology } = methodologyReducer(
      {
        methodology: basicMethodology,
        canUpdateMethodology: true,
      },
      {
        type: 'UPDATE_CONTENT_SECTION',
        payload: {
          meta: { sectionId: basicSection.id, sectionKey: 'content' },
          section: {
            ...basicSection,
            caption: 'updated caption',
            heading: 'updated heading',
          },
        },
      },
    );

    expect(methodology.content[0].id).toEqual(basicSection.id);
    expect(methodology.content[0].caption).toEqual('updated caption');
    expect(methodology.content[0].heading).toEqual('updated heading');
  });
});
