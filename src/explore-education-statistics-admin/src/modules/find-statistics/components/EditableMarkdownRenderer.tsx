import WysiwygEditor from '@admin/components/WysiwygEditor';
import { RendererProps } from '@admin/modules/find-statistics/PublicationReleaseContent';
import marked from 'marked';
import React from 'react';
import { ReactMarkdownProps } from 'react-markdown';

export type MarkdownRendererProps = RendererProps & ReactMarkdownProps;

const EditableMarkdownRenderer = ({ source }: MarkdownRendererProps) => {
  return (
    <>
      <WysiwygEditor
        content={marked(source || '')}
        editable
        onContentChange={ss => {
          // eslint-disable-next-line no-console
          console.log(ss);
        }}
      />
    </>
  );
};

export default EditableMarkdownRenderer;
