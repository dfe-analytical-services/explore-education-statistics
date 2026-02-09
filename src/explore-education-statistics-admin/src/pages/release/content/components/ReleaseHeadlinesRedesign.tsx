import useGetChartFile from '@admin/hooks/useGetChartFile';
import ReleaseBlock from '@admin/pages/release/content/components/ReleaseBlock';
import { EditableRelease } from '@admin/services/releaseContentService';
import TabsSection from '@common/components/TabsSection';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';
import KeyStat, {
  KeyStatContainer,
} from '@common/modules/find-statistics/components/KeyStat';
import KeyStatDataBlock from '@common/modules/find-statistics/components/KeyStatDataBlock';
import ReleasePageContentSection from '@common/modules/find-statistics/components/ReleasePageContentSection';
import React from 'react';

interface Props {
  release: EditableRelease;
  transformFeaturedTableLinks?: (url: string, text: string) => void;
}

const ReleaseHeadlinesRedesign = ({
  release,
  transformFeaturedTableLinks,
}: Props) => {
  const getChartFile = useGetChartFile(release.id);

  const { headlinesSection, keyStatistics, keyStatisticsSecondarySection } =
    release;

  const summaryTab = (
    <>
      <KeyStatContainer>
        {keyStatistics?.map(keyStat => {
          if (keyStat.type === 'KeyStatisticDataBlock') {
            return (
              <KeyStatDataBlock
                key={keyStat.id}
                releaseVersionId={release.id}
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
        <div key={block.id} data-scroll={`editableSectionBlocks-${block.id}`}>
          <ReleaseBlock
            block={block}
            releaseVersionId={release.id}
            transformFeaturedTableLinks={transformFeaturedTableLinks}
          />
        </div>
      ))}
    </>
  );

  return (
    <ReleasePageContentSection
      className="dfe-print-break-before"
      heading="Headline facts and figures"
      id="headlines-section"
      testId="headlines-section"
      dataScrollId="releaseHeadlines-keyStatistics"
    >
      {!keyStatisticsSecondarySection.content.length ? (
        summaryTab
      ) : (
        <DataBlockTabs
          id="releaseHeadlines"
          releaseVersionId={release.id}
          getInfographic={getChartFile}
          dataBlock={keyStatisticsSecondarySection.content[0]}
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

export default ReleaseHeadlinesRedesign;
