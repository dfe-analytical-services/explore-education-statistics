import TabsSection from '@common/components/TabsSection';
import useGetChartFile from '@common/modules/charts/hooks/useGetChartFile';
import ContentBlockRenderer from '@common/modules/find-statistics/components/ContentBlockRenderer';
import DataBlockRenderer from '@common/modules/find-statistics/components/DataBlockRenderer';
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
    keyStatisticsSecondarySection,
    keyStatisticsSection,
    headlinesSection,
    publication,
    slug,
  },
}: Props) => {
  const getChartFile = useGetChartFile(publication.slug, slug);

  return (
    <DataBlockRenderer
      id="releaseHeadlines-dataBlock"
      getInfographic={getChartFile}
      dataBlock={keyStatisticsSecondarySection.content?.[0]}
      firstTabs={
        <TabsSection title="Summary">
          <KeyStatTileContainer>
            {keyStatisticsSection.content.map(block => {
              if (block.type !== 'DataBlock') {
                return null;
              }

              return (
                <KeyStatTileColumn key={block.id}>
                  <KeyStatTile
                    query={block.dataBlockRequest}
                    summary={block.summary}
                    queryOptions={{
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
      }
    />
  );
};

export default PublicationReleaseHeadlinesSection;
