import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import useGetChartFile from '@common/modules/charts/hooks/useGetChartFile';
import ContentBlockRenderer from '@common/modules/find-statistics/components/ContentBlockRenderer';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';
import KeyStat, {
  KeyStatContainer,
} from '@common/modules/find-statistics/components/KeyStat';
import { Release } from '@common/services/publicationService';
import orderBy from 'lodash/orderBy';
import React from 'react';

interface Props {
  release: Release;
}

const PublicationReleaseHeadlinesSection = ({
  release: {
    id,
    keyStatisticsSecondarySection,
    keyStatisticsSection,
    headlinesSection,
    publication,
    slug,
  },
}: Props) => {
  const getChartFile = useGetChartFile(publication.slug, slug);

  const summaryTab = (
    <TabsSection title="Summary" id="releaseHeadlines-summary">
      <KeyStatContainer>
        {keyStatisticsSection.content.map(block => {
          if (block.type !== 'DataBlock') {
            return null;
          }

          return (
            <KeyStat
              key={block.id}
              releaseId={id}
              dataBlockId={block.id}
              summary={block.summary}
            />
          );
        })}
      </KeyStatContainer>

      {orderBy(headlinesSection.content, 'order').map(block => (
        <ContentBlockRenderer key={block.id} block={block} />
      ))}
    </TabsSection>
  );

  if (!keyStatisticsSecondarySection.content.length) {
    return <Tabs id="releaseHeadlines">{summaryTab}</Tabs>;
  }

  return (
    <DataBlockTabs
      id="releaseHeadlines"
      releaseId={id}
      getInfographic={getChartFile}
      dataBlock={keyStatisticsSecondarySection.content[0]}
      firstTabs={summaryTab}
    />
  );
};

export default PublicationReleaseHeadlinesSection;
