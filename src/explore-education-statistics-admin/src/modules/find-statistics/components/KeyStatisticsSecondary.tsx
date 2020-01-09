import React, { useContext } from 'react';
import {
  AbstractRelease,
  Publication,
  ContentSection,
} from '@common/services/publicationService';
import TabsSection from '@common/components/TabsSection';
import { EditableContentBlock } from '@admin/services/publicationService';
import Button from '@common/components/Button';
import {
  EditingContext,
  ReleaseContentContext,
} from '@common/modules/find-statistics/util/wrapEditableComponent';

interface Props {
  release: AbstractRelease<EditableContentBlock, Publication>;
  setRelease: (
    newRelease: AbstractRelease<EditableContentBlock, Publication>,
  ) => void;
  isEditing?: boolean;
  updating?: boolean;
}

export function hasSecondaryStats(
  keyStatisticsSecondarySection:
    | ContentSection<EditableContentBlock>
    | undefined,
) {
  return !!(
    keyStatisticsSecondarySection &&
    keyStatisticsSecondarySection.content &&
    keyStatisticsSecondarySection.content.length
  );
}

// function renderAddSecondaryStats({
//   release,
//   setRelease,
//   isEditing = false,
// }: Props): JSX.Element[] {
//   if (!isEditing) return [];
//   return [
//     <TabsSection
//       key="add"
//       id="headline-add-secondary"
//       title="Add additional statistics"
//     >
//       Something here about adding a datablock
//     </TabsSection>,
//   ];
// }

function renderSecondaryStats({
  release,
  setRelease,
  isEditing = false,
}: Props): JSX.Element[] {
  const { keyStatisticsSecondarySection } = release;
  if (!hasSecondaryStats(keyStatisticsSecondarySection)) {
    return [];
  }

  // @ts-ignore - it will not be undefined due to above check
  const secondaryStatsDatablock = keyStatisticsSecondarySection.content[0];

  return [
    <TabsSection key="table" id="headline-secondary-table" title="Table">
      Table
      {secondaryStatsDatablock}
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

  return hasSecondaryStats(keyStatisticsSecondarySection)
    ? renderSecondaryStats({ release, setRelease, isEditing })
    : [];
}

export const AddSecondaryStats = ({
  release,
  setRelease,
  isEditing,
  updating = false,
}: Props) => {
  if (!isEditing) return null;
  return (
    <>
      <Button
        className="govuk-!-margin-top-4 govuk-!-margin-bottom-4"
        onClick={() => {}}
      >
        {updating ? 'Change' : 'Add'} Secondary Stats
      </Button>
    </>
  );
};

export default getKeyStatisticsSecondaryTabs;
