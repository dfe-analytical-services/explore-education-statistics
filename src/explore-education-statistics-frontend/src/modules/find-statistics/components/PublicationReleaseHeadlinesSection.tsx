import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import ContentBlockRenderer from '@common/modules/find-statistics/components/ContentBlockRenderer';
import DataBlockTabs from '@frontend/modules/find-statistics/components/DataBlockTabs';
import KeyStat, {
  KeyStatContainer,
} from '@common/modules/find-statistics/components/KeyStat';
import KeyStatDataBlock from '@common/modules/find-statistics/components/KeyStatDataBlock';
import useGetReleaseFile from '@common/modules/release/hooks/useGetReleaseFile';
import { ReleaseVersion } from '@common/services/publicationService';
import glossaryService from '@frontend/services/glossaryService';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import orderBy from 'lodash/orderBy';
import React from 'react';

interface Props {
  releaseVersion: ReleaseVersion;
}

const PublicationReleaseHeadlinesSection = ({
  releaseVersion: {
    id: releaseVersionId,
    keyStatisticsSecondarySection,
    keyStatistics,
    headlinesSection,
  },
}: Props) => {
  const getReleaseFile = useGetReleaseFile(releaseVersionId);

  const summaryTab = (
    <TabsSection title="Summary" id="releaseHeadlines-summary">
      <KeyStatContainer>
        {keyStatistics?.map(keyStat => {
          if (keyStat.type === 'KeyStatisticDataBlock') {
            return (
              <KeyStatDataBlock
                key={keyStat.id}
                releaseVersionId={releaseVersionId}
                dataBlockParentId={keyStat.dataBlockParentId}
                trend={keyStat.trend}
                guidanceTitle={keyStat.guidanceTitle}
                guidanceText={keyStat.guidanceText}
              />
            );
          }

          return (
            <KeyStat
              key={keyStat.id}
              title={keyStat.title}
              statistic={keyStat.statistic}
              trend={keyStat.trend}
              guidanceTitle={keyStat.guidanceTitle}
              guidanceText={keyStat.guidanceText}
            />
          );
        })}
      </KeyStatContainer>

      {orderBy(headlinesSection.content, 'order').map(block => (
        <ContentBlockRenderer
          key={block.id}
          block={block}
          getGlossaryEntry={glossaryService.getEntry}
          trackGlossaryLinks={glossaryEntrySlug =>
            logEvent({
              category: `Publication Release Headlines Glossary Link`,
              action: `Glossary link clicked`,
              label: glossaryEntrySlug,
            })
          }
        />
      ))}
    </TabsSection>
  );

  if (!keyStatisticsSecondarySection.content.length) {
    return <Tabs id="releaseHeadlines">{summaryTab}</Tabs>;
  }

  return (
    <DataBlockTabs
      id="releaseHeadlines"
      releaseVersionId={releaseVersionId}
      getInfographic={getReleaseFile}
      dataBlock={keyStatisticsSecondarySection.content[0]}
      firstTabs={summaryTab}
    />
  );
};

export default PublicationReleaseHeadlinesSection;
