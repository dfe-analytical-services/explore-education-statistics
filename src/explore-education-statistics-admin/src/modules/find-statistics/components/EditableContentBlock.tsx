import { Release, ContentBlock } from '@common/services/publicationService';
import React, { Component } from 'react';
import EditableContentSubBlockRenderer from './EditableContentSubBlockRenderer';
import AddComment from '../../../pages/prototypes/components/PrototypeEditableContentAddComment';
import ResolveComment from '../../../pages/prototypes/components/PrototypeEditableContentResolveComment';

interface Props {
  content: Release['content'][0]['content'];
  id?: string;
  editable?: boolean;
  reviewing?: boolean;
  resolveComments?: boolean;
  onContentChange?: (block: ContentBlock, content: string) => void;
}

class EditableContentBlock extends Component<Props> {
  public render() {
    const {
      content,
      id = '',
      editable,
      onContentChange,
      reviewing,
      resolveComments,
    } = this.props;

    return content.length > 0 ? (
      content.map((block, index) => {
        const key = `${index}-${block.heading}-${block.type}`;
        return (
          <>
            <EditableContentSubBlockRenderer
              editable={editable}
              block={block}
              key={key}
              id={id}
              index={index}
              onContentChange={newContent => {
                if (onContentChange) {
                  onContentChange(block, newContent);
                }
              }}
            />
          </>
        );
      })
    ) : (
      <div className="govuk-inset-text">
        There is no content for this section.
      </div>
    );
  }
}

export default EditableContentBlock;
