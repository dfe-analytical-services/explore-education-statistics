import React, { ReactNode } from 'react';
import DfeNavLink from '@admin/components/DfeNavLink';
import PreviousNextLinks, {
  PreviousNextLink,
} from '@admin/components/PreviousNextLinks';
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
            <DfeNavLink
              to={`/edit-release/${releaseId}/setup`}
              activeClassName="app-navigation--current-page"
            >
              Release setup
            </DfeNavLink>
            <DfeNavLink
              to={`/edit-release/${releaseId}/data`}
              activeClassName="app-navigation--current-page"
            >
              Add / edit data
            </DfeNavLink>
            <DfeNavLink
              to={`/edit-release/${releaseId}/build-tables`}
              activeClassName="app-navigation--current-page"
            >
              Build tables
            </DfeNavLink>
            <DfeNavLink
              to={`/edit-release/${releaseId}/tables`}
              activeClassName="app-navigation--current-page"
            >
              View / edit tables
            </DfeNavLink>
            <DfeNavLink
              to={`/edit-release/${releaseId}/content`}
              activeClassName="app-navigation--current-page"
            >
              Add / edit content
            </DfeNavLink>
            <DfeNavLink
              to={`/edit-release/${releaseId}/publish-status`}
              activeClassName="app-navigation--current-page"
            >
              Set publish status
            </DfeNavLink>
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
