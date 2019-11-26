import { EditableRelease } from '@admin/services/publicationService';
import ContentBlock, {
  ContentBlockProps,
} from '@common/modules/find-statistics/components/ContentBlock';
import { ContentBlock as ContentBlockData } from '@common/services/publicationService';
import React from 'react';
import wrapEditableComponent, {
  EditingContext,
  ReleaseContentContext,
} from '@common/modules/find-statistics/util/wrapEditableComponent';
import AddComment from '../../../pages/prototypes/components/PrototypeEditableContentAddComment';
import ResolveComment from '../../../pages/prototypes/components/PrototypeEditableContentResolveComment';
import EditableContentSubBlockRenderer from './EditableContentSubBlockRenderer';

export interface Props extends ContentBlockProps {
  content: EditableRelease['content'][0]['content'];

  sectionId: string;

  editable?: boolean;
  reviewing?: boolean;
  resolveComments?: boolean;
  onContentChange?: (block: ContentBlockData, content: string) => void;
}

interface EditingContentBlockContext extends ReleaseContentContext {
  sectionId?: string;
}

export const EditingContentBlockContext = React.createContext<
  EditingContentBlockContext
>({
  releaseId: undefined,
  isEditing: false,
  sectionId: undefined,
});

const EditableContentBlock = ({
  content,
  id = '',
  sectionId,
  editable,
  onContentChange,
  reviewing,
  resolveComments,
}: Props) => {
  const editingContext = React.useContext(EditingContext);

  if (content.length === 0) {
    return (
      <div className="govuk-inset-text">
        There is no content for this section.
      </div>
    );
  }

  return (
    <EditingContentBlockContext.Provider
      value={{
        ...editingContext,
        sectionId,
      }}
    >
      {content.map((block, index) => {
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
      })}
    </EditingContentBlockContext.Provider>
  );
};

export default wrapEditableComponent(EditableContentBlock, ContentBlock);
