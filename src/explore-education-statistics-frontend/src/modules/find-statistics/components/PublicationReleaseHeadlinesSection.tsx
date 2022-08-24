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
  },
}: Props) => {
  const getReleaseFile = useGetReleaseFile(id);

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
      releaseId={id}
      getInfographic={getReleaseFile}
      dataBlock={keyStatisticsSecondarySection.content[0]}
      firstTabs={summaryTab}
    />
  );
};

export default PublicationReleaseHeadlinesSection;
