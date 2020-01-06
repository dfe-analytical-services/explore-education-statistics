import React from 'react';
import {
  AbstractRelease,
  Publication,
} from '@common/services/publicationService';
import DataBlock from '@common/modules/find-statistics/components/DataBlock';
import { EditableContentBlock } from '@admin/services/publicationService';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';

interface Props {
  release: AbstractRelease<EditableContentBlock, Publication>;
}

const ReleaseHeadlines = ({ release }: Props) => {
  return (
    <>
      <h2 className="dfe-print-break-before">
        Headline facts and figures - {release.yearTitle}
      </h2>

      <Tabs id="releaseHeadlingsTabs">
        <TabsSection id="headline-summary" title="Summary">
          {release.keyStatisticsSection &&
            release.keyStatisticsSection.content && (
              <DataBlock
                {...release.keyStatisticsSection.content[0]}
                id="keystats"
              />
            )}
          {release.headlinesSection && release.headlinesSection.content && (
            <>
              {console.log(release)}
              HEADLINE SECTION
            </>
          )}
        </TabsSection>
      </Tabs>
    </>
  );
};

export default ReleaseHeadlines;
