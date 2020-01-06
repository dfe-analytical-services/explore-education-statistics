import React from 'react';
import {
  AbstractRelease,
  Publication,
} from '@common/services/publicationService';
import DataBlock from '@common/modules/find-statistics/components/DataBlock';
import { EditableContentBlock } from '@admin/services/publicationService';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import ContentBlock from '@admin/modules/find-statistics/components/EditableContentBlock';

interface Props {
  release: AbstractRelease<EditableContentBlock, Publication>;
  setRelease?: (
    newRelease: AbstractRelease<EditableContentBlock, Publication>,
  ) => void;
}

const ReleaseHeadlines = ({ release, setRelease = () => {} }: Props) => {
  return (
    <>
      <h2 className="dfe-print-break-before">
        Headline facts and figures - {release.yearTitle}
      </h2>

      <Tabs id="releaseHeadlingsTabs">
        <TabsSection id="headline-summary" title="Summary">
          {release.keyStatisticsSection &&
            release.keyStatisticsSection.content &&
            release.keyStatisticsSection.content.map(datablock => (
              <DataBlock {...datablock} key={datablock.id} id="keystats" />
            ))}

          {release.headlinesSection && (
            <ContentBlock
              sectionId={release.headlinesSection.id}
              publication={release.publication}
              id={release.headlinesSection.id as string}
              content={release.headlinesSection.content}
              canAddSingleBlock
              onContentChange={newContent =>
                setRelease({
                  ...release,
                  headlinesSection: {
                    ...release.headlinesSection,
                    content: newContent,
                  },
                })
              }
            />
          )}
        </TabsSection>
      </Tabs>
    </>
  );
};

export default ReleaseHeadlines;
