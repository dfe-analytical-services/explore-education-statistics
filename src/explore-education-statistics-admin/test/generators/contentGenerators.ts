import {
  EditableContentBlock,
  EditableDataBlock,
  EditableEmbedBlock,
} from '@admin/services/types/content';
import { ContentSection } from '@common/services/publicationService';
import { TableDataQuery } from '@common/services/tableBuilderService';
import { Table } from '@common/services/types/blocks';

export function generateContentSection({
  content = [],
  heading = 'Content section heading',
  id = 'content-section-id',
  order = 0,
}: Partial<
  ContentSection<EditableContentBlock | EditableDataBlock | EditableEmbedBlock>
>): ContentSection<
  EditableContentBlock | EditableDataBlock | EditableEmbedBlock
> {
  return {
    id,
    heading,
    order,
    content,
  };
}

export function generateEditableContentBlock({
  body = '<p>Content block body</p>',
  comments = [],
  id = 'content-block-id',
  locked,
  lockedUntil,
  lockedBy,
  order = 0,
}: Partial<EditableContentBlock>): EditableContentBlock {
  return {
    body,
    comments,
    id,
    locked,
    lockedUntil,
    lockedBy,
    order,
    type: 'HtmlBlock',
  };
}

export function generateEditableEmbedBlock({
  comments = [],
  id = 'embed-block-id',
  locked,
  lockedUntil,
  lockedBy,
  order = 0,
  title = 'Embed block title',
  url = 'https://department-for-education.shinyapps.io/test-dashboard',
}: Partial<EditableEmbedBlock>): EditableEmbedBlock {
  return {
    comments,
    id,
    locked,
    lockedUntil,
    lockedBy,
    order,
    title,
    type: 'EmbedBlockLink',
    url,
  };
}

const defaultDataBlockQuery: TableDataQuery = {
  subjectId: 'subject-id',
  filters: ['filter-id'],
  indicators: ['indicator-id'],
  locationIds: ['location-id'],
};

const defaultTable: Table = {
  tableHeaders: {
    columnGroups: [],
    columns: [],
    rowGroups: [],
    rows: [],
  },
};

export function generateEditableDataBlock({
  charts = [],
  comments = [],
  dataBlockParentId = 'data-block-parent-id',
  dataSetId = 'data-set-id',
  heading = 'Data block heading',
  id = 'data-block-id',
  locked,
  lockedUntil,
  lockedBy,
  name = 'Data block name',
  order = 0,
  query = defaultDataBlockQuery,
  source = 'Data block source',
  table = defaultTable,
}: Partial<EditableDataBlock>): EditableDataBlock {
  return {
    charts,
    comments,
    dataBlockParentId,
    dataSetId,
    heading,
    id,
    locked,
    lockedUntil,
    lockedBy,
    name,
    order,
    query,
    source,
    table,
    type: 'DataBlock',
  };
}
