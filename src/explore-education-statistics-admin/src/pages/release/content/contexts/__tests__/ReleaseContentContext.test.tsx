import {
  ReleaseContentContextState,
  releaseReducer as originalReleaseReducer,
} from '@admin/pages/release/content/contexts/ReleaseContentContext';
import { ReleaseDispatchAction } from '@admin/pages/release/content/contexts/ReleaseContentContextActionTypes';
import {
  Comment,
  CommentUser,
  EditableBlock,
  EditableContentBlock,
} from '@admin/services/types/content';
import { testComments } from '@admin/components/comments/__data__/testComments';
import generateReleaseContent, {
  generateEditableRelease,
} from '@admin-test/generators/releaseContentGenerators';
import {
  generateContentSection,
  generateEditableContentBlock,
  generateEditableDataBlock,
} from '@admin-test/generators/contentGenerators';
import {
  ContentSection,
  KeyStatisticDataBlock,
} from '@common/services/publicationService';
import { Table } from '@common/services/types/blocks';
import { produce } from 'immer';

const emptyTable: Table = {
  indicators: [],
  tableHeaders: {
    columnGroups: [],
    columns: [],
    rowGroups: [],
    rows: [],
  },
};

const testCommentUser: CommentUser = {
  id: 'user-1',
  displayName: 'Jane Doe',
  email: 'jane@test.com',
};

const testComment: Comment = {
  id: 'comment-1',
  content: 'Comment 1 content',
  createdBy: testCommentUser,
  created: '2021-11-29T13:55',
  resolved: '2021-11-30T13:55',
  resolvedBy: testCommentUser,
};

const releaseReducer = (
  initial: ReleaseContentContextState,
  action: ReleaseDispatchAction,
) => produce(initial, draft => originalReleaseReducer(draft, action));

describe('ReleaseContentContext', () => {
  const testReleaseContent = generateReleaseContent({});
  const testReleaseContentWithContentComments = generateReleaseContent({
    release: generateEditableRelease({
      content: [
        generateContentSection({
          content: [
            generateEditableContentBlock({
              comments: [testComments[0], testComments[1]],
            }),
          ],
        }),
      ],
    }),
  });
  const testReleaseContentWithHeadlinesComments = generateReleaseContent({
    release: generateEditableRelease({
      headlinesSection: {
        id: 'headlines-section-id',
        heading: 'Headlines section heading',
        order: 0,
        content: [
          generateEditableContentBlock({
            comments: [testComments[0]],
          }),
        ],
      },
    }),
  });
  const basicDataBlock = generateEditableDataBlock({});

  test('SET_UNATTACHED_DATABLOCKS sets data blocks', () => {
    expect(
      releaseReducer(
        {
          ...testReleaseContent,
          canUpdateRelease: false,
        },
        {
          type: 'SET_UNATTACHED_DATABLOCKS',
          payload: [basicDataBlock],
        },
      ),
    ).toEqual({
      release: testReleaseContent.release,
      canUpdateRelease: false,
      unattachedDataBlocks: [basicDataBlock],
    });
  });

  test('REMOVE_SECTION_BLOCK removes a block from a named section', () => {
    const { headlinesSection } = testReleaseContent.release;
    const removingBlockId = headlinesSection.content[0].id;

    expect(testReleaseContent.release.headlinesSection.content.length).toEqual(
      1,
    );

    const { release } = releaseReducer(
      {
        release: testReleaseContent.release,
        canUpdateRelease: true,
        unattachedDataBlocks: [basicDataBlock],
      },
      {
        type: 'REMOVE_SECTION_BLOCK',
        payload: {
          meta: {
            blockId: removingBlockId,
            sectionId: headlinesSection.id,
            sectionKey: 'headlinesSection',
          },
        },
      },
    );

    expect(release.headlinesSection.content).toEqual([]);
  });

  test('REMOVE_SECTION_BLOCK removes a block from generic content section', () => {
    const contentSection = testReleaseContent.release.content;
    const removingBlockId = contentSection[0].content[0].id;

    expect(contentSection[0].content).toHaveLength(2);

    const { release } = releaseReducer(
      {
        release: testReleaseContent.release,
        canUpdateRelease: true,
        unattachedDataBlocks: [basicDataBlock],
      },
      {
        type: 'REMOVE_SECTION_BLOCK',
        payload: {
          meta: {
            blockId: removingBlockId,
            sectionId: contentSection[0].id,
            sectionKey: 'content',
          },
        },
      },
    );

    expect(release.content[0].content).toHaveLength(1);
    expect(release.content[0].content).toEqual([contentSection[0].content[1]]);
  });

  test('UPDATE_SECTION_BLOCK updates a block from named section', () => {
    const section = testReleaseContent.release.headlinesSection;
    const blockToUpdate = section.content[0];

    const newBody = 'This is some updated text!';

    const { release } = releaseReducer(
      {
        release: testReleaseContent.release,
        canUpdateRelease: true,
        unattachedDataBlocks: [basicDataBlock],
      },
      {
        type: 'UPDATE_SECTION_BLOCK',
        payload: {
          meta: {
            blockId: blockToUpdate.id,
            sectionId: section.id,
            sectionKey: 'headlinesSection',
          },
          block: {
            ...blockToUpdate,
            body: newBody,
          },
        },
      },
    );

    expect(release.headlinesSection.content[0].body).toEqual(newBody);
  });

  test('UPDATE_SECTION_BLOCK updates a block from a generic content section', () => {
    const sectionId = testReleaseContent.release.content[0].id;
    const blockToUpdate = testReleaseContent.release.content[0]
      .content[0] as EditableContentBlock;

    const newBody = 'This is some updated text!';

    const { release } = releaseReducer(
      {
        release: testReleaseContent.release,
        canUpdateRelease: true,
        unattachedDataBlocks: [basicDataBlock],
      },
      {
        type: 'UPDATE_SECTION_BLOCK',
        payload: {
          meta: {
            blockId: blockToUpdate.id,
            sectionId,
            sectionKey: 'content',
          },
          block: {
            ...blockToUpdate,
            body: newBody,
          },
        },
      },
    );

    expect(
      (release.content[0].content[0] as EditableContentBlock).body,
    ).toEqual(newBody);
  });

  test('ADD_SECTION_BLOCK adds a block to a named section', () => {
    const testReleaseContentWithoutSummaryContent = generateReleaseContent({
      release: generateEditableRelease({
        summarySection: {
          id: 'summary-section-id',
          content: [],
          heading: 'Summary block heading',
          order: 0,
        },
      }),
    });
    const section =
      testReleaseContentWithoutSummaryContent.release.summarySection;
    const newBlock = generateEditableContentBlock({
      body: 'This section is empty...',
    });

    expect(section.content).toHaveLength(0);

    const { release } = releaseReducer(
      {
        release: testReleaseContentWithoutSummaryContent.release,
        canUpdateRelease: false,
        unattachedDataBlocks: [],
      },
      {
        type: 'ADD_SECTION_BLOCK',
        payload: {
          meta: {
            sectionId: section.id,
            sectionKey: 'summarySection',
          },
          block: newBlock,
        },
      },
    );

    expect(release.summarySection.content).toEqual([newBlock]);
  });

  test('ADD_SECTION_BLOCK adds a block to a generic content section', () => {
    const section = testReleaseContent.release.content[0];

    const newBlock: EditableContentBlock = {
      id: '123',
      order: 0,
      body: 'This section is empty...',
      comments: [],
      type: 'HtmlBlock',
    };

    expect(section.content).toHaveLength(2);

    const { release } = releaseReducer(
      {
        release: testReleaseContent.release,
        canUpdateRelease: true,
        unattachedDataBlocks: [basicDataBlock],
      },
      {
        type: 'ADD_SECTION_BLOCK',
        payload: {
          meta: {
            sectionId: section.id,
            sectionKey: 'content',
          },
          block: newBlock,
        },
      },
    );

    expect(release.content[0].content).toEqual([
      section.content[0],
      section.content[1],
      newBlock,
    ]);
  });

  test("UPDATE_SECTION_CONTENT updates a section's content with new content", () => {
    const newContent: EditableBlock[] = [
      {
        name: 'Test data block',
        heading: "'prma' from 'My Pub' in England for 2017/18",
        source: '',
        query: {
          subjectId: '36aa28ce-83ca-49b5-8c27-b34e77b062c9',
          timePeriod: {
            startYear: 2017,
            startCode: 'AY',
            endYear: 2017,
            endCode: 'AY',
          },
          filters: ['5a1b6c93-c9c3-4710-f71f-08d7bec1ddb3'],
          indicators: [
            '272f4b12-9dfe-45ef-fdf9-08d7bec1dd71',
            'd314e2a9-5069-4d79-fdfa-08d7bec1dd71',
            '214f5fb8-c12b-411e-fdfb-08d7bec1dd71',
            '99f5bbc3-de9a-4831-fdf8-08d7bec1dd71',
          ],
          locationIds: ['dd590fcf-b0c1-4fa3-8599-d13c0f540793'],
        },
        charts: [],
        table: emptyTable,
        type: 'DataBlock',
        dataSetId: 'cpih01',
        dataSetName: 'CPIH01',
        id: '69a9522d-501d-441a-9ee5-260ede5cd85c',
        dataBlockParentId: 'datablock-1-parent',
        order: 0,
        comments: [],
      },
      {
        type: 'HtmlBlock',
        id: 'eb809a91-af3d-438d-632c-08d7c408aca5',
        order: 1,
        body: '',
        comments: [
          {
            id: 'comment-1',
            content: 'A comment',
            createdBy: {
              id: 'user-1',
              displayName: 'Bau1',
              email: 'bau1@test.com',
            },
            created: '2020-03-09T09:39:53.736',
          },
          {
            id: 'comment-2',
            content: 'another comment',
            createdBy: {
              id: 'user-1',
              displayName: 'Bau1',
              email: 'bau1@test.com',
            },
            created: '2020-03-09T09:40:16.534',
          },
        ],
      },
    ];

    const { release } = releaseReducer(
      {
        release: testReleaseContent.release,
        canUpdateRelease: true,
        unattachedDataBlocks: [basicDataBlock],
      },
      {
        type: 'UPDATE_SECTION_CONTENT',
        payload: {
          meta: {
            sectionId: testReleaseContent.release.content[0].id,
            sectionKey: 'content',
          },
          sectionContent: newContent,
        },
      },
    );

    expect(release.content[0].content[0]).toEqual(newContent[0]);
    expect(release.content[0].content[1]).toEqual(newContent[1]);
  });

  test('ADD_CONTENT_SECTION adds a new section to release content', () => {
    expect(testReleaseContent.release.content).toHaveLength(2);

    const newSection: ContentSection<EditableBlock> = {
      heading: 'A new section',
      id: 'new-section-1',
      order: testReleaseContent.release.content.length,
      content: [],
    };

    const { release } = releaseReducer(
      {
        release: testReleaseContent.release,
        canUpdateRelease: true,
        unattachedDataBlocks: [basicDataBlock],
      },
      {
        type: 'ADD_CONTENT_SECTION',
        payload: {
          section: newSection,
        },
      },
    );

    expect(release.content).toEqual([
      testReleaseContent.release.content[0],
      testReleaseContent.release.content[1],
      newSection,
    ]);
  });

  test("SET_CONTENT sets the release's content", () => {
    const { release } = releaseReducer(
      {
        release: testReleaseContent.release,
        canUpdateRelease: true,
        unattachedDataBlocks: [basicDataBlock],
      },
      {
        type: 'SET_CONTENT',
        payload: {
          content: [
            testReleaseContent.release.content[1],
            testReleaseContent.release.content[0],
          ],
        },
      },
    );

    expect(release.content).toEqual([
      testReleaseContent.release.content[1],
      testReleaseContent.release.content[0],
    ]);
  });

  test('UPDATE_CONTENT_SECTION updates a generic content section', () => {
    const section = testReleaseContent.release.content[0];

    const updatedSection: ContentSection<EditableBlock> = {
      ...section,
      heading: 'updated heading',
    };

    const { release } = releaseReducer(
      {
        release: testReleaseContent.release,
        canUpdateRelease: true,
        unattachedDataBlocks: [basicDataBlock],
      },
      {
        type: 'UPDATE_CONTENT_SECTION',
        payload: {
          meta: { sectionId: section.id },
          section: updatedSection,
        },
      },
    );

    expect(release.content[0]).toEqual(updatedSection);
  });

  test('ADD_BLOCK_COMMENT adds a comment to named section block', () => {
    const sectionId = testReleaseContent.release.headlinesSection.id;
    const block = testReleaseContent.release.headlinesSection.content[0];

    const { release } = releaseReducer(
      {
        release: testReleaseContent.release,
        canUpdateRelease: true,
        unattachedDataBlocks: [basicDataBlock],
      },
      {
        type: 'ADD_BLOCK_COMMENT',
        payload: {
          meta: {
            blockId: block.id,
            sectionId,
            sectionKey: 'headlinesSection',
          },
          comment: testComment,
        },
      },
    );

    expect(release.headlinesSection.content[0].comments).toEqual([
      ...block.comments,
      testComment,
    ]);
  });

  test('ADD_BLOCK_COMMENT adds a comment to generic content section block', () => {
    const sectionId = testReleaseContent.release.content[0].id;
    const block = testReleaseContent.release.content[0].content[0];

    const { release } = releaseReducer(
      {
        release: testReleaseContent.release,
        canUpdateRelease: true,
        unattachedDataBlocks: [basicDataBlock],
      },
      {
        type: 'ADD_BLOCK_COMMENT',
        payload: {
          meta: {
            blockId: block.id,
            sectionId,
            sectionKey: 'content',
          },
          comment: testComment,
        },
      },
    );

    expect(release.content[0].content[0].comments).toEqual([
      ...block.comments,
      testComment,
    ]);
  });

  test('UPDATE_BLOCK_COMMENT updates a comment in named section block', () => {
    const sectionId =
      testReleaseContentWithHeadlinesComments.release.headlinesSection.id;
    const block =
      testReleaseContentWithHeadlinesComments.release.headlinesSection
        .content[0];

    expect(block.comments).toHaveLength(1);

    const updatedComment: Comment = {
      ...block.comments[0],
      content: 'Updated comment content',
    };

    const { release } = releaseReducer(
      {
        release: testReleaseContentWithHeadlinesComments.release,
        canUpdateRelease: true,
        unattachedDataBlocks: [basicDataBlock],
      },
      {
        type: 'UPDATE_BLOCK_COMMENT',
        payload: {
          meta: {
            blockId: block.id,
            sectionId,
            sectionKey: 'headlinesSection',
          },
          comment: updatedComment,
        },
      },
    );

    expect(release.headlinesSection.content[0].comments).toEqual([
      updatedComment,
    ]);
  });

  test('UPDATE_BLOCK_COMMENT updates a comment in generic content section block', () => {
    const sectionId =
      testReleaseContentWithContentComments.release.content[0].id;
    const block =
      testReleaseContentWithContentComments.release.content[0].content[0];

    expect(block.comments).toHaveLength(2);

    const updatedComment: Comment = {
      ...block.comments[1],
      content: 'Updated comment content',
    };

    const { release } = releaseReducer(
      {
        release: testReleaseContentWithContentComments.release,
        canUpdateRelease: true,
        unattachedDataBlocks: [basicDataBlock],
      },
      {
        type: 'UPDATE_BLOCK_COMMENT',
        payload: {
          meta: {
            blockId: block.id,
            sectionId,
            sectionKey: 'content',
          },
          comment: updatedComment,
        },
      },
    );

    expect(release.content[0].content[0].comments).toEqual([
      block.comments[0],
      updatedComment,
    ]);
  });

  test('REMOVE_BLOCK_COMMENT removes a comment from named section block', () => {
    const sectionId =
      testReleaseContentWithHeadlinesComments.release.headlinesSection.id;
    const block =
      testReleaseContentWithHeadlinesComments.release.headlinesSection
        .content[0];

    expect(block.comments).toHaveLength(1);

    const { release } = releaseReducer(
      {
        release: testReleaseContentWithHeadlinesComments.release,
        canUpdateRelease: true,
        unattachedDataBlocks: [basicDataBlock],
      },
      {
        type: 'REMOVE_BLOCK_COMMENT',
        payload: {
          meta: {
            blockId: block.id,
            sectionId,
            sectionKey: 'headlinesSection',
          },
          commentId: block.comments[0].id,
        },
      },
    );

    expect(release.headlinesSection.content[0].comments).toEqual([]);
  });

  test('UPDATE_BLOCK_COMMENT removes a comment from generic content section block', () => {
    const sectionId =
      testReleaseContentWithContentComments.release.content[0].id;
    const block =
      testReleaseContentWithContentComments.release.content[0].content[0];

    expect(block.comments).toHaveLength(2);

    const { release } = releaseReducer(
      {
        release: testReleaseContentWithContentComments.release,
        canUpdateRelease: true,
        unattachedDataBlocks: [basicDataBlock],
      },
      {
        type: 'REMOVE_BLOCK_COMMENT',
        payload: {
          meta: {
            blockId: block.id,
            sectionId,
            sectionKey: 'content',
          },
          commentId: block.comments[0].id,
        },
      },
    );

    expect(release.content[0].content[0].comments).toEqual([block.comments[1]]);
  });

  test('ADD_KEY_STATISTIC adds a key statistic to release.keyStatistics array', () => {
    expect(testReleaseContent.release.keyStatistics).toHaveLength(1);
    const newKeyStat: KeyStatisticDataBlock = {
      type: 'KeyStatisticDataBlock',
      id: 'keyStat-4',
      dataBlockParentId: 'dataBlock-0',
      trend: 'keyStat-4 trend',
      guidanceTitle: 'keyStat-4 guidanceTitle',
      guidanceText: 'keyStat-4 guidanceText',
      order: 1,
      created: '2022-02-01T12:00:00Z',
    };

    const { release } = releaseReducer(
      {
        release: testReleaseContent.release,
        canUpdateRelease: true,
        unattachedDataBlocks: [basicDataBlock],
      },
      {
        type: 'ADD_KEY_STATISTIC',
        payload: {
          keyStatistic: newKeyStat,
        },
      },
    );

    expect(release.keyStatistics).toEqual([
      ...testReleaseContent.release.keyStatistics,
      newKeyStat,
    ]);
  });

  test('UPDATE_KEY_STATISTIC updates a key statistic in release.keyStatistics array', () => {
    expect(testReleaseContent.release.keyStatistics).toHaveLength(1);
    const updatedKeyStat1 = {
      ...testReleaseContent.release.keyStatistics[0],
      trend: 'keyStat-1 trend updated',
      guidanceTitle: 'keyStat-1 new guidance title',
      guidanceText: 'keyStat-1 new guidance text',
    };
    const { release } = releaseReducer(
      {
        release: testReleaseContent.release,
        canUpdateRelease: true,
        unattachedDataBlocks: [basicDataBlock],
      },
      {
        type: 'UPDATE_KEY_STATISTIC',
        payload: { keyStatistic: updatedKeyStat1 },
      },
    );

    expect(release.keyStatistics).toEqual([
      updatedKeyStat1,
      testReleaseContent.release.keyStatistics[1],
      testReleaseContent.release.keyStatistics[2],
    ]);
  });

  test('REMOVE_KEY_STATISTIC removes a key statistic from release.keyStatistics array', () => {
    expect(testReleaseContent.release.keyStatistics).toHaveLength(1);
    const { release } = releaseReducer(
      {
        release: testReleaseContent.release,
        canUpdateRelease: true,
        unattachedDataBlocks: [basicDataBlock],
      },
      {
        type: 'REMOVE_KEY_STATISTIC',
        payload: {
          keyStatisticId: 'keyStat-2',
        },
      },
    );

    expect(release.keyStatistics).toEqual([
      testReleaseContent.release.keyStatistics[0],
      testReleaseContent.release.keyStatistics[2],
    ]);
  });

  test('SET_KEY_STATISTICS used to reorder existing key stats', () => {
    expect(testReleaseContent.release.keyStatistics).toHaveLength(1);
    const { release } = releaseReducer(
      {
        release: testReleaseContent.release,
        canUpdateRelease: true,
        unattachedDataBlocks: [basicDataBlock],
      },
      {
        type: 'SET_KEY_STATISTICS',
        payload: {
          keyStatistics: [
            testReleaseContent.release.keyStatistics[2],
            testReleaseContent.release.keyStatistics[1],
            testReleaseContent.release.keyStatistics[0],
          ],
        },
      },
    );

    expect(release.keyStatistics).toEqual([
      testReleaseContent.release.keyStatistics[2],
      testReleaseContent.release.keyStatistics[1],
      testReleaseContent.release.keyStatistics[0],
    ]);
  });
});
