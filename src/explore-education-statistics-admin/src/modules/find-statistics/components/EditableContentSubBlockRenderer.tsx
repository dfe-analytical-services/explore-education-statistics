import React, {Component} from 'react';
import ReactMarkdown from 'react-markdown';
import marked from 'marked';

import { DataBlock } from './DataBlock';
import {ContentBlock} from "../../../services/publicationService";
import {PrototypeEditableContent} from "../../../prototypes/components/PrototypeEditableContent";
import {Draggable} from "react-beautiful-dnd";

interface Props {
  block: ContentBlock;
  id: string;
  index: number;
}

class EditableContentSubBlockRenderer extends Component<Props> {

  getBlockHTML(block : ContentBlock) {
    switch (block.type) {
      case 'MarkDownBlock':
        return <PrototypeEditableContent content={`
         <p className="govuk-body">${ marked(block.body) } </p>
      `}/>;
      case 'InsetTextBlock':
        return (
          <div className="govuk-inset-text">
            <PrototypeEditableContent content={`
            ${block.heading ? ('<h3 className="govuk-heading-s">'+block.heading+'</h3>') : "" }

            <p className="govuk-body">${ marked(block.body) } </p>
          `}/>
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

  render() {

    let {
      block,
      id,
      index
    } = this.props;

    //return this.getBlockHTML(block);

    return (
      <Draggable draggableId={`draggable_block_${id}_${index}`} index={index}>

        {(provided, snapshot) => (

          <div
            ref={provided.innerRef}
            {...provided.draggableProps}

          >
            <div className='drag-handle small' {...provided.dragHandleProps} />

            { this.getBlockHTML(block) }

            {provided.placeholder}
          </div>
        )}

      </Draggable>
    );



  }
}

export default EditableContentSubBlockRenderer;
