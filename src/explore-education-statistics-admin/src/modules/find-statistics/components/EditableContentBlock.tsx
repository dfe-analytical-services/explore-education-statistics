import { Release, ContentBlock } from '@common/services/publicationService';
import React, { Component } from 'react';
import { EditableRelease } from '@admin/services/publicationService';
import EditableContentSubBlockRenderer from './EditableContentSubBlockRenderer';
import AddComment from '../../../pages/prototypes/components/PrototypeEditableContentAddComment';
import ResolveComment from '../../../pages/prototypes/components/PrototypeEditableContentResolveComment';

interface Props {
  content: EditableRelease['content'][0]['content'];
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
          <React.Fragment key={key}>
            {reviewing && <AddComment initialComments={block.comments} />}
            {resolveComments && (
              <ResolveComment initialComments={block.comments} />
            )}
            <EditableContentSubBlockRenderer
              editable={editable}
              block={block}
              id={id}
              index={index}
              onContentChange={newContent => {
                if (onContentChange) {
                  onContentChange(block, newContent);
                }
              }}
            />
          </React.Fragment>
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
