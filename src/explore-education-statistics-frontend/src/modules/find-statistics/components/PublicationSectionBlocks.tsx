import Gate from '@common/components/Gate';
import InsetText from '@common/components/InsetText';
import useGetReleaseFile from '@common/modules/release/hooks/useGetReleaseFile';
import ContentBlockRenderer from '@common/modules/find-statistics/components/ContentBlockRenderer';
import DataBlockTabs from '@frontend/modules/find-statistics/components/DataBlockTabs';
import EmbedBlock from '@common/modules/find-statistics/components/EmbedBlock';
import ExploreDataButton from '@frontend/modules/find-statistics/components/ExploreDataButton';
import useReleaseImageAttributeTransformer from '@common/modules/release/hooks/useReleaseImageAttributeTransformer';
import { BlockViewModel } from '@common/services/publicationService';
import { Block } from '@common/services/types/blocks';
import glossaryService from '@frontend/services/glossaryService';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import React from 'react';
import VisuallyHidden from '@common/components/VisuallyHidden';

export interface PublicationSectionBlocksProps {
  releaseVersionId: string;
  blocks: Block[] | BlockViewModel[];
  visible?: boolean;
}

const PublicationSectionBlocks = ({
  releaseVersionId,
  blocks,
  visible,
}: PublicationSectionBlocksProps) => {
  const getReleaseFile = useGetReleaseFile(releaseVersionId);

  const transformImageAttributes = useReleaseImageAttributeTransformer({
    releaseVersionId,
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

        if (block.type === 'EmbedBlock') {
          return (
            <Gate condition={!!visible} key={block.id}>
              <EmbedBlock
                url={block.embedBlock.url}
                title={block.embedBlock.title}
              />
            </Gate>
          );
        }

        if (block.type === 'DataBlock') {
          const dataBlock =
            'dataBlockVersion' in block
              ? {
                  id: block.id,
                  type: block.type,
                  ...block.dataBlockVersion,
                }
              : block;
          return (
            <Gate condition={!!visible} key={block.id}>
              <DataBlockTabs
                dataBlock={dataBlock}
                dataBlockStaleTime={Infinity}
                releaseVersionId={releaseVersionId}
                getInfographic={getReleaseFile}
                onToggle={section => {
                  logEvent({
                    category: 'Publication Release Data Tabs',
                    action: `${section.title} (${dataBlock.name}) tab opened`,
                    label: window.location.pathname,
                  });
                }}
                additionalTabContent={
                  <div className="govuk-!-display-none-print">
                    <h3 className="govuk-heading-m">
                      Explore and edit this data online
                      <VisuallyHidden>{` for ${dataBlock.heading}`}</VisuallyHidden>
                    </h3>

                    <p>Use our table tool to explore this data.</p>
                    <ExploreDataButton
                      block={dataBlock}
                      hiddenText={`for ${dataBlock.heading}`}
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
