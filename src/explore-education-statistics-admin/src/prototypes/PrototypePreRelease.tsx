import PageTitle from '@admin/components/PageTitle';
import PrototypePage from '@admin/prototypes/components/PrototypePage';
import Tabs from '@common/components/Tabs';
import RelatedAside from '@common/components/RelatedAside';
import TabsSection from '@common/components/TabsSection';
import React from 'react';
import NavBar from './components/PrototypeNavBar';
import CreatePreRelease from './components/PrototypePreReleaseCreate';
import ManagePreRelease from './components/PrototypeManagePreRelease';

const PrototypePreReleasePage = () => {
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

      <span className="govuk-tag govuk-!-margin-right-3">Approved</span>
      <span className="govuk-tag">Scheduled</span>

      <NavBar />

      <Tabs id="test">
        <TabsSection title="Pre-release access">
          <ManagePreRelease />
        </TabsSection>
        <TabsSection title="Public pre-release list">
          <CreatePreRelease />
        </TabsSection>
      </Tabs>
    </PrototypePage>
  );
};

export default PrototypePreReleasePage;
