import DataBlock from '@common/modules/find-statistics/components/DataBlock';
import { ContentBlock } from '@common/services/publicationService';
import Link from '@frontend/components/Link';
import React from 'react';
import ReactMarkdown from 'react-markdown';

interface Props {
  block: ContentBlock;
  id: string;
  refreshCallback?: (callback: () => void) => void;
}

const ContentSubBlockRenderer = ({ block, id, refreshCallback }: Props) => {
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
        <DataBlock
          {...block}
          id={`${id}_datablock`}
          refreshCallback={refreshCallback}
          additionalTabContent={
            <>
              <h2 className="govuk-heading-m">
                Explore and edit this data online
              </h2>
              <p>Use our table tool to explore this data.</p>
              <Link to="/table-tool/" className="govuk-button">
                Explore data
              </Link>
            </>
          }
        />
      );
    default:
      return null;
  }
};

export default ContentSubBlockRenderer;
