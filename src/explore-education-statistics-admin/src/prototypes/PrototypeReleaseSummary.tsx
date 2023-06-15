import PageTitle from '@admin/components/PageTitle';
import PrototypePage from '@admin/prototypes/components/PrototypePage';
import RelatedAside from '@common/components/RelatedAside';
import Button from '@common/components/Button';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Details from '@common/components/Details';
import Tag from '@common/components/Tag';
import InsetText from '@common/components/InsetText';
import React from 'react';
import NavBar from './components/PrototypeNavBar';
import styles from './PrototypePublicPage.module.scss';

const PrototypeReleaseSummary = () => {
  return (
    <div className={styles.prototypeAdminPage}>
      <PrototypePage
        wide
        breadcrumbs={[
          { name: 'Dashboard', link: '/dashboard' },
          { name: 'Manage publication', link: '/prototypes/admin-publication' },
          {
            name: 'Pupil absence in schools in England',
            link: '/admin-publication',
          },
        ]}
      >
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <PageTitle
              title="Pupil absence in schools in England"
              caption="Edit release for academic year 2021/22"
            />
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

        <div className="dfe-flex">
          <div>
            <Tag>Draft</Tag>
          </div>
          <Details
            summary="{errorTally}"
            className="govuk-!-margin-bottom-0 govuk-!-margin-left-3"
          >
            <div className="govuk-grid-row">
              <div className="govuk-grid-column-one-third">
                <InsetText variant="error" className="govuk-!-margin-0">
                  <h2 className="govuk-heading-s govuk-!-margin-top-0">
                    <Tag colour="red">2 Errors</Tag>
                  </h2>
                  <p>
                    <strong>2 issues</strong> that must be resolved before this
                    release can be published.
                  </p>
                  <ul>
                    <li>
                      <a href="#">All data imports must be completed</a>
                    </li>
                    <li>
                      <a href="#">
                        All summary information must be completed on the data
                        guidance page
                      </a>
                    </li>
                  </ul>
                </InsetText>
              </div>
              <div className="govuk-grid-column-one-third">
                <InsetText variant="warning" className="govuk-!-margin-0">
                  <h2 className="govuk-heading-s govuk-!-margin-top-0">
                    <Tag colour="orange">3 Warnings</Tag>
                  </h2>
                  <p>
                    <strong>3 things</strong> you may have forgotten, but do not
                    need to resolve to publish this release.
                  </p>
                  <ul>
                    <li>
                      <a href="#">All data imports must be completed</a>
                    </li>
                    <li>
                      <a href="#">
                        A methodology for this publication is not yet approved
                      </a>
                    </li>
                    <li>
                      <a href="#">1 data file does not have any footnotes</a>
                    </li>
                    <li>
                      <a href="#">
                        A methodology for this publication is not yet approved
                      </a>
                    </li>
                  </ul>
                </InsetText>
              </div>
              <div className="govuk-grid-column-one-third">
                <InsetText className="govuk-!-margin-0">
                  <h2 className="govuk-heading-s govuk-!-margin-top-0">
                    <Tag colour="grey">2 Unresolved comments</Tag>
                  </h2>
                  <p>
                    <strong>2 unresolved comments</strong> you may wish to
                    resolve to publish this release.
                  </p>
                  <ul>
                    <li>
                      <a href="#">Check comments</a>
                    </li>
                  </ul>
                </InsetText>
              </div>
            </div>
          </Details>
        </div>

        <NavBar />

        <h2>Release summary</h2>
        <p>
          These details will be shown to users to help identify this release.
        </p>

        <h3>Publication details</h3>
        <SummaryList>
          <SummaryListItem term="Publication title">
            Pupil absence in schools in England
          </SummaryListItem>
          <SummaryListItem term="Lead statistician">
            Sean Gibson
          </SummaryListItem>
        </SummaryList>

        <h3>Release summary</h3>
        <SummaryList>
          <SummaryListItem term="Time period">Academic year</SummaryListItem>
          <SummaryListItem term="Release period">2021/22</SummaryListItem>
          <SummaryListItem term="Release type">
            National statistics
          </SummaryListItem>
        </SummaryList>

        <Button>Edit release summary</Button>
      </PrototypePage>
    </div>
  );
};

export default PrototypeReleaseSummary;
