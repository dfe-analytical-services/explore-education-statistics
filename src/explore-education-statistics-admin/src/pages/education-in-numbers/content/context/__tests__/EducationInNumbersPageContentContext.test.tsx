import { produce } from 'immer';
import { EinContent } from '@admin/services/educationInNumbersContentService';
import testEinPageVersion from '@admin/pages/education-in-numbers/__tests__/__data__/testEducationInNumbersPageAndContent';
import { HtmlBlock } from '@common/services/types/blocks';
import {
  EducationInNumbersPageContextState,
  educationInNumbersPageReducer as originalEinPageReducer,
} from '../EducationInNumbersPageContentContext';
import { EducationInNumbersPageDispatchAction } from '../EducationInNumbersPageContentContextActionTypes';

const basicEinPage: EinContent = {
  id: 'ein-page-0',
  title: 'Education in Numbers',
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
          type: 'HtmlBlock',
          order: 0,
        },
        {
          id: 'content-section-0-content-1',
          body: 'Part 2',
          type: 'HtmlBlock',
          order: 0,
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
};

const einPageReducer = (
  initial: EducationInNumbersPageContextState,
  action: EducationInNumbersPageDispatchAction,
) => produce(initial, draft => originalEinPageReducer(draft, action));

describe('EducationInNumbersPageContentContext', () => {
  test('REMOVE_BLOCK_FROM_SECTION removes a block from a section', () => {
    const keyStatsSection = basicEinPage.content[0];
    const removingBlockId = keyStatsSection.content[0].id;
    const originalLength = keyStatsSection.content.length;

    const { pageContent } = einPageReducer(
      {
        pageContent: basicEinPage,
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

  test('UPDATE_BLOCK_FROM_SECTION updates a block from section', () => {
    const section = basicEinPage.content[0];
    const blockToUpdate = (section.content as HtmlBlock[])[0];

    const newBody = 'This is some updated text!';

    const { pageContent } = einPageReducer(
      {
        pageContent: basicEinPage,
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

    expect((pageContent.content[0].content as HtmlBlock[])[0].body).toEqual(
      newBody,
    );
  });

  test('ADD_BLOCK_TO_SECTION adds a block to a section', () => {
    const section = basicEinPage.content[0];
    const newBlock: HtmlBlock = {
      id: '123',
      order: 1,
      body: 'This section is empty...',
      type: 'HtmlBlock',
    };

    const originalLength = section.content?.length || 0;

    const { pageContent } = einPageReducer(
      {
        pageContent: basicEinPage,
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
      (pageContent.content[0].content as HtmlBlock[])[originalLength].id,
    ).toEqual(newBlock.id);
  });

  test('ADD_CONTENT_SECTION adds a new section to page content', () => {
    const originalLength = basicEinPage.content.length;
    const { pageContent } = einPageReducer(
      {
        pageContent: basicEinPage,
        pageVersion: testEinPageVersion,
      },
      {
        type: 'ADD_CONTENT_SECTION',
        payload: {
          section: {
            caption: '',
            heading: 'A new section',
            id: 'new-section-1',
            order: basicEinPage.content.length,
            content: [],
          },
        },
      },
    );

    expect(pageContent.content).toHaveLength(originalLength + 1);

    expect(pageContent.content[originalLength].id).toEqual('new-section-1');
  });

  test("SET_CONTENT sets the page's content", () => {
    const contentSectionsLength = basicEinPage.content.length;
    const contentSectionsReversed = basicEinPage.content.map(
      (section, index) => {
        return { ...section, order: contentSectionsLength - (1 + index) };
      },
    );

    const { pageContent } = einPageReducer(
      {
        pageContent: basicEinPage,
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
    const basicSection = basicEinPage.content[0];
    const { pageContent } = einPageReducer(
      {
        pageContent: basicEinPage,
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
});
