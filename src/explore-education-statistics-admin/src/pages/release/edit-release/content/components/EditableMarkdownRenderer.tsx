import React from 'react';
import { TextRendererProps, MarkdownRendererProps } from '@common/modules/find-statistics/PublicationReleaseContent';
import ReactMarkdown from 'react-markdown';

const EditableMarkdownRenderer = ({ source }: MarkdownRendererProps) => {
  return (
    <ReactMarkdown className="govuk-body" source={source} />
  );
};

export default EditableMarkdownRenderer;