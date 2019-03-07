import React, { Component } from 'react';
import ReactMarkdown from 'react-markdown';
import { Block } from './ContentBlock';

interface ContentBlockRendererProps {
  block: Block;
}

class ContentBlockRenderer extends Component<ContentBlockRendererProps> {
  public render() {
    const { block } = this.props;

    switch (block.type) {
      case 'MarkDownBlock':
        return <ReactMarkdown className="govuk-body" source={block.body} />;
      case 'InsetTextBlock':
        return (
          <div className="govuk-inset-text">
            <h3 className="govuk-heading-s">{block.heading}</h3>
            <ReactMarkdown className="govuk-body" source={block.body} />
          </div>
        );
      default:
        return null;
    }
  }
}

export default ContentBlockRenderer;
