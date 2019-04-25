import { Release } from '@common/services/publicationService';
import React, { Component } from 'react';
import EditableContentSubBlockRenderer from './EditableContentSubBlockRenderer';

interface Props {
  content: Release['content'][0]['content'];
  id?: string;
  editable?: boolean;
}

class EditableContentBlock extends Component<Props> {
  public render() {
    let { content, id = '' } = this.props;

    return content.length > 0 ? (
      content.map((block, index) => (
        <EditableContentSubBlockRenderer
          editable={this.props.editable}
          block={block}
          key={`${index}-${block.heading}-${block.type}`}
          id={id}
          index={index}
        />
      ))
    ) : (
      <div className="govuk-inset-text">
        There is no content for this section.
      </div>
    );
  }
}

export default EditableContentBlock;
