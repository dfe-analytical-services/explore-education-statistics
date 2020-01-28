import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import ContentSubBlockRenderer from '@common/modules/find-statistics/components/ContentSubBlockRenderer';
import KeyStatTile from '@common/modules/find-statistics/components/KeyStatTile';
import styles from '@common/modules/find-statistics/components/SummaryRenderer.module.scss';
import { DataBlock } from '@common/services/dataBlockService';
import { Publication, Release } from '@common/services/publicationService';
import React from 'react';
import DataBlockRenderer from '@common/modules/find-statistics/components/DataBlockRenderer';

interface Props
  extends Pick<
    Release,
    | 'keyStatisticsSection'
    | 'keyStatisticsSecondarySection'
    | 'headlinesSection'
  > {
  publication: Publication;
}

const HeadlinesSection = ({
  publication,
  keyStatisticsSection,
  headlinesSection,
  keyStatisticsSecondarySection,
}: Props) => {
  return (
    <Tabs id="headlines-section">
      <TabsSection title="Summary">
        <div className={styles.keyStatsContainer}>
          {keyStatisticsSection.content &&
            keyStatisticsSection.content.map(keyStat => {
              if (keyStat.type !== 'DataBlock') return null;
              const block = keyStat as DataBlock;
              return <KeyStatTile key={block.id} {...block} />;
            })}
        </div>

        {(headlinesSection.content || [])
          .sort((a, b) => {
            if (a.order === undefined || b.order === undefined) {
              return 0;
            }
            return a.order - b.order;
          })
          .map((block, i) => (
            <ContentSubBlockRenderer
              key={block.id}
              id={`headlines-section-${i}`}
              publication={publication}
              block={block}
            />
          ))}
      </TabsSection>
      {keyStatisticsSecondarySection &&
        keyStatisticsSecondarySection.content &&
        keyStatisticsSecondarySection.content[0] && (
          <TabsSection title="Table">
            <DataBlockRenderer
              datablock={keyStatisticsSecondarySection.content[0] as DataBlock}
              renderType="table"
            />
          </TabsSection>
        )}
      {keyStatisticsSecondarySection &&
        keyStatisticsSecondarySection.content &&
        keyStatisticsSecondarySection.content[0] &&
        keyStatisticsSecondarySection.content[0].charts &&
        keyStatisticsSecondarySection.content[0].charts[0] && (
          <TabsSection title="Chart">
            <DataBlockRenderer
              datablock={keyStatisticsSecondarySection.content[0] as DataBlock}
              renderType="chart"
            />
          </TabsSection>
        )}
    </Tabs>
  );
};

export default HeadlinesSection;
