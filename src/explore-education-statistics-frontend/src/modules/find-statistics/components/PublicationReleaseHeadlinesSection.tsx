import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import useGetChartFile from '@common/modules/charts/hooks/useGetChartFile';
import ContentBlockRenderer from '@common/modules/find-statistics/components/ContentBlockRenderer';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';
import KeyStatTile, {
  KeyStatTileColumn,
  KeyStatTileContainer,
} from '@common/modules/find-statistics/components/KeyStatTile';
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
    dataLastPublished,
  },
}: Props) => {
  const getChartFile = useGetChartFile(publication.slug, slug);

  const summaryTab = (
    <TabsSection title="Summary">
      <KeyStatTileContainer>
        {keyStatisticsSection.content.map(block => {
          if (block.type !== 'DataBlock') {
            return null;
          }

          return (
            <KeyStatTileColumn key={block.id}>
              <KeyStatTile
                query={{
                  releaseId: id,
                  ...block.query,
                }}
                summary={block.summary}
                queryOptions={{
                  dataLastPublished,
                  expiresIn: 60 * 60 * 24,
                }}
              />
            </KeyStatTileColumn>
          );
        })}
      </KeyStatTileContainer>

      {orderBy(headlinesSection.content, 'order').map(block => (
        <ContentBlockRenderer key={block.id} block={block} />
      ))}
    </TabsSection>
  );

  if (!keyStatisticsSecondarySection.content.length) {
    return <Tabs id="releaseHeadlines-dataBlock">{summaryTab}</Tabs>;
  }

  return (
    <DataBlockTabs
      releaseId={id}
      id="releaseHeadlines-dataBlock"
      getInfographic={getChartFile}
      dataBlock={keyStatisticsSecondarySection.content[0]}
      firstTabs={summaryTab}
      queryOptions={{
        dataLastPublished,
        expiresIn: 60 * 60 * 24,
      }}
    />
  );
};

export default PublicationReleaseHeadlinesSection;
