import ButtonLink from '@common/components/ButtonLink';
import DataBlock from '@common/modules/find-statistics/components/DataBlock';
import { Publication } from '@common/services/publicationService';
import { Block } from '@common/services/types/blocks';
import React from 'react';
import ReactMarkdown from 'react-markdown';

export type SectionToggleHandler = (section: {
  id: string;
  title: string;
}) => void;

interface Props {
  block?: Block;
  id: string;
  publication?: Publication;
  onToggle?: SectionToggleHandler;
}

const BlockRenderer = ({
  block,
  id,
  publication,
  onToggle,
}: Props) => {
  if (block === undefined) return null;
  switch (block.type) {
    case 'MarkDownBlock':
      return <ReactMarkdown className="govuk-body" source={block.body} />;
    case 'HtmlBlock':
      return (
        <div
          // eslint-disable-next-line react/no-danger
          dangerouslySetInnerHTML={{ __html: block.body }}
        />
      );
    case 'DataBlock':
      return (
        <DataBlock
          {...block}
          id={`${id}_datablock`}
          additionalTabContent={
            <div className="dfe-print-hidden">
              <h2 className="govuk-heading-m govuk-!-margin-top-9">
                Explore and edit this data online
              </h2>
              <p>Use our table tool to explore this data.</p>
              {publication ? (
                <ButtonLink
                  as={`/data-tables/${publication.slug}`}
                  to={`/data-tables?publicationSlug=${publication.slug}`}
                  href={`/data-tables?publicationSlug=${publication.slug}`}
                >
                  Explore data
                </ButtonLink>
              ) : (
                <ButtonLink
                  as="/data-tables"
                  to="/data-tables"
                  href="/data-tables"
                >
                  Explore data
                </ButtonLink>
              )}
            </div>
          }
          onToggle={onToggle}
        />
      );
    default:
      return null;
  }
};

export default BlockRenderer;
