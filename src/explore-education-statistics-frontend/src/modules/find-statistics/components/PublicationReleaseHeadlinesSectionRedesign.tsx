import TabsSection from '@common/components/TabsSection';
import ContentBlockRenderer from '@common/modules/find-statistics/components/ContentBlockRenderer';
import KeyStat, {
  KeyStatContainer,
} from '@common/modules/find-statistics/components/KeyStat';
import KeyStatDataBlock from '@common/modules/find-statistics/components/KeyStatDataBlock';
import ReleasePageContentSection from '@common/modules/find-statistics/components/ReleasePageContentSection';
import useGetReleaseFile from '@common/modules/release/hooks/useGetReleaseFile';
import { ReleaseVersionHomeContent } from '@common/services/publicationService';
import DataBlockTabs from '@frontend/modules/find-statistics/components/DataBlockTabs';
import glossaryService from '@frontend/services/glossaryService';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import React from 'react';

const PublicationReleaseHeadlinesSection = ({
  headlinesSection,
  keyStatisticsSecondarySection,
  keyStatistics,
  releaseVersionId,
}: Pick<
  ReleaseVersionHomeContent,
  'headlinesSection' | 'keyStatistics' | 'keyStatisticsSecondarySection'
> & {
  releaseVersionId: string;
}) => {
  const getReleaseFile = useGetReleaseFile(releaseVersionId);

  const secondaryKeyStatsDataBlock = keyStatisticsSecondarySection.content[0];

  const summaryTab = (
    <>
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
                isRedesignStyle
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
              isRedesignStyle
            />
          );
        })}
      </KeyStatContainer>

      {headlinesSection.content.map(block => (
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
    </>
  );

  return (
    <ReleasePageContentSection
      heading="Headline facts and figures"
      id="headlines-section"
      testId="headlines-section"
    >
      {!keyStatisticsSecondarySection.content.length ? (
        summaryTab
      ) : (
        <DataBlockTabs
          id="releaseHeadlines"
          releaseVersionId={releaseVersionId}
          getInfographic={getReleaseFile}
          dataBlock={{
            id: secondaryKeyStatsDataBlock.id,
            type: secondaryKeyStatsDataBlock.type,
            ...secondaryKeyStatsDataBlock.dataBlockVersion,
          }}
          dataBlockStaleTime={Infinity}
          firstTabs={
            <TabsSection title="Summary" id="releaseHeadlines-summary">
              {summaryTab}
            </TabsSection>
          }
        />
      )}
    </ReleasePageContentSection>
  );
};

export default PublicationReleaseHeadlinesSection;
