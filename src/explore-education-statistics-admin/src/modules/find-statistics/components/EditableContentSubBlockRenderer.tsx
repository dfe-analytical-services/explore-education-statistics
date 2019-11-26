import PrototypeEditableContent from '@admin/pages/prototypes/components/PrototypeEditableContent';
import marked from 'marked';
import React from 'react';
// import { Draggable } from 'react-beautiful-dnd';
import DataBlock from '@common/modules/find-statistics/components/DataBlock';
import WysiwygEditor from '@admin/components/WysiwygEditor';
import { EditableContentBlock } from '@admin/services/publicationService';
import EditableMarkdownRenderer from './EditableMarkdownRenderer';

interface Props {
  block: EditableContentBlock;
  id: string;
  index: number;
  editable?: boolean;
  onContentChange?: (content: string) => void;
}

function EditableContentSubBlockRenderer({
  block,
  editable,
  onContentChange,
  id,
}: Props) {
  switch (block.type) {
    case 'MarkDownBlock':
      return (
        <>
          <EditableMarkdownRenderer contentId={block.id} source={block.body} />
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
          <DataBlock
            {...block}
            id={`${id}_datablock`}
            additionalTabContent={
              <>
                <h2 className="govuk-heading-m govuk-!-margin-top-9">
                  Explore and edit this data online
                </h2>
                <p>Use our table tool to explore this data.</p>
                <a href="/table-tool/" className="govuk-button">
                  Explore data
                </a>
              </>
            }
          />
        </div>
      );
    case 'HtmlBlock':
      return <WysiwygEditor editable content={block.body} />;
    default:
      return <div>Unable to edit content type {block.type}</div>;
  }
}

export default EditableContentSubBlockRenderer;
