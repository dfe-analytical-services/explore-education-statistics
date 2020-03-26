import TabsSection from '@common/components/TabsSection';
import ContentBlockRenderer from '@common/modules/find-statistics/components/ContentBlockRender';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';
import KeyStatTile from '@common/modules/find-statistics/components/KeyStatTile';
import styles from '@common/modules/find-statistics/components/SummaryRenderer.module.scss';
import { Release } from '@common/services/publicationService';
import { DataBlock } from '@common/services/types/blocks';
import orderBy from 'lodash/orderBy';
import React from 'react';

interface Props
  extends Pick<
    Release,
    | 'keyStatisticsSection'
    | 'keyStatisticsSecondarySection'
    | 'headlinesSection'
  > {
  releaseId: string;
}

const PublicationReleaseHeadlinesSection = ({
  releaseId,
  keyStatisticsSection,
  headlinesSection,
  keyStatisticsSecondarySection,
}: Props) => {
  return (
    <DataBlockTabs
      id="releaseHeadlines-tabs"
      releaseId={releaseId}
      dataBlock={keyStatisticsSecondarySection.content?.[0]}
      firstTabs={
        <TabsSection title="Summary">
          <div className={styles.keyStatsContainer}>
            {keyStatisticsSection.content.map(keyStat => {
              if (keyStat.type !== 'DataBlock') {
                return null;
              }

              const block = keyStat as DataBlock;

              return <KeyStatTile key={block.id} {...block} />;
            })}
          </div>

          {orderBy(headlinesSection.content, 'order').map(block => (
            <ContentBlockRenderer key={block.id} block={block} />
          ))}
        </TabsSection>
      }
    />
  );
};

export default PublicationReleaseHeadlinesSection;
