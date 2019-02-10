import React, { Component } from 'react';
import ReactMarkdown from 'react-markdown';
import Accordion from '../../../components/Accordion';
import AccordionSection from '../../../components/AccordionSection';
import { contentApi } from '../../../services/api';

interface Props {
  content: Block[];
}

interface RendererProps {
  block: Block;
}

interface Block {
  heading: string;
  type: string;
  body: string;
}

class ContentBlock extends Component<Props> {
  public render() {
    const { content } = this.props;

    return (
      <>
        {content.length > 0 ? (
          <>
            {content.map(block => (
              <ContentBlockRenderer block={block} />
            ))}
          </>
        ) : (
          <div className="govuk-inset-text">No content.</div>
        )}
      </>
    );
  }
}

class ContentBlockRenderer extends Component<RendererProps> {
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

export default ContentBlock;
