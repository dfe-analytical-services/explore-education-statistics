import React, { useContext } from 'react';
import {
  AbstractRelease,
  Publication,
} from '@common/services/publicationService';
import DataBlock from '@common/modules/find-statistics/components/DataBlock';
import { EditableContentBlock } from '@admin/services/publicationService';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import ContentBlocks from '@admin/modules/find-statistics/components/EditableContentBlocks';
import getKeyStatisticsSecondaryTabs from './KeyStatisticsSecondary';

interface Props {
  release: AbstractRelease<EditableContentBlock, Publication>;
  setRelease: (
    newRelease: AbstractRelease<EditableContentBlock, Publication>,
  ) => void;
}

const ReleaseHeadlines = ({ release, setRelease = () => {} }: Props) => {
  const { isEditing } = useContext(EditingContext);
  return (
    <section id="headlines">
      <h2 className="dfe-print-break-before">
        Headline facts and figures - {release.yearTitle}
      </h2>
      <Tabs id="releaseHeadlingsTabs">
        <TabsSection id="headline-summary" title="Summary">
          <section id="keystats">
            {release.keyStatisticsSection &&
              release.keyStatisticsSection.content &&
              release.keyStatisticsSection.content.map(datablock => (
                <DataBlock {...datablock} key={datablock.id} id="keystats" />
              ))}
          </section>
          <section id="headlines">
            {release.headlinesSection && (
              <ContentBlocks
                sectionId={release.headlinesSection.id}
                publication={release.publication}
                id={release.headlinesSection.id as string}
                content={release.headlinesSection.content}
                canAddSingleBlock
                textOnly
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
          </section>
        </TabsSection>

        {getKeyStatisticsSecondaryTabs({ release, setRelease, isEditing })}
      </Tabs>
    </section>
  );
};

export default ReleaseHeadlines;
