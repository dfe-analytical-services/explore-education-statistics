import React, { Component } from 'react';
import ReactMarkdown from 'react-markdown';
import Accordion from '../../../components/Accordion';
import AccordionSection from '../../../components/AccordionSection';
import { contentApi } from '../../../services/api';

interface Props {
  content: Block[];
}

interface Block {
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
            {content.map(({type,body}) => (
              <ReactMarkdown className="govuk-body" source={body} />
            ))}
          </>
        ) : (
          <div className="govuk-inset-text">No content.</div>
        )}
      </>
    );
  }
}

export default ContentBlock;
