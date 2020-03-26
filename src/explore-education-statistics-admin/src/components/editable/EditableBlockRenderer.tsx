import EditableDataBlock from '@admin/components/editable/EditableDataBlock';
import EditableHtmlRenderer from '@admin/components/editable/EditableHtmlRenderer';
import EditableMarkdownRenderer, {
  MarkdownRendererProps,
} from '@admin/components/editable/EditableMarkdownRenderer';
import { useManageReleaseContext } from '@admin/pages/release/ManageReleaseContext';
import { EditableBlock } from '@admin/services/publicationService';
import React from 'react';

interface Props extends MarkdownRendererProps {
  block: EditableBlock;
}

function EditableBlockRenderer({
  block,
  editable,
  insideAccordion,
  onContentChange,
  canDelete = false,
  onDelete,
}: Props) {
  const { releaseId } = useManageReleaseContext();

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
            id={`editableBlockRenderer-dataBlock-${block.id}`}
            dataBlock={block}
            editable={editable}
            releaseId={releaseId}
            canDelete={canDelete}
            onDelete={onDelete}
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
      return <div>Unable to edit content</div>;
  }
}

export default EditableBlockRenderer;
