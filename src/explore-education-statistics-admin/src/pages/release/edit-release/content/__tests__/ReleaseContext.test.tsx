import { produce } from 'immer';
import {
  EditableContentBlock,
  EditableRelease,
} from '@admin/services/publicationService';
import {
  releaseReducer as originalReleaseReducer,
  ReleaseContextState,
} from '../ReleaseContext';
import ReleaseDispatchAction from '../ReleaseContextActionTypes';

const basicRelease: EditableRelease = {
  id: '6a97c9b6-eaa2-4d22-7ba9-08d7bec1ba1a',
  title: 'Academic Year 2020/21',
  yearTitle: '2020/21',
  coverageTitle: 'Academic Year',
  releaseName: '2020',
  slug: '2020-21',
  publicationId: '8586c490-7027-4a8b-9df5-cf105c3e88ec',
  status: 'Draft',
  publication: {
    id: '8586c490-7027-4a8b-9df5-cf105c3e88ec',
    title: 'My Pub',
    slug: 'my-pub',
    otherReleases: [],
    legacyReleases: [],
    dataSource: '',
    description: '',
    nextUpdate: '',
    summary: '',
    topic: { theme: { title: 'Children, early years and social care' } },
    contact: {
      teamName: 'Explore Education Statistics',
      teamEmail: 'explore.statistics@education.gov.uk',
      contactName: 'Cameron Race',
      contactTelNo: '07780991976',
    },
    methodology: {
      id: '8807af2b-d9a7-4efa-a376-08d7b451a98e',
      title: 'First methodology',
      slug: '',
      summary: '',
    },
  },
  latestRelease: false,
  type: {
    id: '9d333457-9132-4e55-ae78-c55cb3673d7c',
    // @ts-ignore
    title: 'Official Statistics',
  },
  updates: [
    {
      id: '262cf6c8-db96-40d8-8fb1-b55028a9f55b',
      reason: 'First published',
      releaseId: '',
      on: new Date(),
    },
  ],
  content: [
    {
      id: '30e57106-66e2-43a3-fc07-08d7bf65f293',
      order: 0,
      heading: 'New section 3',
      content: [
        {
          id: 'eb809a91-af3d-438d-632c-08d7c408aca5',
          body: '',
          type: 'MarkDownBlock',
          order: 0,
          comments: [
            {
              id: '814e3272-1a3e-4849-d53b-08d7c40dd247',
              name: 'Bau1',
              time: new Date('2020-03-09T09:39:53.736'),
              commentText: 'A comment',
              state: 'open',
            },
            {
              id: 'a4a6bd84-992b-44a0-d53c-08d7c40dd247',
              name: 'Bau1',
              time: new Date('2020-03-09T09:40:16.534'),
              commentText: 'another comment',
              state: 'open',
            },
          ],
        },
        {
          heading: "Table showing 'prma' from 'My Pub' in England for 2017/18",
          // @ts-ignore
          customFootnotes: '',
          name: 'DataBlock 1',
          source: '',
          dataBlockRequest: {
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
            country: ['E92000001'],
            includeGeoJson: true,
          },
          charts: [],
          tables: [],
          type: 'DataBlock',
          id: '69a9522d-501d-441a-9ee5-260ede5cd85c',
          order: 1,
          comments: [],
        },
      ],
    },
    {
      id: '47b7cabf-4855-44b8-5fdc-08d7c1e45526',
      order: 1,
      heading: 'New section',
      content: [],
      caption: '',
    },
  ],
  summarySection: {
    id: '6662107c-5c58-4ace-d1b1-08d7bec1ba1b',
    order: 0,
    content: [],
    heading: '',
    caption: '',
  },
  headlinesSection: {
    id: '25a6cfbc-2cb5-43da-d1b7-08d7bec1ba1b',
    heading: '',
    caption: '',
    order: 0,
    content: [
      {
        id: 'f27bf214-a244-41a7-b09b-08d7c1e10b96',
        type: 'MarkDownBlock',
        body: '',
        order: 0,
        comments: [],
      },
    ],
  },
  keyStatisticsSection: {
    id: '10e39b93-0304-43fb-d1b3-08d7bec1ba1b',
    order: 0,
    content: [
      {
        heading:
          "Table showing Main reason for issue: Unauthorised family holiday absence for 'prma' from 'My Pub' in England for 2017/18",
        // @ts-ignore
        customFootnotes: '',
        name: 'Holiday (Key Stat)',
        source: '',
        dataBlockRequest: {
          subjectId: '36aa28ce-83ca-49b5-8c27-b34e77b062c9',
          timePeriod: {
            startYear: 2017,
            startCode: 'AY',
            endYear: 2017,
            endCode: 'AY',
          },
          filters: ['5a1b6c93-c9c3-4710-f71f-08d7bec1ddb3'],
          indicators: ['272f4b12-9dfe-45ef-fdf9-08d7bec1dd71'],
          country: ['E92000001'],
          includeGeoJson: true,
        },
        charts: [],
        tables: [],
        type: 'DataBlock',
        id: 'e2db1389-8220-41f0-a29a-bb8dd1ccfc9c',
        order: 0,
        comments: [],
      },
      {
        heading:
          "Table showing Main reason for issue: Arriving late for 'prma' from 'My Pub' in England for 2017/18",
        customFootnotes: '',
        name: 'Main Reason (Key Stat)',
        source: '',
        dataBlockRequest: {
          subjectId: '36aa28ce-83ca-49b5-8c27-b34e77b062c9',
          timePeriod: {
            startYear: 2017,
            startCode: 'AY',
            endYear: 2017,
            endCode: 'AY',
          },
          filters: ['5a1b6c93-c9c3-4710-f71f-08d7bec1ddb3'],
          indicators: ['d314e2a9-5069-4d79-fdfa-08d7bec1dd71'],
          country: ['E92000001'],
          includeGeoJson: true,
        },
        charts: [],
        tables: [],
        type: 'DataBlock',
        id: 'aa9f1b63-abb3-41a2-bd57-fcf24dfd71ed',
        order: 1,
        comments: [],
      },
      {
        heading:
          "Table showing Main reason for issue: Absence due to other unauthorised circumstances for 'prma' from 'My Pub' in England for 2017/18",
        customFootnotes: '',
        name: "Unauth'd (Key Stat)",
        source: '',
        dataBlockRequest: {
          subjectId: '36aa28ce-83ca-49b5-8c27-b34e77b062c9',
          timePeriod: {
            startYear: 2017,
            startCode: 'AY',
            endYear: 2017,
            endCode: 'AY',
          },
          filters: ['5a1b6c93-c9c3-4710-f71f-08d7bec1ddb3'],
          indicators: ['214f5fb8-c12b-411e-fdfb-08d7bec1dd71'],
          country: ['E92000001'],
          includeGeoJson: true,
        },
        charts: [],
        tables: [],
        type: 'DataBlock',
        id: 'ba1ff405-1f44-4509-9dcf-5822df5b6f8c',
        order: 2,
        comments: [],
      },
    ],
  },
  keyStatisticsSecondarySection: {
    heading: '',
    caption: '',
    id: '5509024d-9389-40d1-d1b5-08d7bec1ba1b',
    order: 0,
    content: [
      {
        heading: "Table showing 'prma' from 'My Pub' in England for 2017/18",
        //  @ts-ignore
        customFootnotes: '',
        name: 'Aggregate table (Secondary)',
        source: '',
        dataBlockRequest: {
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
          ],
          country: ['E92000001'],
          includeGeoJson: true,
        },
        charts: [],
        tables: [],
        type: 'DataBlock',
        id: 'a3197018-66b6-4ce5-97fa-da2355270c40',
        order: 0,
        comments: [],
      },
    ],
  },
  downloadFiles: [
    {
      extension: 'csv',
      name: 'prma',
      path: '6a97c9b6-eaa2-4d22-7ba9-08d7bec1ba1a/data/prma.csv',
      size: '268 Kb',
    },
  ],
  publishScheduled: '2020-03-03T00:00:00',
  nextReleaseDate: { year: 2020, month: 3, day: 4 },
  relatedInformation: [],
};

const basicDataBlock = {
  id: 'datablock-0',
  dataBlockRequest: {
    filters: [],
    indicators: [],
    subjectId: 'subjectId',
  },
};

const releaseReducer = (
  initial: ReleaseContextState,
  action: ReleaseDispatchAction,
) => produce(initial, draft => originalReleaseReducer(draft, action));

describe('ReleaseContext', () => {
  test('CLEAR_STATE clears state', () => {
    expect(
      releaseReducer(
        {
          release: basicRelease,
          canUpdateRelease: true,
          availableDataBlocks: [basicDataBlock],
          unresolvedComments: [],
        },
        { type: 'CLEAR_STATE' },
      ),
    ).toEqual({
      release: undefined,
      canUpdateRelease: false,
      availableDataBlocks: [],
      unresolvedComments: [],
    });
  });

  test('SET_STATE sets the state', () => {
    expect(
      releaseReducer(
        {
          release: undefined,
          canUpdateRelease: false,
          availableDataBlocks: [],
          unresolvedComments: [],
        },
        {
          type: 'SET_STATE',
          payload: {
            release: basicRelease,
            canUpdateRelease: true,
            availableDataBlocks: [basicDataBlock],
            unresolvedComments: [],
          },
        },
      ),
    ).toEqual({
      release: basicRelease,
      canUpdateRelease: true,
      availableDataBlocks: [basicDataBlock],
      unresolvedComments: [],
    });
  });

  test('SET_AVAILABLE_DATABLOCKS sets datablocks', () => {
    expect(
      releaseReducer(
        {
          release: undefined,
          canUpdateRelease: false,
          availableDataBlocks: [],
          unresolvedComments: [],
        },
        {
          type: 'SET_AVAILABLE_DATABLOCKS',
          payload: {
            availableDataBlocks: [basicDataBlock],
          },
        },
      ),
    ).toEqual({
      release: undefined,
      canUpdateRelease: false,
      availableDataBlocks: [basicDataBlock],
      unresolvedComments: [],
    });
  });

  test('REMOVE_BLOCK_FROM_SECTION removes a block from a section', () => {
    const sectionKey = 'keyStatisticsSection';
    const keyStatsSection = basicRelease[sectionKey];
    const removingBlockId = (keyStatsSection.content as EditableContentBlock[])[0]
      .id;
    const originalLength = (keyStatsSection.content as EditableContentBlock[])
      .length;

    const { release } = releaseReducer(
      {
        release: basicRelease,
        canUpdateRelease: true,
        availableDataBlocks: [basicDataBlock],
        unresolvedComments: [],
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
    const releaseContent = basicRelease[sectionKey];
    const removingBlockId = (releaseContent[0]
      .content as EditableContentBlock[])[0].id;
    const originalLength = (releaseContent[0].content as EditableContentBlock[])
      .length;

    const { release } = releaseReducer(
      {
        release: basicRelease,
        canUpdateRelease: true,
        availableDataBlocks: [basicDataBlock],
        unresolvedComments: [],
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
    const section = basicRelease[sectionKey];
    const blockToUpdate = (section.content as EditableContentBlock[])[0];

    const newBody = 'This is some updated text!';

    const { release } = releaseReducer(
      {
        release: basicRelease,
        canUpdateRelease: true,
        availableDataBlocks: [basicDataBlock],
        unresolvedComments: [],
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
    const sectionId = basicRelease.content[0].id;
    const blockToUpdate = (basicRelease.content[0]
      .content as EditableContentBlock[])[0];

    const newBody = 'This is some updated text!';

    const { release } = releaseReducer(
      {
        release: basicRelease,
        canUpdateRelease: true,
        availableDataBlocks: [basicDataBlock],
        unresolvedComments: [],
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
    const section = basicRelease[sectionKey];
    const newBlock: EditableContentBlock = {
      id: '123',
      body: 'This section is empty...',
      comments: [],
      type: 'MarkDownBlock',
    };

    const originalLength = section.content?.length || 0;

    const { release } = releaseReducer(
      {
        release: basicRelease,
        canUpdateRelease: false,
        availableDataBlocks: [],
        unresolvedComments: [],
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
    const section = basicRelease.content[0];

    const newBlock: EditableContentBlock = {
      id: '123',
      body: 'This section is empty...',
      comments: [],
      type: 'MarkDownBlock',
    };

    const originalLength = section.content?.length || 0;

    const { release } = releaseReducer(
      {
        release: basicRelease,
        canUpdateRelease: true,
        availableDataBlocks: [basicDataBlock],
        unresolvedComments: [],
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
    const newContent: EditableContentBlock[] = [
      {
        heading: "Table showing 'prma' from 'My Pub' in England for 2017/18",
        body: '',
        dataBlockRequest: {
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
          country: ['E92000001'],
          includeGeoJson: true,
        },
        charts: [],
        tables: [],
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
            id: '814e3272-1a3e-4849-d53b-08d7c40dd247',
            name: 'Bau1',
            time: new Date('2020-03-09T09:39:53.736'),
            commentText: 'A comment',
            state: 'open',
          },
          {
            id: 'a4a6bd84-992b-44a0-d53c-08d7c40dd247',
            name: 'Bau1',
            time: new Date('2020-03-09T09:40:16.534'),
            commentText: 'another comment',
            state: 'open',
          },
        ],
      },
    ];

    const { release } = releaseReducer(
      {
        release: basicRelease,
        canUpdateRelease: true,
        availableDataBlocks: [basicDataBlock],
        unresolvedComments: [],
      },
      {
        type: 'UPDATE_SECTION_CONTENT',
        payload: {
          meta: {
            sectionId: basicRelease.content[0].id,
            sectionKey: 'content',
          },
          sectionContent: newContent,
        },
      },
    );

    expect(
      (release?.content[0].content as EditableContentBlock[])[0].order,
    ).toEqual(newContent[0].order);
    expect(
      (release?.content[0].content as EditableContentBlock[])[1].order,
    ).toEqual(newContent[1].order);
  });

  test('ADD_CONTENT_SECTION adds a new section to release content', () => {
    const originalLength = basicRelease.content.length;
    const { release } = releaseReducer(
      {
        release: basicRelease,
        canUpdateRelease: true,
        availableDataBlocks: [basicDataBlock],
        unresolvedComments: [],
      },
      {
        type: 'ADD_CONTENT_SECTION',
        payload: {
          section: {
            caption: '',
            heading: 'A new section',
            id: 'new-section-1',
            order: basicRelease.content.length,
          },
        },
      },
    );

    expect(release?.content).toHaveLength(originalLength + 1);

    expect(release?.content[originalLength].id).toEqual('new-section-1');
  });

  test("SET_CONTENT sets the release's content", () => {
    const contentSectionsLength = basicRelease.content.length;
    const contentSectionsReversed = basicRelease.content.map(
      (section, index) => {
        return { ...section, order: contentSectionsLength - (1 + index) };
      },
    );

    const { release } = releaseReducer(
      {
        release: basicRelease,
        canUpdateRelease: true,
        availableDataBlocks: [basicDataBlock],
        unresolvedComments: [],
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
    const basicSection = basicRelease.content[0];
    const { release } = releaseReducer(
      {
        release: basicRelease,
        canUpdateRelease: true,
        availableDataBlocks: [basicDataBlock],
        unresolvedComments: [],
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

    expect(release?.content[0].id).toEqual(basicSection.id);
    expect(release?.content[0].caption).toEqual('updated caption');
    expect(release?.content[0].heading).toEqual('updated heading');
  });
});
