import PrototypeEditableContent from '@admin/pages/prototypes/components/PrototypeEditableContent';
import marked from 'marked';
import React from 'react';
import DataBlock from '@common/modules/find-statistics/components/DataBlock';
import { EditableContentBlock } from '@admin/services/publicationService';
import EditableHtmlRenderer from '@admin/modules/find-statistics/components/EditableHtmlRenderer';
import EditableMarkdownRenderer from '@admin/modules/find-statistics/components/EditableMarkdownRenderer';

interface Props {
  block: EditableContentBlock;
  id: string;
  index: number;
  editable?: boolean;
  canDelete: boolean;
  onContentChange?: (content: string) => void;
}

function EditableContentSubBlockRenderer({
  block,
  editable,
  onContentChange,
  id,
  canDelete,
}: Props) {

  console.log(block.type);

  switch (block.type) {
    case 'MarkDownBlock':
      return (
        <>
          <EditableMarkdownRenderer
            contentId={block.id}
            source={block.body}
            canDelete={canDelete}
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
      return (
        <EditableHtmlRenderer
          contentId={block.id}
          source={block.body}
          canDelete={canDelete}
        />
      );
    default:
      return <div>Unable to edit content type {block.type}</div>;
  }
}

export default EditableContentSubBlockRenderer;
