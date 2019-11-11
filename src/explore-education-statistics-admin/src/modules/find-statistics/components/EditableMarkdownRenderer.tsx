import React from 'react';
import {RendererProps} from '@admin/modules/find-statistics/PublicationReleaseContent';
import ReactMarkdown, {ReactMarkdownProps} from 'react-markdown';
import WysiwygEditor from "@admin/components/WysiwygEditor";
import marked from "marked";

export type MarkdownRendererProps = RendererProps & ReactMarkdownProps;


const EditableMarkdownRenderer = ({source}: MarkdownRendererProps) => {
  return (
    <>
      <WysiwygEditor
        content={marked(source || '')}
        editable
        onContentChange={(ss) => {
          console.log(ss);
        }}
      />
    </>
  );
};

export default EditableMarkdownRenderer;