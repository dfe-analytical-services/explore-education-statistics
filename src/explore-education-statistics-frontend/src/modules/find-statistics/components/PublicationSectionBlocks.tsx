import ButtonLink from '@common/components/ButtonLink';
import SectionBlocks, {
  SectionBlocksProps,
} from '@common/modules/find-statistics/components/SectionBlocks';
import { Publication } from '@common/services/publicationService';
import { OmitStrict } from '@common/types';
import React from 'react';

export interface PublicationSectionBlocksProps
  extends OmitStrict<SectionBlocksProps, 'getInfographic'> {
  publication: Publication;
}

const PublicationSectionBlocks = ({
  publication,
  ...props
}: PublicationSectionBlocksProps) => {
  return (
    <SectionBlocks
      {...props}
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
            <ButtonLink as="/data-tables" to="/data-tables" href="/data-tables">
              Explore data
            </ButtonLink>
          )}
        </div>
      }
    />
  );
};

export default PublicationSectionBlocks;
