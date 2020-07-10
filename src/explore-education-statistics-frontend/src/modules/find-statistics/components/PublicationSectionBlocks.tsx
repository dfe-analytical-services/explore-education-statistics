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
      additionalTabContent={({ dataBlock }) => (
        <div className="dfe-print-hidden">
          <h3 className="govuk-heading-m">Explore and edit this data online</h3>

          <p>Use our table tool to explore this data.</p>

          <ButtonLink
            to="/data-tables/fast-track/[fastTrackId]"
            as={`/data-tables/fast-track/${dataBlock.id}`}
          >
            Explore data
          </ButtonLink>
        </div>
      )}
    />
  );
};

export default PublicationSectionBlocks;
