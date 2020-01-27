import React, { useState, useEffect } from 'react';
import { Release, Publication } from '@common/services/publicationService';
import TabsSection from '@common/components/TabsSection';
import Tabs from '@common/components/Tabs';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ContentSubBlockRenderer from '@common/modules/find-statistics/components/ContentSubBlockRenderer';

interface Props
  extends Pick<
    Release,
    | 'keyStatisticsSection'
    | 'keyStatisticsSecondarySection'
    | 'headlinesSection'
  > {
  releaseId: string;
  publication: Publication;
}

const HeadlinesSection = ({
  releaseId,
  publication,
  keyStatisticsSection,
  headlinesSection,
  keyStatisticsSecondarySection,
}: Props) => {
  const [loading, setLoading] = useState(false);
  useEffect(() => {}, [
    releaseId,
    keyStatisticsSection,
    headlinesSection,
    keyStatisticsSecondarySection,
  ]);

  return (
    <Tabs id={`${releaseId}-headlines-section`}>
      <TabsSection title="Summary">
        {loading ? (
          <LoadingSpinner text="Loading content..." />
        ) : (
          <>
            <ContentSubBlockRenderer
              id="headlines-section"
              publication={publication}
              block={
                headlinesSection.content &&
                headlinesSection.content[0] &&
                headlinesSection.content[0]
              }
            />
          </>
        )}
      </TabsSection>
      {!loading && keyStatisticsSecondarySection && (
        <TabsSection title="Table">Table</TabsSection>
      )}
      {!loading &&
        keyStatisticsSecondarySection &&
        keyStatisticsSecondarySection.content &&
        keyStatisticsSecondarySection.content[0] &&
        keyStatisticsSecondarySection.content[0].charts &&
        keyStatisticsSecondarySection.content[0].charts[0] && (
          <TabsSection title="Chart">
            {JSON.stringify(keyStatisticsSecondarySection.content[0].charts[0])}
          </TabsSection>
        )}
    </Tabs>
  );
};

export default HeadlinesSection;
