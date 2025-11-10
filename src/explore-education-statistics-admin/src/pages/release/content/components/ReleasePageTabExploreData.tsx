import ReleasePageTabPanel from '@admin/pages/release/content/components/ReleasePageTabPanel';
import { useReleaseContentState } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import ReleasePageLayout from '@common/modules/release/components/ReleasePageLayout';
import React from 'react';

interface Props {
  hidden: boolean;
}

const ReleasePageTabExploreData = ({ hidden }: Props) => {
  const { release } = useReleaseContentState();

  return (
    <ReleasePageTabPanel tabKey="explore" hidden={hidden}>
      <ReleasePageLayout navItems={[]}>
        <h2>{release.title} - Data</h2>
      </ReleasePageLayout>
    </ReleasePageTabPanel>
  );
};

export default ReleasePageTabExploreData;
