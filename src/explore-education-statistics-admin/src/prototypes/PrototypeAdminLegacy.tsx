import PageTitle from '@admin/components/PageTitle';
import Link from '@admin/components/Link';
import PrototypePage from '@admin/prototypes/components/PrototypePage';
import RelatedInformation from '@common/components/RelatedInformation';
import Nav from '@admin/prototypes/components/PrototypeNavBarPublication';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import React from 'react';

const PrototypeManagePublication = () => {
  const dfeLinkWarning = {
    color: '#d4351c',
  };

  return (
    <PrototypePage
      wide
      breadcrumbs={[
        { name: 'Dashboard', link: '/prototypes/admin-dashboard' },
        { name: 'Manage publication', link: '#' },
      ]}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <PageTitle
            title="Pupil absence in schools in England"
            caption="Manage publication"
          />
        </div>
        <div className="govuk-grid-column-one-third">
          <RelatedInformation heading="Help and guidance">
            <ul className="govuk-list">
              <li>
                <Link to="/contact-us" target="_blank">
                  Contact us
                </Link>
              </li>
            </ul>
          </RelatedInformation>
        </div>
      </div>

      <Nav />

      <h3 className="govuk-heading-l">Legacy releases</h3>
      <p>
        Legacy releases will be displayed in descending order on the
        publication.
      </p>

      <div style={{ width: '100%', overflow: 'auto' }}>
        <table className="govuk-table">
          <caption className="govuk-table__caption--m">
            Legacy release order
          </caption>
          <thead className="govuk-table__head">
            <tr className="govuk-table__row">
              <th style={{ width: '5%' }}>Order</th>
              <th style={{ width: '20%' }}>Description</th>
              <th style={{ width: '65%' }}>URL</th>
              <th colSpan={2}>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td>2</td>
              <td>Academic year 2015/16</td>
              <td>
                <a href="#">
                  https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2015-to-2016
                </a>
              </td>
              <td>
                <a href="#">Edit</a>
              </td>
              <td>
                <a href="#" style={dfeLinkWarning}>
                  Delete
                </a>
              </td>
            </tr>
            <tr>
              <td>1</td>
              <td>Academic year 2014/15</td>
              <td>
                <a href="#">
                  https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2014-to-2015
                </a>
              </td>
              <td>
                <a href="#">Edit</a>
              </td>
              <td>
                <a href="#" style={dfeLinkWarning}>
                  Delete
                </a>
              </td>
            </tr>
            <tr>
              <td>0</td>
              <td>Academic year 2013/14</td>
              <td>
                <a href="#">
                  https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2013-to-2014
                </a>
              </td>
              <td>
                <a href="#">Edit</a>
              </td>
              <td>
                <a href="#" style={dfeLinkWarning}>
                  Delete
                </a>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <ButtonGroup>
        <Button>Create legacy release</Button>
        <Button variant="secondary">Reorder legacy releases</Button>
      </ButtonGroup>
    </PrototypePage>
  );
};

export default PrototypeManagePublication;
