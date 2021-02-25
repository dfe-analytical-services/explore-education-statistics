import InsetText from '@common/components/InsetText';
import useGetChartFile from '@common/modules/charts/hooks/useGetChartFile';
import ContentBlockRenderer from '@common/modules/find-statistics/components/ContentBlockRenderer';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';
import useReleaseImageAttributeTransformer from '@common/modules/release/hooks/useReleaseImageAttributeTransformer';
import { Release } from '@common/services/publicationService';
import { Block } from '@common/services/types/blocks';
import ButtonLink from '@frontend/components/ButtonLink';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import React from 'react';

export interface PublicationSectionBlocksProps {
  release: Release;
  blocks: Block[];
}

const PublicationSectionBlocks = ({
  release,
  blocks,
}: PublicationSectionBlocksProps) => {
  const { slug, publication } = release;

  const getChartFile = useGetChartFile(publication.slug, slug);

  const transformImageAttributes = useReleaseImageAttributeTransformer({
    releaseId: release.id,
    rootUrl: process.env.CONTENT_API_BASE_URL,
  });

  return blocks.length > 0 ? (
    <>
      {blocks.map(block => {
        if (block.type === 'DataBlock') {
          return (
            <DataBlockTabs
              key={block.id}
              dataBlock={block}
              releaseId={release.id}
              getInfographic={getChartFile}
              onToggle={section => {
                logEvent(
                  'Publication Release Data Tabs',
                  `${section.title} (${section.id}) tab opened`,
                  window.location.pathname,
                );
              }}
              additionalTabContent={
                <div className="dfe-print-hidden">
                  <h3 className="govuk-heading-m">
                    Explore and edit this data online
                  </h3>

                  <p>Use our table tool to explore this data.</p>

                  <ButtonLink
                    to="/data-tables/fast-track/[fastTrackId]"
                    as={`/data-tables/fast-track/${block.id}`}
                    onClick={() => {
                      logEvent(
                        `Publication Release Data Tabs`,
                        `Explore data button clicked`,
                        `Explore data block name: ${block.name}`,
                      );
                    }}
                  >
                    Explore data
                  </ButtonLink>
                </div>
              }
            />
          );
        }

        return (
          <ContentBlockRenderer
            key={block.id}
            block={block}
            transformImageAttributes={transformImageAttributes}
          />
        );
      })}
    </>
  ) : (
    <InsetText>There is no content for this section.</InsetText>
  );
};

export default PublicationSectionBlocks;
