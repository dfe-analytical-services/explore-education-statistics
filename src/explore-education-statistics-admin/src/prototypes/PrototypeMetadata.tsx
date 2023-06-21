import PageTitle from '@admin/components/PageTitle';
import PrototypePage from '@admin/prototypes/components/PrototypePage';
import React from 'react';
import Tabs from '@common/components/Tabs';
import RelatedAside from '@common/components/RelatedAside';
import TabsSection from '@common/components/TabsSection';
import NavBar from './components/PrototypeNavBar';
import CreateMeta from './components/PrototypeMetaCreate';
import DataReplace from './components/PrototypeDataReplace';

const PrototypeMetadata = () => {
  return (
    <PrototypePage wide>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <PageTitle title="An example publication" caption="Edit release" />
        </div>
        <div className="govuk-grid-column-one-third">
          <RelatedAside>
            <h2 className="govuk-heading-m">Help and guidance</h2>
            <ul className="govuk-list">
              <li>
                <a href="#" className="govuk-link">
                  Creating a new release
                </a>
              </li>
            </ul>
          </RelatedAside>
        </div>
      </div>

      <span className="govuk-tag">Draft</span>

      <NavBar />

      <Tabs id="test">
        <TabsSection title="Data uploads">
          <DataReplace />
        </TabsSection>
        <TabsSection title="Footnotes">
          This is the content for tab 2
        </TabsSection>
        <TabsSection title="Ancillary file uploads">
          This is the content for tab 2
        </TabsSection>
        <TabsSection title="Public metadata">
          <CreateMeta />
        </TabsSection>
      </Tabs>
    </PrototypePage>
  );
};

export default PrototypeMetadata;
