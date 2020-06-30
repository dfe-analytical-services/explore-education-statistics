import useGetChartFile from '@common/modules/charts/hooks/useGetChartFile';
import SectionBlocks, {
  SectionBlocksProps,
} from '@common/modules/find-statistics/components/SectionBlocks';
import { Release } from '@common/services/publicationService';
import ButtonLink from '@frontend/components/ButtonLink';
import React from 'react';

export interface PublicationSectionBlocksProps extends SectionBlocksProps {
  release: Release;
}

const PublicationSectionBlocks = ({
  release: { slug, publication },
  ...props
}: PublicationSectionBlocksProps) => {
  const getChartFile = useGetChartFile(publication.slug, slug);

  return (
    <SectionBlocks
      {...props}
      queryOptions={{
        expiresIn: 60 * 60 * 24,
      }}
      getInfographic={getChartFile}
      additionalTabContent={
        <div className="dfe-print-hidden">
          <h2 className="govuk-heading-m govuk-!-margin-top-9">
            Explore and edit this data online
          </h2>
          <p>Use our table tool to explore this data.</p>
          {publication ? (
            <ButtonLink
              to="/data-tables/[publication]"
              as={`/data-tables/${publication.slug}`}
            >
              Explore data
            </ButtonLink>
          ) : (
            <ButtonLink href="/data-tables">Explore data</ButtonLink>
          )}
        </div>
      }
    />
  );
};

export default PublicationSectionBlocks;
