import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import BlockRenderer from '@common/modules/find-statistics/components/BlockRenderer';
import DataBlockRenderer from '@common/modules/find-statistics/components/DataBlockRenderer';
import KeyStatTile from '@common/modules/find-statistics/components/KeyStatTile';
import styles from '@common/modules/find-statistics/components/SummaryRenderer.module.scss';
import { Publication, Release } from '@common/services/publicationService';
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
  publication: Publication;
}

const PublicationReleaseHeadlinesSection = ({
  publication,
  keyStatisticsSection,
  headlinesSection,
  keyStatisticsSecondarySection,
}: Props) => {
  return (
    <Tabs id="headlines-section">
      <TabsSection title="Summary">
        <div className={styles.keyStatsContainer}>
          {keyStatisticsSection.content.map(keyStat => {
            if (keyStat.type !== 'DataBlock') return null;
            const block = keyStat as DataBlock;
            return <KeyStatTile key={block.id} {...block} />;
          })}
        </div>

        {orderBy(headlinesSection.content, 'order').map((block, i) => (
          <BlockRenderer
            key={block.id}
            id={`headlines-section-${i}`}
            publication={publication}
            block={block}
          />
        ))}
      </TabsSection>

      {keyStatisticsSecondarySection.content?.[0] && (
        <TabsSection title="Table">
          <DataBlockRenderer
            dataBlock={keyStatisticsSecondarySection.content[0]}
            type="table"
          />
        </TabsSection>
      )}

      {keyStatisticsSecondarySection.content?.[0]?.charts?.[0] && (
        <TabsSection title="Chart">
          <DataBlockRenderer
            dataBlock={keyStatisticsSecondarySection.content[0]}
            type="chart"
          />
        </TabsSection>
      )}
    </Tabs>
  );
};

export default PublicationReleaseHeadlinesSection;
