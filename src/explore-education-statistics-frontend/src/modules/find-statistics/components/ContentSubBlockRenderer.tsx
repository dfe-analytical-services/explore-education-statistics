import { ContentBlock } from '@common/services/publicationService';
import React from 'react';
import ReactMarkdown from 'react-markdown';
import DataBlock from '@common/modules/find-statistics/components/DataBlock';
import Link from '@frontend/components/Link';

interface Props {
  block: ContentBlock;
}

const ContentSubBlockRenderer = ({ block }: Props) => {
  switch (block.type) {
    case 'MarkDownBlock':
      return <ReactMarkdown className="govuk-body" source={block.body} />;
    case 'InsetTextBlock':
      return (
        <div className="govuk-inset-text">
          {block.heading && (
            <h3 className="govuk-heading-s">{block.heading}</h3>
          )}

          <ReactMarkdown className="govuk-body" source={block.body} />
        </div>
      );
    case 'DataBlock':
      return (
        <div className="dfe-content-overflow">
          <DataBlock
            {...block}
            additionalTabContent={
              <>
                <h2 className="govuk-heading-m govuk-!-margin-top-9">
                  Explore and edit this data online
                </h2>
                <p>Use our table tool to explore this data.</p>
                <Link to="/table-tool/" className="govuk-button">
                  Explore data
                </Link>
              </>
            }
          />
        </div>
      );
    default:
      return null;
  }
};

export default ContentSubBlockRenderer;
