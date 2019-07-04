import React, { ReactNode } from 'react';
import PreviousNextLinks, {
  PreviousNextLink,
} from '@admin/components/PreviousNextLinks';
import NavLink from '@admin/components/NavLink';
import Page from '../../../components/Page';

interface Props {
  releaseId: string;
  children: ReactNode;
  publicationTitle: string;
  previousLink?: PreviousNextLink;
  nextLink?: PreviousNextLink;
}

const EditReleasePageTemplate = ({
  releaseId,
  publicationTitle,
  children,
  previousLink,
  nextLink,
}: Props) => {
  return (
    <Page
      wide
      breadcrumbs={[
        {
          link: '/admin-dashboard',
          name: 'Administrator dashboard',
        },
        { name: 'Edit release', link: '#' },
      ]}
    >
      <h1 className="govuk-heading-l">
        {publicationTitle}
        <span className="govuk-caption-l">Edit release</span>
      </h1>
      <nav className="app-navigation govuk-!-margin-bottom-9">
        <ul className="app-navigation__list govuk-!-margin-bottom-0">
          <li>
            <NavLink
              to={`/edit-release/${releaseId}/setup`}
              activeClassName="app-navigation--current-page"
            >
              Release setup
            </NavLink>
            <NavLink
              to={`/edit-release/${releaseId}/data`}
              activeClassName="app-navigation--current-page"
            >
              Add / edit data
            </NavLink>
            <NavLink
              to={`/edit-release/${releaseId}/build-tables`}
              activeClassName="app-navigation--current-page"
            >
              Build tables
            </NavLink>
            <NavLink
              to={`/edit-release/${releaseId}/tables`}
              activeClassName="app-navigation--current-page"
            >
              View / edit tables
            </NavLink>
            <NavLink
              to={`/edit-release/${releaseId}/content`}
              activeClassName="app-navigation--current-page"
            >
              Add / edit content
            </NavLink>
            <NavLink
              to={`/edit-release/${releaseId}/publish-status`}
              activeClassName="app-navigation--current-page"
            >
              Set publish status
            </NavLink>
          </li>
        </ul>
      </nav>
      {children}
      <PreviousNextLinks
        previousSection={previousLink}
        nextSection={nextLink}
      />
    </Page>
  );
};

export default EditReleasePageTemplate;
