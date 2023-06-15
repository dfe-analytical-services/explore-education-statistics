import PageTitle from '@admin/components/PageTitle';
import Link from '@admin/components/Link';
import PrototypePage from '@admin/prototypes/components/PrototypePage';
import RelatedInformation from '@common/components/RelatedInformation';
import Nav from '@admin/prototypes/components/PrototypeNavBarPublication';
import Button from '@common/components/Button';
import Tag from '@common/components/Tag';
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

      <div className="govuk-!-width-full">
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-three-quarters">
            <h3 className="govuk-heading-l">Update release access</h3>
            <form>
              <label htmlFor="topic" className="govuk-label">
                Select release
              </label>
              <select name="topic" id="topic" className="govuk-select">
                <option value="">Academic year 2020/21 (Not live)</option>
                <option value="">
                  Academic year 2019/20 (Live - Latest release)
                </option>
                <option value="">Academic year 2018/19 (Live)</option>
                <option value="">Academic year 2017/18 (Live)</option>
              </select>
            </form>
          </div>
          <div className="govuk-grid-column-one-quarter dfe-align--right">
            <h4>Other options</h4>
            <ul className="govuk-list">
              <li>
                <a href="#">Invite new users</a>
              </li>
            </ul>
          </div>
        </div>

        <table className="govuk-table govuk-!-margin-top-9">
          <caption className="govuk-table__caption govuk-table__caption--m">
            Academic year 2020/21 (Not live) <Tag>DRAFT</Tag>
          </caption>
          <thead className="govuk-table__head">
            <tr className="govuk-table__row">
              <th>Name</th>
              <th className="govuk-table__cell--numeric">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td>Andrew Adams</td>
              <td className="govuk-table__cell--numeric">
                <a href="#" style={dfeLinkWarning}>
                  Remove
                </a>
              </td>
            </tr>
            <tr>
              <td>Ben Browne</td>
              <td className="govuk-table__cell--numeric">
                <a href="#" style={dfeLinkWarning}>
                  Remove
                </a>
              </td>
            </tr>
            <tr>
              <td>Charlotte Chesterton</td>
              <td className="govuk-table__cell--numeric">
                <a href="#" style={dfeLinkWarning}>
                  Remove
                </a>
              </td>
            </tr>
          </tbody>
        </table>
        <Button>Add or remove users</Button>
      </div>
    </PrototypePage>
  );
};

export default PrototypeManagePublication;
