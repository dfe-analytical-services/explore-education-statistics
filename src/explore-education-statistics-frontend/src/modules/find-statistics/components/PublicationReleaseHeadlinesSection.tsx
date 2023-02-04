import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import ContentBlockRenderer from '@common/modules/find-statistics/components/ContentBlockRenderer';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';
import KeyStat, {
  KeyStatContainer,
} from '@common/modules/find-statistics/components/KeyStat';
import KeyStatDataBlock from '@common/modules/find-statistics/components/KeyStatDataBlock';
import useGetReleaseFile from '@common/modules/release/hooks/useGetReleaseFile';
import { Release } from '@common/services/publicationService';
import glossaryService from '@frontend/services/glossaryService';
import {
  logEvent,
  logOutboundLink,
} from '@frontend/services/googleAnalyticsService';
import orderBy from 'lodash/orderBy';
import React from 'react';

interface Props {
  release: Release;
}

const PublicationReleaseHeadlinesSection = ({
  release: {
    id: releaseId,
    keyStatisticsSecondarySection,
    keyStatistics,
    headlinesSection,
  },
}: Props) => {
  const getReleaseFile = useGetReleaseFile(releaseId);

  const summaryTab = (
    <TabsSection title="Summary" id="releaseHeadlines-summary">
      <KeyStatContainer>
        {keyStatistics?.map(keyStat => {
          if (keyStat.type === 'KeyStatisticDataBlock') {
            return (
              <KeyStatDataBlock
                key={keyStat.id}
                releaseId={releaseId}
                dataBlockId={keyStat.dataBlockId}
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
          trackContentLinks={(url, newTab) => {
            logOutboundLink(
              `Publication release headlines link: ${url}`,
              url,
              newTab,
            );
          }}
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
      releaseId={releaseId}
      getInfographic={getReleaseFile}
      dataBlock={keyStatisticsSecondarySection.content[0]}
      firstTabs={summaryTab}
    />
  );
};

export default PublicationReleaseHeadlinesSection;
