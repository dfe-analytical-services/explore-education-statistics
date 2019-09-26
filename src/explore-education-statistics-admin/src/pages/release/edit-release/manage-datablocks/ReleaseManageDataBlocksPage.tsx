import React from 'react';
import TabsSection from '@common/components/TabsSection';
import Tabs from '@common/components/Tabs';
import CreateDataBlocks from './CreateDataBlocks';
import ViewDataBlocks from './ViewDataBlocks';

const ReleaseManageDataBlocksPage = () => {
  return (
    <>
      <Tabs id="manageDataBlocks">
        <TabsSection title="Create data blocks">
          <CreateDataBlocks />
        </TabsSection>
        <TabsSection title="View datablocks">
          <ViewDataBlocks />
        </TabsSection>
      </Tabs>
    </>
  );
};

export default ReleaseManageDataBlocksPage;
