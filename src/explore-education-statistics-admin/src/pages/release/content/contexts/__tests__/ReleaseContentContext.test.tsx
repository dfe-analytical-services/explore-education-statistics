import {
  ReleaseContentContextState,
  releaseReducer as originalReleaseReducer,
} from '@admin/pages/release/content/contexts/ReleaseContentContext';
import { ReleaseDispatchAction } from '@admin/pages/release/content/contexts/ReleaseContentContextActionTypes';
import {
  EditableBlock,
  EditableContentBlock,
} from '@admin/services/types/content';
import { DataBlock, Table } from '@common/services/types/blocks';
import { testEditableRelease } from '@admin/pages/release/__data__/testEditableRelease';
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

const basicDataBlock: DataBlock = {
  id: 'datablock-0',
  order: 1,
  type: 'DataBlock',
  name: 'Test data block',
  heading: '',
  source: '',
  charts: [],
  table: emptyTable,
  query: {
    filters: [],
    indicators: [],
    locationIds: [],
    subjectId: 'subjectId',
  },
};

const releaseReducer = (
  initial: ReleaseContentContextState,
  action: ReleaseDispatchAction,
) => produce(initial, draft => originalReleaseReducer(draft, action));

describe('ReleaseContentContext', () => {
  test('SET_AVAILABLE_DATABLOCKS sets datablocks', () => {
    expect(
      releaseReducer(
        {
          release: testEditableRelease,
          canUpdateRelease: false,
          availableDataBlocks: [],
        },
        {
          type: 'SET_AVAILABLE_DATABLOCKS',
          payload: [basicDataBlock],
        },
      ),
    ).toEqual({
      release: testEditableRelease,
      canUpdateRelease: false,
      availableDataBlocks: [basicDataBlock],
    });
  });

  test('REMOVE_BLOCK_FROM_SECTION removes a block from a section', () => {
    const sectionKey = 'keyStatisticsSection';
    const keyStatsSection = testEditableRelease[sectionKey];
    const removingBlockId = keyStatsSection.content[0].id;
    const originalLength = keyStatsSection.content.length;

    const { release } = releaseReducer(
      {
        release: testEditableRelease,
        canUpdateRelease: true,
        availableDataBlocks: [basicDataBlock],
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

    expect(release?.keyStatisticsSection.content?.length).toEqual(
      originalLength - 1,
    );

    expect(
      release?.keyStatisticsSection.content?.filter(
        block => block.id === removingBlockId,
      ),
    ).toHaveLength(0);
  });

  test('REMOVE_BLOCK_FROM_SECTION removes a block from content section', () => {
    const sectionKey = 'content';
    const releaseContent = testEditableRelease[sectionKey];
    const removingBlockId = (releaseContent[0]
      .content as EditableContentBlock[])[0].id;
    const originalLength = (releaseContent[0].content as EditableContentBlock[])
      .length;

    const { release } = releaseReducer(
      {
        release: testEditableRelease,
        canUpdateRelease: true,
        availableDataBlocks: [basicDataBlock],
      },
      {
        type: 'REMOVE_BLOCK_FROM_SECTION',
        payload: {
          meta: {
            blockId: removingBlockId,
            sectionId: releaseContent[0].id,
            sectionKey,
          },
        },
      },
    );

    expect(release?.content[0].content?.length).toEqual(originalLength - 1);

    expect(
      release?.content[0].content?.filter(
        block => block.id === removingBlockId,
      ),
    ).toHaveLength(0);
  });

  test('UPDATE_BLOCK_FROM_SECTION updates a block from section', () => {
    const sectionKey = 'headlinesSection';
    const section = testEditableRelease[sectionKey];
    const blockToUpdate = (section.content as EditableContentBlock[])[0];

    const newBody = 'This is some updated text!';

    const { release } = releaseReducer(
      {
        release: testEditableRelease,
        canUpdateRelease: true,
        availableDataBlocks: [basicDataBlock],
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
      (release?.headlinesSection.content as EditableContentBlock[])[0].body,
    ).toEqual(newBody);
  });

  test('UPDATE_BLOCK_FROM_SECTION updates a block from a content section', () => {
    const sectionKey = 'content';
    const sectionId = testEditableRelease.content[0].id;
    const blockToUpdate = (testEditableRelease.content[0]
      .content as EditableContentBlock[])[0];

    const newBody = 'This is some updated text!';

    const { release } = releaseReducer(
      {
        release: testEditableRelease,
        canUpdateRelease: true,
        availableDataBlocks: [basicDataBlock],
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
      (release?.content[0].content as EditableContentBlock[])[0].body,
    ).toEqual(newBody);
  });

  test('ADD_BLOCK_TO_SECTION adds a block to a section', () => {
    const sectionKey = 'summarySection';
    const section = testEditableRelease[sectionKey];
    const newBlock: EditableContentBlock = {
      id: '123',
      order: 0,
      body: 'This section is empty...',
      comments: [],
      type: 'MarkDownBlock',
    };

    const originalLength = section.content?.length || 0;

    const { release } = releaseReducer(
      {
        release: testEditableRelease,
        canUpdateRelease: false,
        availableDataBlocks: [],
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

    expect(release?.summarySection.content).toHaveLength(originalLength + 1);

    expect(
      (release?.summarySection.content as EditableContentBlock[])[
        originalLength
      ].id,
    ).toEqual(newBlock.id);
  });

  test('ADD_BLOCK_TO_SECTION adds a block to a content section', () => {
    const sectionKey = 'content';
    const section = testEditableRelease.content[0];

    const newBlock: EditableContentBlock = {
      id: '123',
      order: 0,
      body: 'This section is empty...',
      comments: [],
      type: 'MarkDownBlock',
    };

    const originalLength = section.content?.length || 0;

    const { release } = releaseReducer(
      {
        release: testEditableRelease,
        canUpdateRelease: true,
        availableDataBlocks: [basicDataBlock],
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

    expect(release?.content[0].content as EditableContentBlock[]).toHaveLength(
      originalLength + 1,
    );

    expect(
      (release?.content[0].content as EditableContentBlock[])[originalLength]
        .id,
    ).toEqual(newBlock.id);
  });

  test("UPDATE_SECTION_CONTENT updates a section's content with new content", () => {
    const newContent: EditableBlock[] = [
      {
        name: 'Test datablock',
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
          includeGeoJson: true,
        },
        charts: [],
        table: emptyTable,
        type: 'DataBlock',
        id: '69a9522d-501d-441a-9ee5-260ede5cd85c',
        order: 0,
        comments: [],
      },
      {
        type: 'MarkDownBlock',
        id: 'eb809a91-af3d-438d-632c-08d7c408aca5',
        order: 1,
        body: '',
        comments: [
          {
            id: 'comment-1',
            content: 'A comment',
            createdBy: {
              id: 'user-1',
              firstName: 'Bau1',
              lastName: '',
              email: 'bau1@test.com',
            },
            created: '2020-03-09T09:39:53.736',
          },
          {
            id: 'comment-2',
            content: 'another comment',
            createdBy: {
              id: 'user-1',
              firstName: 'Bau1',
              lastName: '',
              email: 'bau1@test.com',
            },
            created: '2020-03-09T09:40:16.534',
          },
        ],
      },
    ];

    const { release } = releaseReducer(
      {
        release: testEditableRelease,
        canUpdateRelease: true,
        availableDataBlocks: [basicDataBlock],
      },
      {
        type: 'UPDATE_SECTION_CONTENT',
        payload: {
          meta: {
            sectionId: testEditableRelease.content[0].id,
            sectionKey: 'content',
          },
          sectionContent: newContent,
        },
      },
    );

    expect(release?.content[0].content[0]).toEqual(newContent[0]);
    expect(release?.content[0].content[1]).toEqual(newContent[1]);
  });

  test('ADD_CONTENT_SECTION adds a new section to release content', () => {
    const originalLength = testEditableRelease.content.length;
    const { release } = releaseReducer(
      {
        release: testEditableRelease,
        canUpdateRelease: true,
        availableDataBlocks: [basicDataBlock],
      },
      {
        type: 'ADD_CONTENT_SECTION',
        payload: {
          section: {
            heading: 'A new section',
            id: 'new-section-1',
            order: testEditableRelease.content.length,
            content: [],
          },
        },
      },
    );

    expect(release?.content).toHaveLength(originalLength + 1);

    expect(release?.content[originalLength].id).toEqual('new-section-1');
  });

  test("SET_CONTENT sets the release's content", () => {
    const contentSectionsLength = testEditableRelease.content.length;
    const contentSectionsReversed = testEditableRelease.content.map(
      (section, index) => {
        return { ...section, order: contentSectionsLength - (1 + index) };
      },
    );

    const { release } = releaseReducer(
      {
        release: testEditableRelease,
        canUpdateRelease: true,
        availableDataBlocks: [basicDataBlock],
      },
      {
        type: 'SET_CONTENT',
        payload: {
          content: contentSectionsReversed,
        },
      },
    );

    expect(release?.content[0].order).toEqual(contentSectionsLength - 1);
    expect(release?.content[1].order).toEqual(contentSectionsLength - 2);
    expect(release?.content).toHaveLength(contentSectionsLength);
  });

  test('UPDATE_CONTENT_SECTION updates a content section', () => {
    const basicSection = testEditableRelease.content[0];
    const { release } = releaseReducer(
      {
        release: testEditableRelease,
        canUpdateRelease: true,
        availableDataBlocks: [basicDataBlock],
      },
      {
        type: 'UPDATE_CONTENT_SECTION',
        payload: {
          meta: { sectionId: basicSection.id },
          section: {
            ...basicSection,
            heading: 'updated heading',
          },
        },
      },
    );

    expect(release?.content[0].id).toEqual(basicSection.id);
    expect(release?.content[0].heading).toEqual('updated heading');
  });
});
