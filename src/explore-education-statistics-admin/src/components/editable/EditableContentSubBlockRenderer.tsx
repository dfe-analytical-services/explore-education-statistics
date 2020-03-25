import EditableDataBlock from '@admin/components/editable/EditableDataBlock';
import EditableHtmlRenderer from '@admin/components/editable/EditableHtmlRenderer';
import EditableMarkdownRenderer, {
  MarkdownRendererProps,
} from '@admin/components/editable/EditableMarkdownRenderer';
import { EditableContentBlock } from '@admin/services/publicationService';
import React from 'react';

interface Props extends MarkdownRendererProps {
  block: EditableContentBlock;
}

function EditableContentSubBlockRenderer({
  block,
  editable,
  releaseId,
  insideAccordion,
  onContentChange,
  canDelete = false,
  onDelete,
}: Props) {
  switch (block.type) {
    case 'MarkDownBlock':
      return (
        <EditableMarkdownRenderer
          editable={editable}
          contentId={block.id}
          insideAccordion={insideAccordion}
          source={block.body}
          canDelete={canDelete}
          onDelete={onDelete}
          onContentChange={onContentChange}
        />
      );
    case 'DataBlock':
      return (
        <div className="dfe-content-overflow">
          <EditableDataBlock
            editable={editable}
            {...block}
            canDelete={canDelete}
            onDelete={onDelete}
            releaseId={releaseId}
          />
        </div>
      );
    case 'HtmlBlock':
      return (
        <EditableHtmlRenderer
          editable={editable}
          insideAccordion={insideAccordion}
          contentId={block.id}
          source={block.body}
          canDelete={canDelete}
          onDelete={onDelete}
          onContentChange={onContentChange}
        />
      );
    default:
      return <div>Unable to edit content type {block.type}</div>;
  }
}

export default EditableContentSubBlockRenderer;
