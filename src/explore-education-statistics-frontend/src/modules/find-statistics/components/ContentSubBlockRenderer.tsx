import DataBlock from '@common/modules/find-statistics/components/DataBlock';
import { ContentBlock, Publication } from '@common/services/publicationService';
import ButtonLink from '@frontend/components/ButtonLink';
import React from 'react';
import ReactMarkdown from 'react-markdown';

interface Props {
  block: ContentBlock;
  id: string;
  publication: Publication;

  onToggle?: (section: { id: string; title: string }) => void;
}

const ContentSubBlockRenderer = ({
  block,
  id,
  publication,
  onToggle,
}: Props) => {
  switch (block.type) {
    case 'MarkDownBlock':
      return <ReactMarkdown className="govuk-body" source={block.body} />;
    case 'HtmlBlock':
      // eslint-disable-next-line react/no-danger
      return <div dangerouslySetInnerHTML={{ __html: block.body }} />;
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
          additionalTabContent={
            <div className="dfe-print-hidden">
              <h2 className="govuk-heading-m">
                Explore and edit this data online
              </h2>
              <p>Use our table tool to explore this data.</p>
              <ButtonLink
                as={`/table-tool/${publication.slug}`}
                href={`/table-tool?publicationSlug=${publication.slug}`}
              >
                Explore data
              </ButtonLink>
            </div>
          }
          onToggle={onToggle}
        />
      );
    default:
      return null;
  }
};

export default ContentSubBlockRenderer;
