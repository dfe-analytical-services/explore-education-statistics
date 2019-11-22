import WysiwygEditor from '@admin/components/WysiwygEditor';
import { RendererProps } from '@admin/modules/find-statistics/PublicationReleaseContent';
import marked from 'marked';
import React from 'react';
import ReactMarkdown, { ReactMarkdownProps } from 'react-markdown';
import wrapEditableComponent, { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';

export type MarkdownRendererProps = RendererProps & ReactMarkdownProps;

const EditableMarkdownRenderer = ({ contentId, source }: MarkdownRendererProps) => {

  const editingContext = React.useContext(EditingContext);

  return (
    <>
      <WysiwygEditor
        content={source || ''}
        editable
        useMarkdown
        onContentChange={ss => {
          // eslint-disable-next-line no-console
          console.log(editingContext.releaseId);
          console.log(contentId);
          console.log(ss);


        }}
      />
    </>
  );
};

export default wrapEditableComponent(EditableMarkdownRenderer, ReactMarkdown);
