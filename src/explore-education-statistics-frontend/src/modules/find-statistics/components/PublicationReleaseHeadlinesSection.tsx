import glossaryService from '@frontend/services/glossaryService';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import useGetReleaseFile from '@common/modules/release/hooks/useGetReleaseFile';
import ContentBlockRenderer from '@common/modules/find-statistics/components/ContentBlockRenderer';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';
import KeyStat, {
  KeyStatContainer,
} from '@common/modules/find-statistics/components/KeyStat';
import {
  logEvent,
  logOutboundLink,
} from '@frontend/services/googleAnalyticsService';
import {
  KeyStatisticDataBlock,
  KeyStatisticText,
  Release,
} from '@common/services/publicationService';
import orderBy from 'lodash/orderBy';
import React from 'react';
import KeyStatDataBlock from '@common/modules/find-statistics/components/KeyStatDataBlock';

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
        {keyStatistics.map(keyStat => {
          if ((keyStat as KeyStatisticDataBlock).dataBlockId) {
            return (
              <KeyStatDataBlock
                key={keyStat.id}
                releaseId={releaseId}
                dataBlockId={(keyStat as KeyStatisticDataBlock).dataBlockId}
                trend={keyStat.trend}
                guidanceTitle={keyStat.guidanceTitle}
                guidanceText={keyStat.guidanceText}
              />
            );
          }

          return (
            <KeyStat key={keyStat.id} {...(keyStat as KeyStatisticText)} />
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
