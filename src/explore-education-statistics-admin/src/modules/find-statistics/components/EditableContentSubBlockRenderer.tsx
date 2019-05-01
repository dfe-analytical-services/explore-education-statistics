import PrototypeEditableContent from '@admin/pages/prototypes/components/PrototypeEditableContent';
import { ContentBlock } from '@common/services/publicationService';
import marked from 'marked';
import React, { Component } from 'react';
// import { Draggable } from 'react-beautiful-dnd';
import { DataBlock } from './DataBlock';

interface Props {
  block: ContentBlock;
  id: string;
  index: number;
  editable?: boolean;
  onContentChange?: (content: string) => void;
}

class EditableContentSubBlockRenderer extends Component<Props> {
  public render() {
    const { block, editable, onContentChange } = this.props;

    switch (block.type) {
      case 'MarkDownBlock':
        return (
          <PrototypeEditableContent
            editable={editable}
            onContentChange={onContentChange}
            content={`
         <div className="govuk-body">${marked(block.body)} </div>
      `}
          />
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
            <DataBlock {...block} />
          </div>
        );
      default:
        return null;
    }
  }
}

export default EditableContentSubBlockRenderer;
