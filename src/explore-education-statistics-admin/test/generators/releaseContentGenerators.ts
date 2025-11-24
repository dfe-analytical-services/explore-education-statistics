import {
  EditableRelease,
  ReleaseContent,
} from '@admin/services/releaseContentService';
import {
  EditableContentBlock,
  EditableDataBlock,
  EditableEmbedBlock,
} from '@admin/services/types/content';
import {
  generateContentSection,
  generateEditableContentBlock,
  generateEditableDataBlock,
} from '@admin-test/generators/contentGenerators';
import {
  Publication,
  ContentSection,
  KeyStatistic,
  BasicLink,
  ReleaseNote,
} from '@common/services/publicationService';
import { FileInfo } from '@common/services/types/file';

const defaultContent: ContentSection<
  EditableContentBlock | EditableDataBlock | EditableEmbedBlock
>[] = [
  generateContentSection({
    content: [
      generateEditableContentBlock({
        body: '<p>Content block 1 body</p>',
        id: 'content-block-1',
        order: 0,
      }),
      generateEditableContentBlock({
        body: '<p>Content block 2 body</p>',
        id: 'content-block-2',
        order: 0,
      }),
    ],
    heading: 'Content section 1',
    id: 'content-section-1-id',
  }),
  generateContentSection({
    content: [
      generateEditableContentBlock({
        body: '<p>Content block 3 body</p>',
        id: 'content-block-3',
        order: 0,
      }),
    ],
    heading: 'Content section 3',
    id: 'content-section-2-id',
    order: 1,
  }),
];

const defaultDownloadFiles: FileInfo[] = [
  {
    id: 'download-file-id',
    extension: 'csv',
    fileName: 'File name',
    name: 'Download file name',
    size: '100kb',
    type: 'DataZip',
  },
];

const defaultHeadlinesSection: ContentSection<EditableContentBlock> = {
  id: 'headlines-section-id',
  heading: 'Headlines section heading',
  order: 0,
  content: [
    generateEditableContentBlock({
      body: '<p>Headlines block body</p>',
      id: 'headlines-block',
      order: 0,
    }),
  ],
};

export const defaultPublication: Publication = {
  contact: {
    contactName: 'Contact name',
    teamEmail: 'contact@test.com',
    teamName: 'Team name',
  },
  id: 'publication-id',
  methodologies: [
    {
      id: 'methodology-id',
      title: 'Methodology title',
      slug: 'methodology-slug',
    },
  ],
  releaseSeries: [
    {
      isLegacyLink: true,
      description: 'legacy link 1',
      legacyLinkUrl: 'https://test.com/1',
      releaseSlug: 'test-release-slug',
    },
  ],
  slug: 'publication-slug',
  title: 'Publication title',
  theme: { id: 'test-theme', title: 'Test theme' },
};

const defaultKeyStatistics: KeyStatistic[] = [
  {
    id: 'key-statistic-id',
    statistic: 'Statistic',
    title: 'Key statistic',
    type: 'KeyStatisticText',
  },
];

const defaultKeyStatisticsSecondarySection: ContentSection<EditableDataBlock> =
  {
    id: 'key-statistics-secondary-section-id',
    content: [generateEditableDataBlock({})],
    heading: 'Key statistics secondary section heading',
    order: 0,
  };

const defaultRelatedInformation: BasicLink[] = [
  {
    id: 'related-information-id',
    description: 'Related information description',
    url: 'https://test.com',
  },
];

const defaultSummarySection: ContentSection<EditableContentBlock> = {
  id: 'summary-section-id',
  content: [
    generateEditableContentBlock({
      body: '<p>Summary block body</p>',
      id: 'summary-block-id',
      order: 0,
    }),
  ],
  heading: 'Summary block heading',
  order: 0,
};

const defaultUpdates: ReleaseNote[] = [
  {
    id: 'update-id',
    on: new Date('2020-01-01'),
    reason: 'Update reason',
  },
];

export function generateEditableRelease({
  approvalStatus = 'Draft',
  content = defaultContent,
  coverageTitle = 'Academic year',
  downloadFiles = defaultDownloadFiles,
  hasDataGuidance = true,
  hasPreReleaseAccessList = false,
  headlinesSection = defaultHeadlinesSection,
  id,
  keyStatistics = defaultKeyStatistics,
  keyStatisticsSecondarySection = defaultKeyStatisticsSecondarySection,
  latestRelease = false,
  nextReleaseDate,
  publication = defaultPublication,
  published,
  publishScheduled,
  relatedDashboardsSection,
  relatedInformation = defaultRelatedInformation,
  title = 'Release title',
  slug,
  summarySection = defaultSummarySection,
  type = 'OfficialStatistics',
  updates = defaultUpdates,
  yearTitle = '2020/21',
}: Partial<EditableRelease>): EditableRelease {
  const releaseSlug = title.replaceAll(' ', '-');
  return {
    approvalStatus,
    content,
    coverageTitle,
    downloadFiles,
    hasDataGuidance,
    hasPreReleaseAccessList,
    headlinesSection,
    id: id ?? `${releaseSlug}-id`,
    keyStatistics,
    keyStatisticsSecondarySection,
    latestRelease,
    nextReleaseDate,
    publicationId: publication.id,
    publication,
    published,
    publishScheduled,
    relatedDashboardsSection,
    relatedInformation,
    slug: slug ?? releaseSlug,
    summarySection,
    title,
    type,
    updates,
    yearTitle,
  };
}

export default function generateReleaseContent({
  release = generateEditableRelease({}),
  unattachedDataBlocks = [],
}: Partial<ReleaseContent>): ReleaseContent {
  return {
    release,
    unattachedDataBlocks,
  };
}
