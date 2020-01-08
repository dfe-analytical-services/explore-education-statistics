import React from 'react';
import {
  AbstractRelease,
  Publication,
} from '@common/services/publicationService';
import TabsSection from '@common/components/TabsSection';
import { EditableContentBlock } from '@admin/services/publicationService';

interface Props {
  release: AbstractRelease<EditableContentBlock, Publication>;
  setRelease: (
    newRelease: AbstractRelease<EditableContentBlock, Publication>,
  ) => void;
  isEditing?: boolean;
}

function renderAddSecondaryStats({
  release,
  setRelease,
  isEditing = false,
}: Props): JSX.Element[] {
  if (!isEditing) return [];
  return [
    <TabsSection
      key="add"
      id="headline-add-secondary"
      title="Add additional statistics"
    >
      Something here about adding a datablock
    </TabsSection>,
  ];
}

function renderSecondaryStats({
  release,
  setRelease,
  isEditing = false,
}: Props): JSX.Element[] {
  return [
    <TabsSection key="table" id="headline-secondary-table" title="Table">
      Table
    </TabsSection>,
    <TabsSection key="chart" id="headline-secondary-Chart" title="Chart">
      Chart
    </TabsSection>,
  ];
}

function getKeyStatisticsSecondaryTabs({
  release,
  setRelease,
  isEditing = false,
}: Props): JSX.Element[] {
  //returns a set of tabs based on what has previously added to the release
  const { keyStatisticsSecondarySection } = release;

  function hasSecondaryStats() {
    return (
      keyStatisticsSecondarySection &&
      keyStatisticsSecondarySection.content &&
      keyStatisticsSecondarySection.content.length
    );
  }

  return hasSecondaryStats()
    ? renderSecondaryStats({ release, setRelease, isEditing })
    : renderAddSecondaryStats({ release, setRelease, isEditing });
}

export default getKeyStatisticsSecondaryTabs;
