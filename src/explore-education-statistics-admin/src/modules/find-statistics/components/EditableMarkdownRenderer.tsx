import WysiwygEditor from '@admin/components/WysiwygEditor';
import { RendererProps } from '@admin/modules/find-statistics/PublicationReleaseContent';
import marked from 'marked';
import React from 'react';
import ReactMarkdown, { ReactMarkdownProps } from 'react-markdown';
import wrapEditableComponent from '@common/modules/find-statistics/util/wrapEditableComponent';

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

export default wrapEditableComponent(EditableMarkdownRenderer, ReactMarkdown);
