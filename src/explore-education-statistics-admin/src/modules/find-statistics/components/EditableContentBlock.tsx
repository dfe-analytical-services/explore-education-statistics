import { Release } from '@common/services/publicationService';
import React from 'react';
import EditableContentSubBlockRenderer from './EditableContentSubBlockRenderer';

interface Props {
  content: Release['content'][0]['content'];
  id?: string;
}

function EditableContentBlock({ content, id = '' }: Props) {
  return (
    <>
      {content.length > 0 ? (
        content.map((block, index) => {
          const key = `${index}_${block.heading}_${block.type}`;

          return (
            <EditableContentSubBlockRenderer
              block={block}
              key={key}
              id={id}
              index={index}
            />
          );
        })
      ) : (
        <div className="govuk-inset-text">
          There is no content for this section.
        </div>
      )}
    </>
  );
}

export default EditableContentBlock;
