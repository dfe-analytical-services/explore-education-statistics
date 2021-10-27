import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useToggle from '@common/hooks/useToggle';
import useGetReleaseFile from '@common/modules/release/hooks/useGetReleaseFile';
import ContentBlockRenderer from '@common/modules/find-statistics/components/ContentBlockRenderer';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';
import useReleaseImageAttributeTransformer from '@common/modules/release/hooks/useReleaseImageAttributeTransformer';
import { Release } from '@common/services/publicationService';
import { Block } from '@common/services/types/blocks';
import glossaryService from '@frontend/services/glossaryService';
import ButtonLink from '@frontend/components/ButtonLink';
import {
  logEvent,
  logOutboundLink,
} from '@frontend/services/googleAnalyticsService';
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

  const [exploreDataButtonClicked, toggleExploreDataButtonClicked] = useToggle(
    false,
  );

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
                    disabled={exploreDataButtonClicked}
                    onClick={() => {
                      toggleExploreDataButtonClicked.on();
                      logEvent({
                        category: `Publication Release Data Tabs`,
                        action: `Explore data button clicked`,
                        label: `Explore data block name: ${block.name}`,
                      });
                    }}
                  >
                    Explore data
                  </ButtonLink>
                  <LoadingSpinner
                    alert
                    inline
                    hideText
                    loading={exploreDataButtonClicked}
                    size="md"
                    text="Loading data"
                    className="govuk-!-margin-left-2"
                  />
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
            getGlossaryEntry={glossaryService.getEntry}
            trackContentLinks={url =>
              logOutboundLink(`Publication release content link: ${url}`, url)
            }
            trackGlossaryLinks={glossaryEntrySlug =>
              logEvent({
                category: `Publication Release Content Glossary Link`,
                action: `Glossary link clicked`,
                label: glossaryEntrySlug,
              })
            }
          />
        );
      })}
    </>
  ) : (
    <InsetText>There is no content for this section.</InsetText>
  );
};

export default PublicationSectionBlocks;
