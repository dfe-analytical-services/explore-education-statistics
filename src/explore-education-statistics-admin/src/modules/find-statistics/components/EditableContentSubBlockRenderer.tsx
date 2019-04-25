import { PrototypeEditableContent } from '@admin/pages/prototypes/components/PrototypeEditableContent';
import { ContentBlock } from '@common/services/publicationService';
import marked from 'marked';
import React, { Component } from 'react';
import { Draggable } from 'react-beautiful-dnd';
import { DataBlock } from './DataBlock';

interface Props {
  block: ContentBlock;
  id: string;
  index: number;
  editable?: boolean;
}

class EditableContentSubBlockRenderer extends Component<Props> {
  public getBlockHTML(block: ContentBlock) {
    switch (block.type) {
      case 'MarkDownBlock':
        return (
          <PrototypeEditableContent
            editable={this.props.editable}
            content={`
         <p className="govuk-body">${marked(block.body)} </p>
      `}
          />
        );
      case 'InsetTextBlock':
        return (
          <div className="govuk-inset-text">
            <PrototypeEditableContent
              editable={this.props.editable}
              content={`
            ${
              block.heading
                ? '<h3 className="govuk-heading-s">' + block.heading + '</h3>'
                : ''
            }

            <p className="govuk-body">${marked(block.body)} </p>
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

  public render() {
    const { block, id, index } = this.props;

    return this.getBlockHTML(block);

    return (
      <Draggable draggableId={`draggable_block_${id}_${index}`} index={index}>
        {provided => (
          <div ref={provided.innerRef} {...provided.draggableProps}>
            <div className="drag-handle small" {...provided.dragHandleProps} />

            {this.getBlockHTML(block)}

            {provided.placeholder}
          </div>
        )}
      </Draggable>
    );
  }
}

export default EditableContentSubBlockRenderer;
