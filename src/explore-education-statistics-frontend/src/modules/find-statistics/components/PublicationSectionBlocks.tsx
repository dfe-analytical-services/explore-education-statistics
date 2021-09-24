import InsetText from '@common/components/InsetText';
import useGetReleaseFile from '@common/modules/release/hooks/useGetReleaseFile';
import ContentBlockRenderer from '@common/modules/find-statistics/components/ContentBlockRenderer';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';
import useReleaseImageAttributeTransformer from '@common/modules/release/hooks/useReleaseImageAttributeTransformer';
import { Release } from '@common/services/publicationService';
import { Block } from '@common/services/types/blocks';
import glossaryService from '@frontend/services/glossaryService';
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
  const getReleaseFile = useGetReleaseFile(release.id);

  const transformImageAttributes = useReleaseImageAttributeTransformer({
    releaseId: release.id,
    rootUrl: process.env.CONTENT_API_BASE_URL.replace('/api', ''),
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
              getInfographic={getReleaseFile}
              onToggle={section => {
                logEvent({
                  category: 'Publication Release Data Tabs',
                  action: `${section.title} (${block.name}) tab opened`,
                  label: window.location.pathname,
                });
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
                      logEvent({
                        category: `Publication Release Data Tabs`,
                        action: `Explore data button clicked`,
                        label: `Explore data block name: ${block.name}`,
                      });
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
            getGlossaryEntry={glossaryService.getGlossaryEntry}
          />
        );
      })}
    </>
  ) : (
    <InsetText>There is no content for this section.</InsetText>
  );
};

export default PublicationSectionBlocks;
