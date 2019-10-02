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
          We're currently working on embedding a version of the table tool
          filtered to data uploaded from this release within the page. This will
          then allow the creation of data blocks and the ability to create
          charts and tables.
        </p>
        <p>
          While this work is in progress we have added a temporary table tool
          page that allows access to data you have uploaded.
        </p>
        <Link to="/prototypes/table-tool" target="_blank">
          Temporary admin table tool
        </Link>
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
