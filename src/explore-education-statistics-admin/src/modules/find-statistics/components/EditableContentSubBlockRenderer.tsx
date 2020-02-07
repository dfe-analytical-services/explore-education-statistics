import PrototypeEditableContent from '@admin/pages/prototypes/components/PrototypeEditableContent';
import marked from 'marked';
import React from 'react';
import { EditableContentBlock } from '@admin/services/publicationService';
import EditableHtmlRenderer from '@admin/modules/find-statistics/components/EditableHtmlRenderer';
import EditableMarkdownRenderer from '@admin/modules/find-statistics/components/EditableMarkdownRenderer';
import EditableDataBlock from '@admin/modules/find-statistics/components/EditableDataBlock';

interface Props {
  block: EditableContentBlock;
  id: string;
  index: number;
  editable?: boolean;
  canDelete?: boolean;
  onContentChange?: (content: string) => void;
  onDelete?: () => void;
}

function EditableContentSubBlockRenderer({
  block,
  editable,
  onContentChange,
  canDelete = false,
  onDelete,
}: Props) {
  switch (block.type) {
    case 'MarkDownBlock':
      return (
        <>
          <EditableMarkdownRenderer
            editable={editable}
            contentId={block.id}
            source={block.body}
            canDelete={canDelete}
            onDelete={onDelete}
            onContentChange={onContentChange}
          />
        </>
      );
    case 'InsetTextBlock':
      return (
        <div className="govuk-inset-text">
          <PrototypeEditableContent
            editable={editable}
            onContentChange={onContentChange}
            content={`
            ${
              block.heading
                ? `<h3 className="govuk-heading-s">${block.heading}</h3>`
                : ''
            }

            <div className="govuk-body">${marked(block.body)} </div>
          `}
          />
        </div>
      );
    case 'DataBlock':
      return (
        <div className="dfe-content-overflow">
          <EditableDataBlock
            canDelete={canDelete}
            onDelete={onDelete}
            {...block}
          />
        </div>
      );
    case 'HtmlBlock':
      return (
        <EditableHtmlRenderer
          editable={editable}
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
