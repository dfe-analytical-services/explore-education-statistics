import Gate from '@common/components/Gate';
import InsetText from '@common/components/InsetText';
import useGetReleaseFile from '@common/modules/release/hooks/useGetReleaseFile';
import ContentBlockRenderer from '@common/modules/find-statistics/components/ContentBlockRenderer';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';
import EmbedBlock from '@common/modules/find-statistics/components/EmbedBlock';
import ExploreDataButton from '@frontend/modules/find-statistics/components/ExploreDataButton';
import useReleaseImageAttributeTransformer from '@common/modules/release/hooks/useReleaseImageAttributeTransformer';
import { Release } from '@common/services/publicationService';
import { Block } from '@common/services/types/blocks';
import glossaryService from '@frontend/services/glossaryService';
import {
  logEvent,
  logOutboundLink,
} from '@frontend/services/googleAnalyticsService';
import React from 'react';
import VisuallyHidden from '@common/components/VisuallyHidden';

export interface PublicationSectionBlocksProps {
  release: Release;
  blocks: Block[];
  visible?: boolean;
}

const PublicationSectionBlocks = ({
  release,
  blocks,
  visible,
}: PublicationSectionBlocksProps) => {
  const getReleaseFile = useGetReleaseFile(release.id);

  const transformImageAttributes = useReleaseImageAttributeTransformer({
    releaseId: release.id,
    rootUrl: process.env.CONTENT_API_BASE_URL.replace('/api', ''),
  });

  return blocks.length > 0 ? (
    <>
      {blocks.map(block => {
        if (block.type === 'EmbedBlockLink') {
          return (
            <Gate condition={!!visible} key={block.id}>
              <EmbedBlock url={block.url} title={block.title} />
            </Gate>
          );
        }

        if (block.type === 'DataBlock') {
          return (
            <Gate condition={!!visible} key={block.id}>
              <DataBlockTabs
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
                      <VisuallyHidden>{` for ${block.heading}`}</VisuallyHidden>
                    </h3>

                    <p>Use our table tool to explore this data.</p>
                    <ExploreDataButton
                      block={block}
                      hiddenText={`for ${block.heading}`}
                    />
                  </div>
                }
              />
            </Gate>
          );
        }

        return (
          <ContentBlockRenderer
            key={block.id}
            block={block}
            transformImageAttributes={transformImageAttributes}
            getGlossaryEntry={glossaryService.getEntry}
            trackContentLinks={(url, newTab) => {
              logOutboundLink(
                `Publication release content link: ${url}`,
                url,
                newTab,
              );
            }}
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
