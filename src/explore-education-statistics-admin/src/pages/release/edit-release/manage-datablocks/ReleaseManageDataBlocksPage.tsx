import Link from '@admin/components/Link';
import React from 'react';
import TabsSection from '@common/components/TabsSection';
import Tabs from '@common/components/Tabs';
import CreateDataBlocks from './CreateDataBlocks';
import ViewDataBlocks from './ViewDataBlocks';

const ReleaseManageDataBlocksPage = () => {
  return (
    <>
      <div className="govuk-inset-text">
        <h3>This functionality is still in development.</h3>
        <p>
          Functionality to go through the creation process via the table has
          been added, however the ability to save data blocks has not yet been
          implemented.
        </p>
        <p>
          Work is also ongoing to remove the publication step from the table
          tool below and we are working to display the list of subjects specific
          to the release you are viewing. Currently this will display subjects
          only for the last created release on the publicaiton.
        </p>
      </div>

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
