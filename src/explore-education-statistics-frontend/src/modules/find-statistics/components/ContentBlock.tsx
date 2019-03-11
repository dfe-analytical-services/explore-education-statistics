import React, { Component } from 'react';
import ContentBlockRenderer from './ContentBlockRenderer';

interface Props {
  content: Block[];
}

export interface Block {
  body: string;
  heading: string;
  order: number;
  type: string;
}

class ContentBlock extends Component<Props> {
  public render() {
    const { content } = this.props;

    return content.length > 0 ? (
      content.map(block => (
        <ContentBlockRenderer
          block={block}
          key={`${block.order}-${block.heading}`}
        />
      ))
    ) : (
      <div className="govuk-inset-text">
        There is no content for this section.
      </div>
    );
  }
}

export default ContentBlock;
